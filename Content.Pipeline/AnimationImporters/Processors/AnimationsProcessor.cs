#region License
//   Copyright 2011-2016 Kastellanos Nikolaos
//
//   Licensed under the Apache License, Version 2.0 (the "License");
//   you may not use this file except in compliance with the License.
//   You may obtain a copy of the License at
//
//       http://www.apache.org/licenses/LICENSE-2.0
//
//   Unless required by applicable law or agreed to in writing, software
//   distributed under the License is distributed on an "AS IS" BASIS,
//   WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
//   See the License for the specific language governing permissions and
//   limitations under the License.
#endregion

using System;
using System.Collections.Generic;
using System.ComponentModel;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content.Pipeline;
using Microsoft.Xna.Framework.Content.Pipeline.Graphics;
using Microsoft.Xna.Framework.Graphics;
using tainicom.Aether.Content.Pipeline.Animation;

namespace tainicom.Aether.Content.Pipeline.Processors
{
    [ContentProcessor(DisplayName = "Animation - Aether")]
    class AnimationsProcessor : ContentProcessor<NodeContent, AnimationsContent>
    {
        private int _maxBones = SkinnedEffect.MaxBones;
        private int _generateKeyframesFrequency = 0;
        private bool _fixRealBoneRoot = false;

#if !PORTABLE
        [DisplayName("MaxBones")]
#endif
        [DefaultValue(SkinnedEffect.MaxBones)]
        public virtual int MaxBones
        {
            get { return _maxBones; }
            set { _maxBones = value; }
        }

#if !PORTABLE
        [DisplayName("Generate Keyframes Frequency")]
#endif
        [DefaultValue(0)] // (0=no, 30=30fps, 60=60fps)
        public virtual int GenerateKeyframesFrequency
        {
            get { return _generateKeyframesFrequency; }
            set { _generateKeyframesFrequency = value; }
        }

#if !PORTABLE
        [DisplayName("Fix BoneRoot from MG importer")]
#endif
        [DefaultValue(false)]
        public virtual bool FixRealBoneRoot
        {
            get { return _fixRealBoneRoot; }
            set { _fixRealBoneRoot = value; }
        }

        public override AnimationsContent Process(NodeContent input, ContentProcessorContext context)
        {
            if(_fixRealBoneRoot)
                MGFixRealBoneRoot(input, context);

            ValidateMesh(input, context, null);

            // Find the skeleton.
            BoneContent skeleton = MeshHelper.FindSkeleton(input);

            if (skeleton == null)
                throw new InvalidContentException("Input skeleton not found.");

            // We don't want to have to worry about different parts of the model being
            // in different local coordinate systems, so let's just bake everything.
            FlattenTransforms(input, skeleton);

            // Read the bind pose and skeleton hierarchy data.
            IList<BoneContent> bones = MeshHelper.FlattenSkeleton(skeleton);

            if (bones.Count > MaxBones)
            {
                throw new InvalidContentException(string.Format("Skeleton has {0} bones, but the maximum supported is {1}.", bones.Count, MaxBones));
            }

            List<Matrix> bindPose = new List<Matrix>();
            List<Matrix> invBindPose = new List<Matrix>();
            List<int> skeletonHierarchy = new List<int>();
            List<string> boneNames = new List<string>();

            foreach(var bone in bones)
            {
                bindPose.Add(bone.Transform);
                invBindPose.Add(Matrix.Invert(bone.AbsoluteTransform));
                skeletonHierarchy.Add(bones.IndexOf(bone.Parent as BoneContent));
                boneNames.Add(bone.Name);
            }

            // Convert animation data to our runtime format.
            Dictionary<string, ClipContent> clips;
            clips = ProcessAnimations(input, context, skeleton.Animations, bones, GenerateKeyframesFrequency);

            return new AnimationsContent(bindPose, invBindPose, skeletonHierarchy, boneNames, clips);
        }
        
