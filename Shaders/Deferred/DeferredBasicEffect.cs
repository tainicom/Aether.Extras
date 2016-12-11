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
    public class DeferredBasicEffect : Effect, IEffectMatrices
    {
        #region Effect Parameters
            
        // IEffectMatrices
        EffectParameter projectionParam;
        EffectParameter viewParam;
        EffectParameter worldParam;

        #endregion

        #region Fields


#if ((MG && WINDOWS) || W8_1 || W10)
        static readonly String resourceName = "tainicom.Aether.Shaders.Resources.DeferredBasicEffect.dx11.mgfxo";
#else
        static readonly String resourceName = "tainicom.Aether.Shaders.Resources.DeferredBasicEffect.xna.WinReach";
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
            var mgVersion = GetAssembly(typeof(Effect)).GetName().Version;
            if (mgVersion.Major == 3)
            {
                if (mgVersion.Minor == 4)
                    name += ".6";
                if (mgVersion.Minor == 5)
                    name += ".7";
                if (mgVersion.Minor == 6)
                    name += ".8";
            }

            Stream stream = GetAssembly(typeof(DeferredBasicEffect)).GetManifestResourceStream(name);
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
            get { return projectionParam.GetValueMatrix(); }
            set { projectionParam.SetValue(value); }
        }

        public Matrix View
        {
            get { return viewParam.GetValueMatrix(); }
            set { viewParam.SetValue(value); }
        }

        public Matrix World
        {
            get { return worldParam.GetValueMatrix(); }
            set { worldParam.SetValue(value); }
        }

        #endregion

        #region Methods

         public DeferredBasicEffect(GraphicsDevice graphicsDevice)
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

        public DeferredBasicEffect(GraphicsDevice graphicsDevice, byte[] Bytecode): base(graphicsDevice, Bytecode)
        {   
            CacheEffectParameters(null);
        }

        protected DeferredBasicEffect(DeferredBasicEffect cloneSource)
            : base(cloneSource)
        {
            CacheEffectParameters(cloneSource);
        }
        
        public override Effect Clone()
        {
            return new DeferredBasicEffect(this);
        }

        void CacheEffectParameters(DeferredBasicEffect cloneSource)
        {    

            // IEffectMatrices
            projectionParam = Parameters["Projection"];
            viewParam = Parameters["View"];
            worldParam = Parameters["World"];
        }
        
        #endregion
    }
}
