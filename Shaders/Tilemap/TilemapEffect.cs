#region License
//   Copyright 2019-2021 Kastellanos Nikolaos
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
using System.IO;
using System.Reflection;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace tainicom.Aether.Shaders
{
    public class TilemapEffect : Effect, IEffectMatrices
    {
        #region Effect Parameters
        
        EffectParameter textureParam;
        EffectParameter textureAtlasParam;

        EffectParameter mapSizeParam;
        EffectParameter invAtlasSizeParam;
        
        EffectParameter diffuseColorParam;
        EffectParameter fogColorParam;
        EffectParameter fogVectorParam;

        // IEffectMatrices
        EffectParameter worldViewProjParam;

        #endregion

        #region Fields
        
        bool fogEnabled;
        bool vertexColorEnabled;

        Matrix world = Matrix.Identity;
        Matrix view = Matrix.Identity;
        Matrix projection = Matrix.Identity;

        Matrix worldView;

        Vector3 diffuseColor = Vector3.One;

        float alpha = 1;

        float fogStart = 0;
        float fogEnd = 1;
                
        Vector2 atlasSize;

        EffectDirtyFlags dirtyFlags = EffectDirtyFlags.All;

#if ((MG && WINDOWS) || W8_1 || W10)
        static readonly String resourceName = "tainicom.Aether.Shaders.Resources.TilemapEffect.dx11.mgfxo";
#else
        static readonly String resourceName = "tainicom.Aether.Shaders.Resources.TilemapEffect.xna.WinReach";
#endif

        internal static byte[] LoadEffectResource(string name)
        {
            using (var stream = LoadEffectResourceStream(name))
            using (var ms = new MemoryStream())
            {
                stream.CopyTo(ms);
                return ms.ToArray();
            }
        }

        internal static Stream LoadEffectResourceStream(string name)
        {
            // Detect MG version
            var version = "";
#if !XNA && !PORTABLE
            version = ".9";
            var mgVersion = GetAssembly(typeof(Effect)).GetName().Version;
            if (mgVersion.Major == 3)
            {
                if (mgVersion.Minor == 4)
                    version = ".6";
                if (mgVersion.Minor == 5)
                    version = ".7";
                if (mgVersion.Minor == 6)
                    version = ".8";
                if (mgVersion.Minor == 7)
                    version = ".8";
                if (mgVersion.Minor == 8)
                    version = ".9";
            }
            name = name + version;
#endif

            Stream stream = GetAssembly(typeof(TilemapEffect)).GetManifestResourceStream(name);
            return stream;
        }

        private static Assembly GetAssembly(Type type)
        {            
            #if W8_1 || W10 
            return type.GetTypeInfo().Assembly;
            #else
            return type.Assembly;
            #endif
        }

        #endregion
        
        #region Public Properties

        public Matrix Projection
        {
            get { return projection; }
            set
            {
                projection = value;
                dirtyFlags |= EffectDirtyFlags.WorldViewProj;
            }
        }

        public Matrix View
        {
            get { return view; }
            set
            {
                view = value;
                dirtyFlags |= EffectDirtyFlags.WorldViewProj | EffectDirtyFlags.Fog;
            }
        }

        public Matrix World
        {
            get { return world; }
            set
            {
                world = value;
                dirtyFlags |= EffectDirtyFlags.WorldViewProj | EffectDirtyFlags.Fog;
            }
        }
                
        /// <summary>
        /// Gets or sets the Tilemap texture.
        /// </summary>
        public Texture2D Texture
        {
            get { return textureParam.GetValueTexture2D(); }
            set { textureParam.SetValue(value); }
        }


        /// <summary>
        /// Gets or sets the Atlas texture.
        /// </summary>
        public Texture2D TextureAtlas
        {
            get { return textureAtlasParam.GetValueTexture2D(); }
            set { textureAtlasParam.SetValue(value); }
        }


        /// <summary>
        /// Gets or sets the Map size.
        /// </summary>
        public Vector2 MapSize
        {
            get { return mapSizeParam.GetValueVector2(); }
            set { mapSizeParam.SetValue(value); }
        }

        /// <summary>
        /// Gets or sets the Atlas size.
        /// </summary>
        public Vector2 AtlasSize
        {
            get { return atlasSize; }
            set 
            { 
                atlasSize = value;
                value.X = 1f/value.X;
                value.Y = 1f/value.Y;
                invAtlasSizeParam.SetValue(value);
            }
        }

        /// <summary>
        /// Gets or sets the material diffuse color (range 0 to 1).
        /// </summary>
        public Vector3 DiffuseColor
        {
            get { return diffuseColor; }

            set
            {
                diffuseColor = value;
                dirtyFlags |= EffectDirtyFlags.MaterialColor;
            }
        }


        /// <summary>
        /// Gets or sets the material alpha.
        /// </summary>
        public float Alpha
        {
            get { return alpha; }

            set
            {
                alpha = value;
                dirtyFlags |= EffectDirtyFlags.MaterialColor;
            }
        }


        /// <summary>
        /// Gets or sets the fog enable flag.
        /// </summary>
        public bool FogEnabled
        {
            get { return fogEnabled; }

            set
            {
                if (fogEnabled != value)
                {
                    fogEnabled = value;
                    dirtyFlags |= EffectDirtyFlags.ShaderIndex | EffectDirtyFlags.FogEnable;
                }
            }
        }


        /// <summary>
        /// Gets or sets the fog start distance.
        /// </summary>
        public float FogStart
        {
            get { return fogStart; }

            set
            {
                fogStart = value;
                dirtyFlags |= EffectDirtyFlags.Fog;
            }
        }


        /// <summary>
        /// Gets or sets the fog end distance.
        /// </summary>
        public float FogEnd
        {
            get { return fogEnd; }

            set
            {
                fogEnd = value;
                dirtyFlags |= EffectDirtyFlags.Fog;
            }
        }


        /// <summary>
        /// Gets or sets the fog color.
        /// </summary>
        public Vector3 FogColor
        {
            get { return fogColorParam.GetValueVector3(); }
            set { fogColorParam.SetValue(value); }
        }


        /// <summary>
        /// Gets or sets whether vertex color is enabled.
        /// </summary>
        public bool VertexColorEnabled
        {
            get { return vertexColorEnabled; }

            set
            {
                if (vertexColorEnabled != value)
                {
                    vertexColorEnabled = value;
                    dirtyFlags |= EffectDirtyFlags.ShaderIndex;
                }
            }
        }

        #endregion

        #region Methods

         public TilemapEffect(GraphicsDevice graphicsDevice)
            : base(graphicsDevice, 
#if NETFX_CORE || WP8
            LoadEffectResourceStream(resourceName), true
#else
            LoadEffectResource(resourceName)
#endif
           )
        {    
            CacheEffectParameters(null);
        }

        public TilemapEffect(GraphicsDevice graphicsDevice, byte[] Bytecode): base(graphicsDevice, Bytecode)
        {   
            CacheEffectParameters(null);
        }

        protected TilemapEffect(TilemapEffect cloneSource)
            : base(cloneSource)
        {
            CacheEffectParameters(cloneSource);
            
            fogEnabled = cloneSource.fogEnabled;
            vertexColorEnabled = cloneSource.vertexColorEnabled;

            world = cloneSource.world;
            view = cloneSource.view;
            projection = cloneSource.projection;

            diffuseColor = cloneSource.diffuseColor;

            alpha = cloneSource.alpha;

            fogStart = cloneSource.fogStart;
            fogEnd = cloneSource.fogEnd;

            atlasSize = cloneSource.atlasSize;
        }
        
        public override Effect Clone()
        {
            return new TilemapEffect(this);
        }

        void CacheEffectParameters(TilemapEffect cloneSource)
        {   
            textureParam = Parameters["Texture"];
            textureAtlasParam = Parameters["TextureAtlas"];
            mapSizeParam = Parameters["MapSize"];
            invAtlasSizeParam = Parameters["InvAtlasSize"];
            diffuseColorParam = Parameters["DiffuseColor"];
            fogColorParam = Parameters["FogColor"];
            fogVectorParam = Parameters["FogVector"];
            // IEffectMatrices
            worldViewProjParam = Parameters["WorldViewProj"];
        }


        /// <summary>
        /// Lazily computes derived parameter values immediately before applying the effect.
        /// </summary>
        protected override void OnApply()
        {
            // Recompute the world+view+projection matrix or fog vector?
            dirtyFlags = SetWorldViewProjAndFog(dirtyFlags, ref world, ref view, ref projection, ref worldView, fogEnabled, fogStart, fogEnd, worldViewProjParam, fogVectorParam);

            // Recompute the diffuse/alpha material color parameter?
            if ((dirtyFlags & EffectDirtyFlags.MaterialColor) != 0)
            {
                diffuseColorParam.SetValue(new Vector4(diffuseColor * alpha, alpha));

                dirtyFlags &= ~EffectDirtyFlags.MaterialColor;
            }

            // Recompute the shader index?
            if ((dirtyFlags & EffectDirtyFlags.ShaderIndex) != 0)
            {
                int shaderIndex = 0;

                if (!fogEnabled)
                    shaderIndex += 1;

                if (vertexColorEnabled)
                    shaderIndex += 2;

                dirtyFlags &= ~EffectDirtyFlags.ShaderIndex;

                CurrentTechnique = Techniques[shaderIndex];
            }
        }


        /// <summary>
        /// Lazily recomputes the world+view+projection matrix and
        /// fog vector based on the current effect parameter settings.
        /// </summary>
        static EffectDirtyFlags SetWorldViewProjAndFog(EffectDirtyFlags dirtyFlags,
                                                                ref Matrix world, ref Matrix view, ref Matrix projection, ref Matrix worldView,
                                                                bool fogEnabled, float fogStart, float fogEnd,
                                                                EffectParameter worldViewProjParam, EffectParameter fogVectorParam)
        {
            // Recompute the world+view+projection matrix?
            if ((dirtyFlags & EffectDirtyFlags.WorldViewProj) != 0)
            {
                Matrix worldViewProj;

                Matrix.Multiply(ref world, ref view, out worldView);
                Matrix.Multiply(ref worldView, ref projection, out worldViewProj);

                worldViewProjParam.SetValue(worldViewProj);

                dirtyFlags &= ~EffectDirtyFlags.WorldViewProj;
            }

            if (fogEnabled)
            {
                // Recompute the fog vector?
                if ((dirtyFlags & (EffectDirtyFlags.Fog | EffectDirtyFlags.FogEnable)) != 0)
                {
                    SetFogVector(ref worldView, fogStart, fogEnd, fogVectorParam);

                    dirtyFlags &= ~(EffectDirtyFlags.Fog | EffectDirtyFlags.FogEnable);
                }
            }
            else
            {
                // When fog is disabled, make sure the fog vector is reset to zero.
                if ((dirtyFlags & EffectDirtyFlags.FogEnable) != 0)
                {
                    fogVectorParam.SetValue(Vector4.Zero);

                    dirtyFlags &= ~EffectDirtyFlags.FogEnable;
                }
            }

            return dirtyFlags;
        }
        
        /// <summary>
        /// Sets a vector which can be dotted with the object space vertex position to compute fog amount.
        /// </summary>
        static void SetFogVector(ref Matrix worldView, float fogStart, float fogEnd, EffectParameter fogVectorParam)
        {
            if (fogStart == fogEnd)
            {
                // Degenerate case: force everything to 100% fogged if start and end are the same.
                fogVectorParam.SetValue(new Vector4(0, 0, 0, 1));
            }
            else
            {
                // We want to transform vertex positions into view space, take the resulting
                // Z value, then scale and offset according to the fog start/end distances.
                // Because we only care about the Z component, the shader can do all this
                // with a single dot product, using only the Z row of the world+view matrix.

                float scale = 1f / (fogStart - fogEnd);

                Vector4 fogVector = new Vector4();

                fogVector.X = worldView.M13 * scale;
                fogVector.Y = worldView.M23 * scale;
                fogVector.Z = worldView.M33 * scale;
                fogVector.W = (worldView.M43 + fogStart) * scale;

                fogVectorParam.SetValue(fogVector);
            }
        }
        
        #endregion


        enum EffectDirtyFlags
        {
            WorldViewProj = 1,
            //World = 2,
            //EyePosition = 4,
            MaterialColor = 8,
            Fog = 16,
            FogEnable = 32,
            //AlphaTest = 64,
            ShaderIndex = 128,
            All = -1
        }
    }
}
