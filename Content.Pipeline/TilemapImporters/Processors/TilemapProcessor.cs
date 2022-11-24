#region License
//   Copyright 2021 Kastellanos Nikolaos
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
using System.ComponentModel;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content.Pipeline;
using Microsoft.Xna.Framework.Content.Pipeline.Graphics;
using Microsoft.Xna.Framework.Content.Pipeline.Processors;

namespace tainicom.Aether.Content.Pipeline
{
    [ContentProcessor(DisplayName = "Tilemap Processor - Aether")]
    public class TilemapProcessor : TextureProcessor, IContentProcessor
    {        
        private bool _mipmapsPerSprite = true;

#if WINDOWS
        // override InputType
        [Browsable(false)]
#endif
        Type IContentProcessor.InputType { get { return typeof(TilemapContent); } }

#if WINDOWS
        // override OutputType
        [Browsable(false)]
#endif
        Type IContentProcessor.OutputType { get { return typeof(TilemapContent); } }
        

        [DefaultValue(true)]
        public bool MipmapsPerSprite
        {
            get { return _mipmapsPerSprite; }
            set { _mipmapsPerSprite = value; }
        }

        public TilemapProcessor()
        {
        }
        
        object IContentProcessor.Process(object input, ContentProcessorContext context)
        {
            return Process((TilemapContent)input, context);
        }

        public TilemapContent Process(TilemapContent input, ContentProcessorContext context)
        {
            if (MipmapsPerSprite && GenerateMipmaps)
                foreach (var texture in input.DestinationTiles)
                    texture.SrcTexture.GenerateMipmaps(false);

            var output = input;
            
            if (GenerateMipmaps)
            {
                if (MipmapsPerSprite)
                {
                    var maxTileWidth = 1;
                    var maxTileHeight = 1;
                    foreach (var tile in input.DestinationTiles)
                    {
                        maxTileWidth = Math.Max(maxTileWidth, tile.DstBounds.Width);
                        maxTileHeight = Math.Max(maxTileHeight, tile.DstBounds.Height);
                    }

                    for (int mipLevel = 1; ; mipLevel++)
                    {
                        int mipLevel2 = (int)Math.Pow(2, mipLevel);
                        Rectangle size = new Rectangle(0, 0, output.TextureAtlas.Faces[0][0].Width, output.TextureAtlas.Faces[0][0].Height);
                        size.Width /= mipLevel2;
                        size.Height /= mipLevel2;

                        if ((maxTileWidth / mipLevel2) < 1 && (maxTileHeight / mipLevel2) < 1)
                            break;

                        var mipmapBmp = new PixelBitmapContent<Color>(size.Width, size.Height);
                        foreach (var tile in input.DestinationTiles)
                        {
                            if (mipLevel >= tile.SrcTexture.Faces[0].Count) continue;
                            var srcBmp = tile.SrcTexture.Faces[0][mipLevel];
                            var srcBounds = new Rectangle(0, 0, srcBmp.Width, srcBmp.Height);
                            var dstBounds = tile.DstBounds;
                            dstBounds.X = (int)Math.Ceiling((float)dstBounds.X / mipLevel2);
                            dstBounds.Y = (int)Math.Ceiling((float)dstBounds.Y / mipLevel2);
                            dstBounds.Width = (int)(dstBounds.Width / mipLevel2);
                            dstBounds.Height = (int)(dstBounds.Height / mipLevel2);
                            // snap image to bottom
                            dstBounds.Width  = srcBounds.Width;
                            dstBounds.Y     += (dstBounds.Height - srcBounds.Height);
                            dstBounds.Height = srcBounds.Height;

                            if (dstBounds.Width == 0 || dstBounds.Height == 0)
                                continue;
                                
                            //if (dstBounds.Width > 0 && dstBounds.Height > 0)
                                BitmapContent.Copy(srcBmp, srcBounds, mipmapBmp, dstBounds);
                        }
                        output.TextureAtlas.Mipmaps.Add(mipmapBmp);
                    }

                    var outputFace0 = output.TextureAtlas.Faces[0];
                    while (outputFace0[outputFace0.Count - 1].Width > 1 || outputFace0[outputFace0.Count - 1].Height > 1)
                    {
                        var lastMipmap = outputFace0[outputFace0.Count - 1];
                        var w = Math.Max(1, lastMipmap.Width/2);
                        var h = Math.Max(1, lastMipmap.Height/2);
                        var mipmapBmp = new PixelBitmapContent<Color>(w, h);
                        //PixelBitmapContent<Color>.Copy(lastMipmap, mipmapBmp);
                        output.TextureAtlas.Mipmaps.Add(mipmapBmp);
                    }
                }
                else
                {
                    output.TextureAtlas.GenerateMipmaps(false);
                }
            }
            
            // Workaround MonoGame TextureProcessor bug.
            // MonoGame TextureProcessor overwrites existing mipmaps.
            if (GenerateMipmaps && MipmapsPerSprite)
            {
                GenerateMipmaps = false;
                base.Process(output.TextureAtlas, context);
                GenerateMipmaps = true;
            }
            else
            {
                base.Process(output.TextureAtlas, context);
            }
            
            return output;
        }
        
    }
}