        /// <summary>
        /// MonoGame converts some NodeContent into BoneContent.
        /// Here we revert that to get the original Skeleton and  
        /// add the real boneroot to the root node.
        /// </summary>
        private void MGFixRealBoneRoot(NodeContent input, ContentProcessorContext context)
        {
            for (int i = input.Children.Count - 1; i >= 0; i--)
            {
                var node = input.Children[i];
                if (node is BoneContent &&
                    node.AbsoluteTransform == Matrix.Identity &&
                    node.Children.Count ==1 &&
                    node.Children[0] is BoneContent &&
                    node.Children[0].AbsoluteTransform == Matrix.Identity
                    )
                {
                    //dettach real boneRoot
                    var realBoneRoot = node.Children[0];
                    node.Children.RemoveAt(0);
                    //copy animation from node to boneRoot
                    foreach (var animation in node.Animations)
                        realBoneRoot.Animations.Add(animation.Key, animation.Value);
                    // convert fake BoneContent back to NodeContent
                    input.Children[i] = new NodeContent()
                    {
                        Name = node.Name,
                        Identity = node.Identity,
                        Transform = node.Transform,                        
                    };
                    foreach (var animation in node.Animations)
                        input.Children[i].Animations.Add(animation.Key, animation.Value);
                    foreach (var opaqueData in node.OpaqueData)
                        input.Children[i].OpaqueData.Add(opaqueData.Key, opaqueData.Value);
                    //attach real boneRoot to the root node
                    input.Children.Add(realBoneRoot);

                    break;
                }
            }
        }

        /// <summary>
        /// Makes sure this mesh contains the kind of data we know how to animate.
        /// </summary>
        void ValidateMesh(NodeContent node, ContentProcessorContext context, string parentBoneName)
        {
            MeshContent mesh = node as MeshContent;
            if (mesh != null)
            {
                // Validate the mesh.
                if (parentBoneName != null)
                {
                    context.Logger.LogWarning(null, null,
                        "Mesh {0} is a child of bone {1}. AnimatedModelProcessor " +
                        "does not correctly handle meshes that are children of bones.",
                        mesh.Name, parentBoneName);
                }

                if (!MeshHasSkinning(mesh))
                {
                    context.Logger.LogWarning(null, null,
                        "Mesh {0} has no skinning information, so it has been deleted.",
                        mesh.Name);

                    mesh.Parent.Children.Remove(mesh);
                    return;
                }
            }
            else if (node is BoneContent)
            {
                // If this is a bone, remember that we are now looking inside it.
                parentBoneName = node.Name;
            }

            // Recurse (iterating over a copy of the child collection,
            // because validating children may delete some of them).
            foreach (NodeContent child in new List<NodeContent>(node.Children))
                ValidateMesh(child, context, parentBoneName);
        }
        
        /// <summary>
        /// Checks whether a mesh contains skininng information.
        /// </summary>
        bool MeshHasSkinning(MeshContent mesh)
        {
            foreach (GeometryContent geometry in mesh.Geometry)
            {
                if (!geometry.Vertices.Channels.Contains(VertexChannelNames.Weights()) &&
                    !geometry.Vertices.Channels.Contains("BlendWeight0"))
                    return false;
            }

            return true;
        }
        
        /// <summary>
        /// Bakes unwanted transforms into the model geometry,
        /// so everything ends up in the same coordinate system.
        /// </summary>
        void FlattenTransforms(NodeContent node, BoneContent skeleton)
        {
            foreach (NodeContent child in node.Children)
            {
                // Don't process the skeleton, because that is special.
                if (child == skeleton)
                    continue;

                // Bake the local transform into the actual geometry.
                MeshHelper.TransformScene(child, child.Transform);

                // Having baked it, we can now set the local
                // coordinate system back to identity.
                child.Transform = Matrix.Identity;

                // Recurse.
                FlattenTransforms(child, skeleton);
            }
        }
        
