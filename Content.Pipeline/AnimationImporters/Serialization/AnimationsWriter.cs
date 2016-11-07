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
using Microsoft.Xna.Framework.Content.Pipeline;
using Microsoft.Xna.Framework.Content.Pipeline.Serialization.Compiler;
using tainicom.Aether.Content.Pipeline.Animation;

namespace tainicom.Aether.Content.Pipeline.Serialization
{   
    [ContentTypeWriter]
    class AnimationsDataWriter : ContentTypeWriter<AnimationsContent>
    {
        protected override void Write(ContentWriter output, AnimationsContent value)
        {
            WriteClips(output, value.Clips);
            WriteBindPose(output, value.BindPose);
            WriteInvBindPose(output, value.InvBindPose);
            WriteSkeletonHierarchy(output, value.SkeletonHierarchy);
        }

        private void WriteClips(ContentWriter output, Dictionary<string, ClipContent> clips)
        {
            Int32 count = clips.Count;
            output.Write((Int32)count);

            foreach (var clip in clips)
            {
                output.Write(clip.Key);
                output.WriteObject<ClipContent>(clip.Value);
            }            

            return;
        }

        private void WriteBindPose(ContentWriter output, List<Microsoft.Xna.Framework.Matrix> bindPoses)
        {
            Int32 count = bindPoses.Count;
            output.Write((Int32)count);

            for (int i = 0; i < count; i++)
                output.Write(bindPoses[i]);

            return;
        }

        private void WriteInvBindPose(ContentWriter output, List<Microsoft.Xna.Framework.Matrix> invBindPoses)
        {
            Int32 count = invBindPoses.Count;
            output.Write((Int32)count);

            for (int i = 0; i < count; i++)
                output.Write(invBindPoses[i]);

            return;
        }

        private void WriteSkeletonHierarchy(ContentWriter output, List<int> skeletonHierarchy)
        {
            Int32 count = skeletonHierarchy.Count;
            output.Write((Int32)count);

            for (int i = 0; i < count; i++)
                output.Write((Int32)skeletonHierarchy[i]);

            return;
        }
    
        public override string GetRuntimeType(TargetPlatform targetPlatform)
        {
            return "tainicom.Aether.Animation.Animations, Aether.Animation";
        }

        public override string GetRuntimeReader(TargetPlatform targetPlatform)
        {
            return "tainicom.Aether.Animation.Content.AnimationsReader, Aether.Animation";
        }
    }
        
}
