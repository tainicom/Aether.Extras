#region License
//   Copyright 2019 Kastellanos Nikolaos
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

using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content.Pipeline;
using Microsoft.Xna.Framework.Content.Pipeline.Graphics;
using Microsoft.Xna.Framework.Content.Pipeline.Processors;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Graphics.PackedVector;
using System.ComponentModel;

namespace tainicom.Aether.Content.Pipeline
{
    [ContentProcessor(DisplayName = "SLMCProcessor - Aether")]
    public class SLMCProcessor : ContentProcessor<TextureContent, TextureContent>
    {
        SLMCOutputFormat _textureFormat = SLMCOutputFormat.BGRA4444;
        bool _generateMipmaps  = true;

        [DefaultValue(SLMCOutputFormat.BGRA4444)]
        public virtual SLMCOutputFormat TextureFormat
        {
            get { return _textureFormat; }
            set { _textureFormat = value; }
        }

        [DefaultValue(true)]
        public virtual bool GenerateMipmaps
        {
            get { return _generateMipmaps; }
            set { _generateMipmaps = value; }
        }
        
        public SLMCProcessor()
        {
        }

        public override TextureContent Process(TextureContent input, ContentProcessorContext context)
        {
            TextureContent output = input;

            SurfaceFormat format;
            output.Faces[0][0].TryGetFormat(out format);
            if (format != SurfaceFormat.Color)
                throw new InvalidContentException("Input SurfaceFormat must be color.");


            if (TextureFormat == SLMCOutputFormat.BGRA4444)
            {
                output.ConvertBitmapType(typeof(PixelBitmapContent<Bgra4444>));             
            }


            if (GenerateMipmaps)
                output.GenerateMipmaps(false);

            // TODO: implement TextureContent.Validate in MonoGame
            //output.Validate(context.TargetProfile);

            return output;
        }

    }

}