        /// <summary>
        /// Converts an intermediate format content pipeline AnimationContentDictionary
        /// object to our runtime AnimationClip format.
        /// </summary>
        Dictionary<string, ClipContent> ProcessAnimations(NodeContent input, ContentProcessorContext context, AnimationContentDictionary animations, IList<BoneContent> bones, int generateKeyframesFrequency)
        {
            // Build up a table mapping bone names to indices.
            Dictionary<string, int> boneMap = new Dictionary<string, int>();

            for (int i = 0; i < bones.Count; i++)
            {
                string boneName = bones[i].Name;

                if (!string.IsNullOrEmpty(boneName))
                    boneMap.Add(boneName, i);
            }

            // Convert each animation in turn.
            Dictionary<string, ClipContent> animationClips;
            animationClips = new Dictionary<string, ClipContent>();

            foreach (KeyValuePair<string, AnimationContent> animation in animations)
            {
                ClipContent clip = ProcessAnimation(input, context, animation.Value, boneMap, generateKeyframesFrequency);

                animationClips.Add(animation.Key, clip);
            }

            if (animationClips.Count == 0)
            {
                //throw new InvalidContentException("Input file does not contain any animations.");
                context.Logger.LogWarning(null, null, "Input file does not contain any animations.");
            }

            return animationClips;
        }
        
        /// <summary>
        /// Converts an intermediate format content pipeline AnimationContent
        /// object to our runtime AnimationClip format.
        /// </summary>
        ClipContent ProcessAnimation(NodeContent input, ContentProcessorContext context, AnimationContent animation, Dictionary<string, int> boneMap, int generateKeyframesFrequency)
        {
            List<KeyframeContent> keyframes = new List<KeyframeContent>();

            // For each input animation channel.
            foreach (KeyValuePair<string, AnimationChannel> channel in
                animation.Channels)
            {
                // Look up what bone this channel is controlling.
                int boneIndex;

                if (!boneMap.TryGetValue(channel.Key, out boneIndex))
                {
                    //throw new InvalidContentException(string.Format("Found animation for bone '{0}', which is not part of the skeleton.", channel.Key));
                    context.Logger.LogWarning(null, null, "Found animation for bone '{0}', which is not part of the skeleton.", channel.Key);

                    continue;
                }

                foreach (AnimationKeyframe keyframe in channel.Value)
                    keyframes.Add(new KeyframeContent(boneIndex, keyframe.Time, keyframe.Transform));
            }

            // Sort the merged keyframes by time.
            keyframes.Sort(CompareKeyframeTimes);

            //System.Diagnostics.Debugger.Launch();
            if (generateKeyframesFrequency > 0)
                keyframes = InterpolateKeyframes(animation.Duration, keyframes, generateKeyframesFrequency);

            keyframes = EnsureFirstKeyframeExists(animation.Duration, keyframes);

            if (keyframes.Count == 0)
                throw new InvalidContentException("Animation has no keyframes.");

            if (animation.Duration <= TimeSpan.Zero)
                throw new InvalidContentException("Animation has a zero duration.");

            return new ClipContent(animation.Duration, keyframes.ToArray());
        }

        int CompareKeyframeTimes(KeyframeContent a, KeyframeContent b)
        {
            int cmpTime = a.Time.CompareTo(b.Time);
            if (cmpTime == 0)
                return a.Bone.CompareTo(b.Bone);

            return cmpTime;
        }

