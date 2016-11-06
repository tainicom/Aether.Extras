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

using System;
using System.IO;
using Microsoft.Xna.Framework.Content.Pipeline;
using Microsoft.Xna.Framework.Content.Pipeline.Graphics;
using Microsoft.Xna.Framework.Graphics;

namespace tainicom.Aether.Content.Pipeline
{
    [ContentImporter(".dds", DisplayName = "DDS Importer - Aether", DefaultProcessor = "DDSProcessor")]
    public class DDSImporter : ContentImporter<TextureContent>
    {
        public override TextureContent Import(string filename, ContentImporterContext context)
        {
            TextureContent output;

            using(var stream = new FileStream(filename, FileMode.Open, FileAccess.Read, FileShare.Read))
            {
                using(var reader = new BinaryReader(stream))
                {
                    var header = new DDSHeader(reader);
                    if (header.PixelFormat.Flags == PF_Flags.FOURCC && header.PixelFormat.FourCC == PF_FourCC.DX10)
                    {
                        throw new NotImplementedException("DX10 Header not supported");
                    }
                    if (header.PitchOrLinearSize != 0)
                        throw new NotImplementedException();
                    

                    if (header.Caps2.HasFlag(DDS_Caps2.CUBEMAP))
                    {
                        var cube = new TextureCubeContent();
                        var format = GetFormat(header.PixelFormat);
                        for (int f = 0; f < 6; f++)
                        {
                            var width = header.Width;
                            var height = header.Height;
                            BitmapContent bitmap = CreateBitmap(format, width, height);
                            var size = GetBitmapSize(format, width, height);
                            byte[] src = reader.ReadBytes(size);
                            bitmap.SetPixelData(src);
                            cube.Faces[f].Add(bitmap);

                            if (header.Caps.HasFlag(DDS_Caps.MIPMAP))
                            {
                                for(int m=0; m<header.MipMapCount-1; m++)
                                {
                                    width = width/2;
                                    height = height/2;

                                    bitmap = CreateBitmap(format, width, height);
                                    size = GetBitmapSize(format, width, height);
                                    src = reader.ReadBytes(size);
                                    bitmap.SetPixelData(src);
                                    cube.Faces[f].Add(bitmap);
                                }
                            }                            
                        }
                        output = cube;
                    }
                    else
                    {
                        throw new NotImplementedException(header.Caps2 + " not supported");
                    }

                    reader.Close();
                    stream.Close();
                }
            }            

            return output;
        }

        internal static int GetBitmapSize(SurfaceFormat format, int width, int height)
        {
            int pixels = width * height;

            switch (format)
            {
                case SurfaceFormat.Dxt1:
                    return Math.Max(8, (pixels / 2));
                default:
                    throw new NotImplementedException();
            }
        }
        
        private SurfaceFormat GetFormat(DDSPixelFormat pixelFormat)
        {
            switch (pixelFormat.Flags)
            {
                case PF_Flags.FOURCC:
                    switch (pixelFormat.FourCC)
                    {
                        case PF_FourCC.DXT1:
                            return SurfaceFormat.Dxt1;
                        default:
                            throw new NotImplementedException();
                    }
                default:
                    throw new NotImplementedException();
            }
        }

        private BitmapContent CreateBitmap(SurfaceFormat targetFormat, int width, int height)
        {
            switch (targetFormat)
            {
                case SurfaceFormat.Dxt1:
                    return new Dxt1BitmapContent(width, height);
                default:
                    throw new NotImplementedException();
            }
        }
    }

}
