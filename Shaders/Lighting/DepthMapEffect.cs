#region License
//   Copyright 2020-2021 Kastellanos Nikolaos
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
using System.Reflection;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace tainicom.Aether.Shaders
{
    public class DepthMapEffect : Effect, IEffectMatrices
    {
        #region Effect Parameters
            
        // IEffectMatrices
        EffectParameter projectionParam;
        EffectParameter viewParam;
        EffectParameter worldParam;

        #endregion

        #region Fields



        static readonly String ResourceName = "tainicom.Aether.Shaders.Resources.DepthMapEffect";
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

         public DepthMapEffect(GraphicsDevice graphicsDevice)
            : base(graphicsDevice, LoadEffectResource(GetResourceName(graphicsDevice)))
        {    
            CacheEffectParameters(null);
        }

        protected DepthMapEffect(DepthMapEffect cloneSource)
            : base(cloneSource)
        {
            CacheEffectParameters(cloneSource);
        }
        
        public override Effect Clone()
        {
            return new DepthMapEffect(this);
        }

        void CacheEffectParameters(DepthMapEffect cloneSource)
        {
            // IEffectMatrices
            projectionParam = Parameters["Projection"];
            viewParam = Parameters["View"];
            worldParam = Parameters["World"];
        }
        
        #endregion
    }
}
