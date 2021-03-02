#region File Description
//-----------------------------------------------------------------------------
// BasicEffect.cs
//
// Microsoft XNA Community Game Platform
// Copyright (C) Microsoft Corporation. All rights reserved.
//-----------------------------------------------------------------------------
#endregion

using System;
using System.Reflection;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace tainicom.Aether.Shaders
{
    public class LightingEffect : Effect, IEffectMatrices, IEffectLights, IEffectFog
    {
        #region Effect Parameters

        EffectParameter textureParam;
        EffectParameter diffuseColorParam;
        EffectParameter emissiveColorParam;
        EffectParameter specularColorParam;
        EffectParameter specularPowerParam;
        EffectParameter eyePositionParam;
        EffectParameter fogColorParam;
        EffectParameter fogVectorParam;
        EffectParameter worldParam;
        EffectParameter worldInverseTransposeParam;
        EffectParameter worldViewProjParam;

        #endregion

        #region Fields

        bool lightingEnabled;
        bool preferPerPixelLighting;
        bool oneLight;
        bool fogEnabled;
        bool textureEnabled;
        bool vertexColorEnabled;

        Matrix world = Matrix.Identity;
        Matrix view = Matrix.Identity;
        Matrix projection = Matrix.Identity;

        Matrix worldView;

        Vector3 diffuseColor = Vector3.One;
        Vector3 emissiveColor = Vector3.Zero;
        Vector3 ambientLightColor = Vector3.Zero;

        float alpha = 1;

        DirectionalLight light0;
        DirectionalLight light1;
        DirectionalLight light2;

        float fogStart = 0;
        float fogEnd = 1;

        EffectDirtyFlags dirtyFlags = EffectDirtyFlags.All;


        static readonly String ResourceName = "tainicom.Aether.Shaders.Resources.LightingEffect";
#if XNA
        static readonly String PlatformName = ".xna";
#elif ((MG && WINDOWS) || W8_1 || W10)
        static readonly String PlatformName = ".dx11.mgfxo";
#endif

        private static string GetResourceName(GraphicsDevice graphicsDevice)
        {
            string profileName = (graphicsDevice.GraphicsProfile == GraphicsProfile.Reach) ? ".Reach" : ".HiDef";


            // Detect MG version
            var version = "";
#if !XNA
            version = ".9";
            var mgVersion = GetAssembly(typeof(Effect)).GetName().Version;
            if (mgVersion.Major == 3)
            {                
                if (mgVersion.Minor == 6)
                    version = ".8";
                if (mgVersion.Minor == 7)
                    version = ".8";
                if (mgVersion.Minor == 8)
                    version = ".9";
            }
#endif

            return ResourceName + profileName + PlatformName + version;
        }

        internal static byte[] LoadEffectResource(string name)
        {
            using (var stream = GetAssembly(typeof(LightingEffect)).GetManifestResourceStream(name))
            {
                var bytecode = new byte[stream.Length];
                stream.Read(bytecode, 0, (int)stream.Length);
                return bytecode;
            }
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


        /// <summary>
        /// Gets or sets the world matrix.
        /// </summary>
        public Matrix World
        {
            get { return world; }
            
            set
            {
                world = value;
                dirtyFlags |= EffectDirtyFlags.World | EffectDirtyFlags.WorldViewProj | EffectDirtyFlags.Fog;
            }
        }


        /// <summary>
        /// Gets or sets the view matrix.
        /// </summary>
        public Matrix View
        {
            get { return view; }
            
            set
            {
                view = value;
                dirtyFlags |= EffectDirtyFlags.WorldViewProj | EffectDirtyFlags.EyePosition | EffectDirtyFlags.Fog;
            }
        }


        /// <summary>
        /// Gets or sets the projection matrix.
        /// </summary>
        public Matrix Projection
        {
            get { return projection; }
            
            set
            {
                projection = value;
                dirtyFlags |= EffectDirtyFlags.WorldViewProj;
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
        /// Gets or sets the material emissive color (range 0 to 1).
        /// </summary>
        public Vector3 EmissiveColor
        {
            get { return emissiveColor; }
            
            set
            {
                emissiveColor = value;
                dirtyFlags |= EffectDirtyFlags.MaterialColor;
            }
        }


        /// <summary>
        /// Gets or sets the material specular color (range 0 to 1).
        /// </summary>
        public Vector3 SpecularColor
        {
            get { return specularColorParam.GetValueVector3(); }
            set { specularColorParam.SetValue(value); }
        }


        /// <summary>
        /// Gets or sets the material specular power.
        /// </summary>
        public float SpecularPower
        {
            get { return specularPowerParam.GetValueSingle(); }
            set { specularPowerParam.SetValue(value); }
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

        public bool LightingEnabled
        {
            get { return lightingEnabled; }
            
            set
            {
                if (lightingEnabled != value)
                {
                    lightingEnabled = value;
                    dirtyFlags |= EffectDirtyFlags.ShaderIndex | EffectDirtyFlags.MaterialColor;
                }
            }
        }


        /// <summary>
        /// Gets or sets the per-pixel lighting prefer flag.
        /// </summary>
        public bool PreferPerPixelLighting
        {
            get { return preferPerPixelLighting; }
            
            set
            {
                if (preferPerPixelLighting != value)
                {
                    preferPerPixelLighting = value;
                    dirtyFlags |= EffectDirtyFlags.ShaderIndex;
                }
            }
        }


        public Vector3 AmbientLightColor
        {
            get { return ambientLightColor; }
            
            set
            {
                ambientLightColor = value;
                dirtyFlags |= EffectDirtyFlags.MaterialColor;
            }
        }


        public DirectionalLight DirectionalLight0 { get { return light0; } }


        public DirectionalLight DirectionalLight1 { get { return light1; } }


        public DirectionalLight DirectionalLight2 { get { return light2; } }


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


        /// <inheritdoc/>
        public float FogStart
        {
            get { return fogStart; }
            
            set
            {
                fogStart = value;
                dirtyFlags |= EffectDirtyFlags.Fog;
            }
        }


        public float FogEnd
        {
            get { return fogEnd; }
            
            set
            {
                fogEnd = value;
                dirtyFlags |= EffectDirtyFlags.Fog;
            }
        }


        public Vector3 FogColor
        {
            get { return fogColorParam.GetValueVector3(); }
            set { fogColorParam.SetValue(value); }
        }


        /// <summary>
        /// Gets or sets whether texturing is enabled.
        /// </summary>
        public bool TextureEnabled
        {
            get { return textureEnabled; }
            
            set
            {
                if (textureEnabled != value)
                {
                    textureEnabled = value;
                    dirtyFlags |= EffectDirtyFlags.ShaderIndex;
                }
            }
        }


        public Texture2D Texture
        {
            get { return textureParam.GetValueTexture2D(); }
            set { textureParam.SetValue(value); }
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

        public LightingEffect(GraphicsDevice graphicsDevice)
            : base(graphicsDevice, LoadEffectResource(GetResourceName(graphicsDevice)))
        {
            CacheEffectParameters(null);

            DirectionalLight0.Enabled = true;
            SpecularColor = Vector3.One;
            SpecularPower = 16;
        }

        protected LightingEffect(LightingEffect cloneSource)
            : base(cloneSource)
        {
            CacheEffectParameters(cloneSource);

            lightingEnabled = cloneSource.lightingEnabled;
            preferPerPixelLighting = cloneSource.preferPerPixelLighting;
            fogEnabled = cloneSource.fogEnabled;            
            textureEnabled = cloneSource.textureEnabled;
            vertexColorEnabled = cloneSource.vertexColorEnabled;

            world = cloneSource.world;
            view = cloneSource.view;
            projection = cloneSource.projection;

            diffuseColor = cloneSource.diffuseColor;
            emissiveColor = cloneSource.emissiveColor;
            ambientLightColor = cloneSource.ambientLightColor;

            alpha = cloneSource.alpha;

            fogStart = cloneSource.fogStart;
            fogEnd = cloneSource.fogEnd;
        }

        public override Effect Clone()
        {
            return new LightingEffect(this);
        }


        public void EnableDefaultLighting()
        {
            LightingEnabled = true;

            AmbientLightColor = EffectHelpers.EnableDefaultLighting(light0, light1, light2);
        }

        void CacheEffectParameters(LightingEffect cloneSource)
        {
            textureParam                = Parameters["Texture"];
            diffuseColorParam           = Parameters["DiffuseColor"];
            emissiveColorParam          = Parameters["EmissiveColor"];
            specularColorParam          = Parameters["SpecularColor"];
            specularPowerParam          = Parameters["SpecularPower"];
            eyePositionParam            = Parameters["EyePosition"];
            fogColorParam               = Parameters["FogColor"];
            fogVectorParam              = Parameters["FogVector"];
            worldParam                  = Parameters["World"];
            worldInverseTransposeParam  = Parameters["WorldInverseTranspose"];
            worldViewProjParam          = Parameters["WorldViewProj"];

            light0 = new DirectionalLight(Parameters["DirLight0Direction"],
                                          Parameters["DirLight0DiffuseColor"],
                                          Parameters["DirLight0SpecularColor"],
                                          (cloneSource != null) ? cloneSource.light0 : null);

            light1 = new DirectionalLight(Parameters["DirLight1Direction"],
                                          Parameters["DirLight1DiffuseColor"],
                                          Parameters["DirLight1SpecularColor"],
                                          (cloneSource != null) ? cloneSource.light1 : null);

            light2 = new DirectionalLight(Parameters["DirLight2Direction"],
                                          Parameters["DirLight2DiffuseColor"],
                                          Parameters["DirLight2SpecularColor"],
                                          (cloneSource != null) ? cloneSource.light2 : null);
        }


        protected override void OnApply()
        {
            // Recompute the world+view+projection matrix or fog vector?
            dirtyFlags = EffectHelpers.SetWorldViewProjAndFog(dirtyFlags, ref world, ref view, ref projection, ref worldView, fogEnabled, fogStart, fogEnd, worldViewProjParam, fogVectorParam);
            
            // Recompute the diffuse/emissive/alpha material color parameters?
            if ((dirtyFlags & EffectDirtyFlags.MaterialColor) != 0)
            {
                EffectHelpers.SetMaterialColor(lightingEnabled, alpha, ref diffuseColor, ref emissiveColor, ref ambientLightColor, diffuseColorParam, emissiveColorParam);

                dirtyFlags &= ~EffectDirtyFlags.MaterialColor;
            }

            if (lightingEnabled)
            {
                // Recompute the world inverse transpose and eye position?
                dirtyFlags = EffectHelpers.SetLightingMatrices(dirtyFlags, ref world, ref view, worldParam, worldInverseTransposeParam, eyePositionParam);

                
                // Check if we can use the only-bother-with-the-first-light shader optimization.
                bool newOneLight = !light1.Enabled && !light2.Enabled;
                
                if (oneLight != newOneLight)
                {
                    oneLight = newOneLight;
                    dirtyFlags |= EffectDirtyFlags.ShaderIndex;
                }
            }

            // Recompute the shader index?
            if ((dirtyFlags & EffectDirtyFlags.ShaderIndex) != 0)
            {
                int shaderIndex = 0;
                
                if (!fogEnabled)
                    shaderIndex += 1;
                
                if (vertexColorEnabled)
                    shaderIndex += 2;
                
                if (textureEnabled)
                    shaderIndex += 4;

                if (lightingEnabled)
                {
                    if (preferPerPixelLighting)
                        shaderIndex += 24;
                    else if (oneLight)
                        shaderIndex += 16;
                    else
                        shaderIndex += 8;
                }

                dirtyFlags &= ~EffectDirtyFlags.ShaderIndex;

                CurrentTechnique = Techniques[shaderIndex];
            }
        }


        #endregion
    }
}