        private List<KeyframeContent> InterpolateKeyframes(TimeSpan duration, List<KeyframeContent> keyframes, int generateKeyframesFrequency)
        {
            if (generateKeyframesFrequency <= 0)
                return keyframes;

            int keyframeCount = keyframes.Count;

            // find bones
            HashSet<int> bonesSet = new HashSet<int>();
            int maxBone = 0;
            for (int i = 0; i < keyframeCount; i++)
            {
                int bone = keyframes[i].Bone;
                maxBone = Math.Max(maxBone, bone);
                bonesSet.Add(bone);
            }
            int boneCount = bonesSet.Count;

            // split bones 
            List<KeyframeContent>[] boneFrames = new List<KeyframeContent>[maxBone + 1];
            for (int i = 0; i < keyframeCount; i++)
            {
                int bone = keyframes[i].Bone;
                if (boneFrames[bone] == null) boneFrames[bone] = new List<KeyframeContent>();
                boneFrames[bone].Add(keyframes[i]);
            }

            //            
            System.Diagnostics.Debug.WriteLine("Duration: " + duration);
            System.Diagnostics.Debug.WriteLine("keyframeCount: " + keyframeCount);

            // generate first and last frame
            Matrix mBlend = Matrix.Identity;
            for (int b = 0; b < boneFrames.Length; b++)
            {
                if (boneFrames[b] == null)
                    continue;

                var keyframeA = boneFrames[b][0];
                var keyframeB = boneFrames[b][boneFrames[b].Count - 1];
                if (keyframeA.Time == TimeSpan.Zero && keyframeB.Time == duration)
                    continue;

                var diffA = keyframeA.Time;
                var diffB = duration - keyframeB.Time;
                float lerpAmount = (float)diffA.Ticks / (float)(diffA + diffB).Ticks;
                MatrixLerpDecomposition(ref keyframeA.Transform, ref keyframeB.Transform, lerpAmount, ref mBlend);

                if (keyframeA.Time != TimeSpan.Zero)
                {
                    var keyframe0 = new KeyframeContent(keyframeA.Bone, TimeSpan.Zero, mBlend);
                    boneFrames[b].Insert(0, keyframe0);
                }
                if (keyframeB.Time != TimeSpan.Zero)
                {
                    var keyframeZ = new KeyframeContent(keyframeA.Bone, duration, mBlend);
                    boneFrames[b].Add(keyframeZ);
                }
            }

            TimeSpan keySpan = TimeSpan.FromTicks((long)((1f / generateKeyframesFrequency) * TimeSpan.TicksPerSecond));
            for (int b = 0; b < boneFrames.Length; b++)
            {
                boneFrames[b] = InterpolateFramesBone(b, boneFrames[b], keySpan);
            }

            int frames = keyframeCount / boneCount;

            TimeSpan checkDuration = TimeSpan.FromSeconds((frames - 1) / generateKeyframesFrequency);
            if (duration == checkDuration) return keyframes;

            List<KeyframeContent> newKeyframes = new List<KeyframeContent>();
            for (int b = 0; b < boneFrames.Length; b++)
            {
                if (boneFrames[b] != null)
                {
                    for (int k = 0; k < boneFrames[b].Count; ++k)
                    {
                        newKeyframes.Add(boneFrames[b][k]);
                    }
                }
            }
            newKeyframes.Sort(CompareKeyframeTimes);

            return newKeyframes;
        }

        // the player currently requires all bones to have a keyframe at time zero
        private List<KeyframeContent> EnsureFirstKeyframeExists(TimeSpan duration, List<KeyframeContent> keyframes)
        {
            int keyframeCount = keyframes.Count;

            // find bones
            HashSet<int> bonesSet = new HashSet<int>();
            int maxBone = 0;
            for (int i = 0; i < keyframeCount; i++)
            {
                int bone = keyframes[i].Bone;
                maxBone = Math.Max(maxBone, bone);
                bonesSet.Add(bone);
            }
            int boneCount = bonesSet.Count;

            // split bones 
            List<KeyframeContent>[] boneFrames = new List<KeyframeContent>[maxBone + 1];
            for (int i = 0; i < keyframeCount; i++)
            {
                int bone = keyframes[i].Bone;
                if (boneFrames[bone] == null) boneFrames[bone] = new List<KeyframeContent>();
                boneFrames[bone].Add(keyframes[i]);
            }

            //            
            System.Diagnostics.Debug.WriteLine("Duration: " + duration);
            System.Diagnostics.Debug.WriteLine("keyframeCount: " + keyframeCount);


            for (int b = 0; b < boneFrames.Length; b++)
            {
                if (boneFrames[b] == null)
                    continue;
                                
                if (boneFrames[b][0].Time != TimeSpan.Zero)
                {
                    var keyframe0 = new KeyframeContent(boneFrames[b][0].Bone, TimeSpan.Zero, boneFrames[b][0].Transform);
                    boneFrames[b].Insert(0, keyframe0);
                }
            }

            List<KeyframeContent> newKeyframes = new List<KeyframeContent>();
            for (int b = 0; b < boneFrames.Length; b++)
            {
                if (boneFrames[b] != null)
                {
                    for (int k = 0; k < boneFrames[b].Count; ++k)
                    {
                        newKeyframes.Add(boneFrames[b][k]);
                    }
                }
            }
            newKeyframes.Sort(CompareKeyframeTimes);

            return newKeyframes;
        }

