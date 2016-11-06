#region License
//   Copyright 2015-2016 Kastellanos Nikolaos
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

namespace tainicom.Aether.Content.Pipeline
{
    [ContentProcessor(DisplayName = "DDSProcessor - Aether")]
    public class DDSProcessor : ContentProcessor<TextureContent, TextureContent>
    {
        public virtual bool GenerateMipmaps { get; set; }
        public virtual TextureProcessorOutputFormat TextureFormat { get; set; }
        
        public DDSProcessor()
        {
            GenerateMipmaps = true;
            TextureFormat = TextureProcessorOutputFormat.NoChange;
        }

        public override TextureContent Process(TextureContent input, ContentProcessorContext context)
        {
            TextureContent output = input;

            switch (TextureFormat)
            {
                case TextureProcessorOutputFormat.Color:
                    SurfaceFormat format;
                    output.Faces[0][0].TryGetFormat(out format);
                    if (format != SurfaceFormat.Color)
                        ConvertToColor(output);
                    break;

            }

            //output.ConvertBitmapType();

            if (GenerateMipmaps)
                output.GenerateMipmaps(false);

            return output;
        }

        private void ConvertToColor(TextureContent textureContent)
        {
            foreach (var face in textureContent.Faces)
            {
                for (int m = 0; m < face.Count; m++)
                {
                    BitmapContent input = face[m];
                    BitmapContent output = ConvertToColor(input);
                    face[m] = output;
                }
            }
            return;
        }

        unsafe private static BitmapContent ConvertToColor(BitmapContent input)
        {
            var width = input.Width;
            var height = input.Height;

            SurfaceFormat format;
            input.TryGetFormat(out format);
            var formatSize = DDSImporter.GetBitmapSize(format, width, height);
            var blocks = formatSize;
            var inData = input.GetPixelData();

            var output = new PixelBitmapContent<Color>(width, height);

            fixed (byte* p = &inData[0])
            {
                DXT1Block* block = (DXT1Block*)p;

                //convert DXT1 to color
                for (int y = 0; y < height; ++y)
                {
                    for (int x = 0; x < width; ++x)
                    {
                        int blockIdx = (x / 4) + (y / 4) * (width / 4);
                        int colorIndex = x % 4 + (y % 4) * 4;

                        Color color = block[blockIdx].GetColor(colorIndex);
                        output.SetPixel(x, y, color);
                    }
                }
            }

            return output;
        }
    }
}