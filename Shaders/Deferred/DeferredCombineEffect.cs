#region License
//   Copyright 2014-2016 Kastellanos Nikolaos
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
    public class DeferredCombineEffect : Effect
    {
        #region Effect Parameters

        EffectParameter colorMapParam;
        EffectParameter lightMapParam;
        EffectParameter halfPixelParam;

        #endregion

        #region Fields


#if ((MG && WINDOWS) || W8_1 || W10)
        static readonly String resourceName = "tainicom.Aether.Shaders.Resources.DeferredCombine.dx11.mgfxo";
#else
        static readonly String resourceName = "tainicom.Aether.Shaders.Resources.DeferredCombine.xna.WinReach";
#endif

        internal static byte[] LoadEffectResource(string name)
        {
            using (var stream = LoadEffectResourceStream(name))
            {
                var bytecode = new byte[stream.Length];
                stream.Read(bytecode, 0, (int)stream.Length);
                return bytecode;
            }
        }

        internal static Stream LoadEffectResourceStream(string name)
        {
            // Detect MG version            
            var version = "";
#if !XNA
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

            Stream stream = GetAssembly(typeof(DeferredCombineEffect)).GetManifestResourceStream(name);
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

        public RenderTarget2D ColorMap
        {
            get { return colorMapParam.GetValueTexture2D() as RenderTarget2D; }
            set { colorMapParam.SetValue(value); }
        }

        public RenderTarget2D LightMap
        {
            get { return lightMapParam.GetValueTexture2D() as RenderTarget2D; }
            set { lightMapParam.SetValue(value); }
        }

        public Vector2 HalfPixel
        {
            get { return halfPixelParam.GetValueVector2(); }
            set { halfPixelParam.SetValue(value); }
        }

        #endregion

        #region Methods

         public DeferredCombineEffect(GraphicsDevice graphicsDevice)
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

        public DeferredCombineEffect(GraphicsDevice graphicsDevice, byte[] Bytecode): base(graphicsDevice, Bytecode)
        {   
            CacheEffectParameters(null);
        }

        protected DeferredCombineEffect(DeferredCombineEffect cloneSource)
            : base(cloneSource)
        {
            CacheEffectParameters(cloneSource);
        }
        
        public override Effect Clone()
        {
            return new DeferredCombineEffect(this);
        }

        void CacheEffectParameters(DeferredCombineEffect cloneSource)
        {
            colorMapParam = Parameters["colorMap"];
            lightMapParam = Parameters["lightMap"];
            halfPixelParam = Parameters["halfPixel"];
        }
        
        #endregion
    }
}