        private static List<KeyframeContent> InterpolateFramesBone(int bone, List<KeyframeContent> frames, TimeSpan keySpan)
        {
            System.Diagnostics.Debug.WriteLine("");
            System.Diagnostics.Debug.WriteLine("Bone: " + bone);
            if (frames == null)
            {
                System.Diagnostics.Debug.WriteLine("Frames: " + "null");
                return frames;
            }
            System.Diagnostics.Debug.WriteLine("Frames: " + frames.Count);
            System.Diagnostics.Debug.WriteLine("MinTime: " + frames[0].Time);
            System.Diagnostics.Debug.WriteLine("MaxTime: " + frames[frames.Count - 1].Time);

            for (int i = 0; i < frames.Count - 1; ++i)
            {
                InterpolateFrames(bone, frames, keySpan, i);
            }

            return frames;
        }

        private static void InterpolateFrames(int bone, List<KeyframeContent> frames, TimeSpan keySpan, int i)
        {
            int a = i;
            int b = i + 1;
            var diff = frames[b].Time - frames[a].Time;
            Matrix mBlend = Matrix.Identity;
            if (diff > keySpan)
            {
                TimeSpan newTime = frames[a].Time + keySpan;
                float amount = (float)(keySpan.TotalSeconds / diff.TotalSeconds);
                Matrix m1 = frames[a].Transform;
                Matrix m2 = frames[b].Transform;

                MatrixLerpDecomposition(ref m1, ref m2, amount, ref mBlend);

                frames.Insert(b, new KeyframeContent(bone, newTime, mBlend));
            }
            return;
        }

        private static void MatrixLerpPrecise(ref Matrix m1, ref Matrix m2, float amount, ref Matrix result)
        {
            float w1 = 1f - amount;
            float w2 = amount;
            result.M11 = (m1.M11 * w1) + (m2.M11 * w2);
            result.M12 = (m1.M12 * w1) + (m2.M12 * w2);
            result.M13 = (m1.M13 * w1) + (m2.M13 * w2);
            result.M21 = (m1.M21 * w1) + (m2.M21 * w2);
            result.M22 = (m1.M22 * w1) + (m2.M22 * w2);
            result.M23 = (m1.M23 * w1) + (m2.M23 * w2);
            result.M31 = (m1.M31 * w1) + (m2.M31 * w2);
            result.M32 = (m1.M32 * w1) + (m2.M32 * w2);
            result.M33 = (m1.M33 * w1) + (m2.M33 * w2);
            result.M41 = (m1.M41 * w1) + (m2.M41 * w2);
            result.M42 = (m1.M42 * w1) + (m2.M42 * w2);
            result.M43 = (m1.M43 * w1) + (m2.M43 * w2);
        }

        private static void MatrixLerpDecomposition(ref Matrix m1, ref Matrix m2, float amount, ref Matrix mBlend)
        {
            Vector3 pScale; Quaternion pRotation; Vector3 pTranslation;
            m1.Decompose(out pScale, out pRotation, out pTranslation);

            Vector3 iScale; Quaternion iRotation; Vector3 iTranslation;
            m2.Decompose(out iScale, out iRotation, out iTranslation);

            Vector3 Scale; Quaternion Rotation; Vector3 Translation;
            //lerp
            Vector3.Lerp(ref pScale, ref iScale, amount, out Scale);
            Quaternion.Slerp(ref pRotation, ref iRotation, amount, out Rotation);
            Vector3.Lerp(ref pTranslation, ref iTranslation, amount, out Translation);

            Matrix rotation;
            Matrix.CreateFromQuaternion(ref Rotation, out rotation);

            mBlend.M11 = Scale.X * rotation.M11;
            mBlend.M12 = Scale.X * rotation.M12;
            mBlend.M13 = Scale.X * rotation.M13;
            mBlend.M21 = Scale.Y * rotation.M21;
            mBlend.M22 = Scale.Y * rotation.M22;
            mBlend.M23 = Scale.Y * rotation.M23;
            mBlend.M31 = Scale.Z * rotation.M31;
            mBlend.M32 = Scale.Z * rotation.M32;
            mBlend.M33 = Scale.Z * rotation.M33;
            mBlend.M41 = Translation.X;
            mBlend.M42 = Translation.Y;
            mBlend.M43 = Translation.Z;
        }
        
    }
}
