#region License
//   Copyright 2016 Kastellanos Nikolaos
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
using tainicom.Aether.Content.Pipeline.Atlas;

namespace tainicom.Aether.Content.Pipeline
{
    [ContentProcessor(DisplayName = "AtlasProcessor - Aether")]
    public class AtlasProcessor : TextureProcessor, IContentProcessor
    {        
        private bool _mipmapsPerSprite = true;

#if WINDOWS
        // override InputType
        [Browsable(false)]
#endif
        Type IContentProcessor.InputType { get { return typeof(TextureAtlasContent); } }

#if WINDOWS
        // override OutputType
        [Browsable(false)]
#endif
        Type IContentProcessor.OutputType { get { return typeof(TextureAtlasContent); } }
        

        [DefaultValue(true)]
        public new bool MipmapsPerSprite
        {
            get { return _mipmapsPerSprite; }
            set { _mipmapsPerSprite = value; }
        }

        public AtlasProcessor()
        {
        }
        
        object IContentProcessor.Process(object input, ContentProcessorContext context)
        {
            return Process((TextureAtlasContent)input, context);
        }

        public TextureAtlasContent Process(TextureAtlasContent input, ContentProcessorContext context)
        {
            if (MipmapsPerSprite && GenerateMipmaps)
                foreach (var texture in input.Textures)
                    texture.Texture.GenerateMipmaps(false);

            var output = input;
            
            if (GenerateMipmaps)
            {
                if (MipmapsPerSprite)
                {
                    var maxSpriteWidth = 1;
                    var maxSpriteHeight = 1;
                    foreach (var sprite in input.Textures)
                    {
                        var face0 = sprite.Texture.Faces[0];
                        maxSpriteWidth = Math.Max(maxSpriteWidth, face0[0].Width);
                        maxSpriteHeight = Math.Max(maxSpriteHeight, face0[0].Height);
                    }

                    for (int mipLevel = 1; ; mipLevel++)
                    {
                        int mipLevel2 = (int)Math.Pow(2, mipLevel);
                        Rectangle size = new Rectangle(0, 0, input.Width, input.Height);
                        size.Width /= mipLevel2;
                        size.Height /= mipLevel2;

                        if ((maxSpriteWidth / mipLevel2) < 1 && (maxSpriteHeight / mipLevel2) < 1) break;

                        var mipmapBmp = new PixelBitmapContent<Color>(size.Width, size.Height);
                        foreach (var sprite in input.Sprites)
                        {
                            if (mipLevel >= sprite.Texture.Faces[0].Count) continue;
                            var srcBmp = sprite.Texture.Faces[0][mipLevel];
                            var srcRect = new Rectangle(0, 0, srcBmp.Width, srcBmp.Height);
                            var destRect = sprite.DestinationRectangle;
                            destRect.X = (int)Math.Ceiling((float)destRect.X / mipLevel2);
                            destRect.Y = (int)Math.Ceiling((float)destRect.Y / mipLevel2);
                            destRect.Width = (int)(destRect.Width / mipLevel2);
                            destRect.Height = (int)(destRect.Height / mipLevel2);
                            if (destRect.Width > 1 && destRect.Height > 1)
                                BitmapContent.Copy(srcBmp, srcRect, mipmapBmp, destRect);
                        }
                        output.Mipmaps.Add(mipmapBmp);
                    }

                    var outputFace0 = output.Faces[0];
                    while (outputFace0[outputFace0.Count - 1].Width > 1 || outputFace0[outputFace0.Count - 1].Height > 1)
                    {
                        var lastMipmap = outputFace0[outputFace0.Count - 1];
                        var w = Math.Max(1, lastMipmap.Width/2);
                        var h = Math.Max(1, lastMipmap.Height/2);
                        var mipmapBmp = new PixelBitmapContent<Color>(w, h);
                        //PixelBitmapContent<Color>.Copy(lastMipmap, mipmapBmp);
                        output.Mipmaps.Add(mipmapBmp);
                    }
                }
                else
                {
                    output.GenerateMipmaps(false);
                }
            }
            
            base.Process(output, context);
            
            return output;
        }
        
    }
}