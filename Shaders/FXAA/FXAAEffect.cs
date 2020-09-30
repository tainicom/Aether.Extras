#region License
//   Copyright 2013-2016 Kastellanos Nikolaos
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
    public class FXAAEffect : Effect , IEffectMatrices
    {
        #region Effect Parameters
        
        EffectParameter subPixelAliasingRemovalParam;
        EffectParameter edgeThresholdParam;
        EffectParameter edgeThresholdMinParam;
        EffectParameter consoleEdgeSharpnessParam;
        EffectParameter consoleEdgeThresholdParam;
        EffectParameter consoleEdgeThresholdMinParam;
        EffectParameter inverseViewportSizeParam;
        EffectParameter consoleSharpnessParam;
        EffectParameter consoleOpt1Param;
        EffectParameter consoleOpt2Param;
        EffectParameter projectionParam;
        EffectParameter viewParam;
        EffectParameter worldParam;

        #endregion

        #region Fields

        int techniqueIndex = 0;

        public const int FXAA = 0x00000001;

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
            
            Stream stream = GetAssembly(typeof(FXAAEffect)).GetManifestResourceStream(name);
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

        /// <summary>        
        /// Choose the amount of sub-pixel aliasing removal.
        /// This can effect sharpness.
        ///   1.00 - upper limit (softer)
        ///   0.75 - default amount of filtering
        ///   0.50 - lower limit (sharper, less sub-pixel aliasing removal)
        ///   0.25 - almost off
        ///   0.00 - completely off
        /// </summary>
        public float SubPixelAliasingRemoval
        {
            get { return subPixelAliasingRemovalParam.GetValueSingle(); }
            set { subPixelAliasingRemovalParam.SetValue(value); }
        }

        /// <summary>
        /// The minimum amount of local contrast required to apply algorithm.
        ///   0.333 - too little (faster)
        ///   0.250 - low quality
        ///   0.166 - default
        ///   0.125 - high quality 
        ///   0.063 - overkill (slower)
        /// </summary>
        public float EdgeThreshold
        {
            get { return edgeThresholdParam.GetValueSingle(); }
            set { edgeThresholdParam.SetValue(value); }
        }

        /// <summary>
        /// Trims the algorithm from processing darks.
        ///   0.0833 - upper limit (default, the start of visible unfiltered edges)
        ///   0.0625 - high quality (faster)
        ///   0.0312 - visible limit (slower)
        /// Special notes when using FXAA_GREEN_AS_LUMA,
        ///   Likely want to set this to zero.
        ///   As colors that are mostly not-green
        ///   will appear very dark in the green channel!
        ///   Tune by looking at mostly non-green content,
        ///   then start at zero and increase until aliasing is a problem.
        /// </summary>
        public float EdgeThresholdMin
        {
            get { return edgeThresholdMinParam.GetValueSingle(); }
            set { edgeThresholdMinParam.SetValue(value); }
        }

        public float ConsoleEdgeSharpness
        {
            get { return consoleEdgeSharpnessParam.GetValueSingle(); }
            set { consoleEdgeSharpnessParam.SetValue(value); }
        }

        public float ConsoleEdgeThreshold
        {
            get { return consoleEdgeThresholdParam.GetValueSingle(); }
            set { consoleEdgeThresholdParam.SetValue(value); }
        }

        public float ConsoleEdgeThresholdMin
        {
            get { return consoleEdgeThresholdMinParam.GetValueSingle(); }
            set { consoleEdgeThresholdMinParam.SetValue(value); }
        }

        public Vector2 InverseViewportSize
        {
            get { return inverseViewportSizeParam.GetValueVector2(); }
            set { inverseViewportSizeParam.SetValue(value); }
        }

        public Vector4 ConsoleSharpness
        {
            get { return consoleSharpnessParam.GetValueVector4(); }
            set { consoleSharpnessParam.SetValue(value); }
        }

        public Vector4 ConsoleOpt1
        {
            get { return consoleOpt1Param.GetValueVector4(); }
            set { consoleOpt1Param.SetValue(value); }
        }

        public Vector4 ConsoleOpt2
        {
            get { return consoleOpt2Param.GetValueVector4(); }
            set { consoleOpt2Param.SetValue(value); }
        }

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

        public bool AntialiasingEnabled
        {
            get { return (techniqueIndex & FXAA) == FXAA; }
            set
            {
                techniqueIndex = (value) ? (techniqueIndex|FXAA) : (techniqueIndex&~FXAA);
                CurrentTechnique = Techniques[techniqueIndex];
            }
        }

        #endregion

        #region Methods

        public FXAAEffect(GraphicsDevice graphicsDevice, byte[] Bytecode): base(graphicsDevice, Bytecode)
        {   
            CacheEffectParameters(null);
            
            SubPixelAliasingRemoval = 0.75f;
            EdgeThreshold = 0.166f;
            EdgeThresholdMin = 0.0833f;
            ConsoleEdgeSharpness = 8.0f;
            ConsoleEdgeThreshold = 0.125f;
            ConsoleEdgeThresholdMin = 0f;         

            AntialiasingEnabled = true;
        }

        protected FXAAEffect(FXAAEffect cloneSource)
            : base(cloneSource)
        {
            CacheEffectParameters(cloneSource);

            AntialiasingEnabled = cloneSource.AntialiasingEnabled;
        }

        /// <param name="N">
        /// This effects sub-pixel AA quality and inversely sharpness.
        ///   Where N ranges between,
        ///     N = 0.50 (default)
        ///     N = 0.33 (sharper)
        /// </param>
        public void SetDefaultParameters(int width, int height, float N = 0.5f)
        {
            Matrix projection = Matrix.CreateOrthographicOffCenter(0, width, height, 0, 0, 1);
#if XNA 
            Matrix halfPixelOffset = Matrix.CreateTranslation(-0.5f, -0.5f, 0);
#else
            Matrix halfPixelOffset = Matrix.Identity;
#endif

            World = Matrix.Identity;
            View = Matrix.Identity;
            Projection = halfPixelOffset * projection;
            InverseViewportSize = new Vector2(1f / width, 1f / height);
            ConsoleSharpness = new Vector4(
                -N / width, -N / height,
                N / width, N / height);
            ConsoleOpt1 = new Vector4(
                -2.0f / width, -2.0f / height,
                2.0f / width, 2.0f / height);
            ConsoleOpt2 = new Vector4(
                8.0f / width, 8.0f / height,
                -4.0f / width, -4.0f / height);
        }

        public override Effect Clone()
        {
            return new FXAAEffect(this);
        }

        void CacheEffectParameters(FXAAEffect cloneSource)
        {
            subPixelAliasingRemovalParam = Parameters["SubPixelAliasingRemoval"];
            edgeThresholdParam = Parameters["EdgeThreshold"];
            edgeThresholdMinParam = Parameters["EdgeThresholdMin"];
            consoleEdgeSharpnessParam = Parameters["ConsoleEdgeSharpness"];
            consoleEdgeThresholdParam = Parameters["ConsoleEdgeThreshold"];
            consoleEdgeThresholdMinParam = Parameters["ConsoleEdgeThresholdMin"];
            inverseViewportSizeParam = Parameters["InverseViewportSize"];
            consoleSharpnessParam = Parameters["ConsoleSharpness"];
            consoleOpt1Param = Parameters["ConsoleOpt1"];
            consoleOpt2Param = Parameters["ConsoleOpt2"];
            projectionParam = Parameters["Projection"];
            viewParam = Parameters["View"];
            worldParam = Parameters["World"];
        }
        
        #endregion
    }
}
