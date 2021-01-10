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
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content.Pipeline;
using Microsoft.Xna.Framework.Content.Pipeline.Graphics;

namespace tainicom.Aether.Content.Pipeline
{
    public class TilemapContent : Texture2DContent
    {
        public Texture2DContent TextureAtlas { get { return this; } }
        public Texture2DContent TextureMap;
        public readonly Dictionary<string, TileContent> Tiles = new Dictionary<string, TileContent>();
        
        public TilesetContent Tileset;
        internal readonly List<TileContent> DestinationTiles = new List<TileContent>();

        internal int MapColumns, MapRows;
        internal int TileWidth, TileHeight;
        internal int Width, Height;

        internal string Renderorder;
        internal int Firstgid;
        internal int LayerColumns, LayerRows;
        internal int[] MapData;



        internal static void PackTiles(TilemapContent output, int tileWidth, int tileHeight)
        {
            var dstTiles = TilePacker.ArrangeGlyphs(output.Tileset.SourceTiles, tileWidth, tileHeight, true, true);

            foreach (var dstTile in dstTiles)
            {
                output.DestinationTiles.Add(dstTile);
                var name = dstTile.SrcTexture.Name;
                if (output.Tiles.ContainsKey(name))
                    name = name + dstTile.Id;
                output.Tiles.Add(name, dstTile);
            }
        }
        
        internal static void RenderAtlas(TilemapContent output)
        {
            Rectangle s = new Rectangle(0,0,1,1);
            foreach (var dstTile in output.DestinationTiles)
            {
                s = Rectangle.Union(s,dstTile.DstBounds);
            }

            var outputBmp = new PixelBitmapContent<Color>(s.Width, s.Height);

            foreach (var dstTile in output.DestinationTiles)
            {
                var srcBounds = dstTile.SrcBounds;
                var dstBounds = new Rectangle(dstTile.DstBounds.X, dstTile.DstBounds.Y, srcBounds.Width, srcBounds.Height);
                var offsetX = 0;
                var offsetY = dstTile.DstBounds.Height - srcBounds.Height;
                dstBounds.X += offsetX;
                dstBounds.Y += offsetY;
                var srcBmp = dstTile.SrcTexture.Faces[0][0];
                BitmapContent.Copy(srcBmp, srcBounds, outputBmp, dstBounds);
            }
            var mipmapChain = new MipmapChain(outputBmp);
            output.TextureAtlas.Mipmaps = mipmapChain;
        }

        internal static void RenderMap(TilemapContent output)
        {
            var mp = new byte[output.MapData.Length * 4];
            for (int i = 0; i < output.MapData.Length; i++)
            {
                var id = output.MapData[i] - output.Firstgid;
                TileContent tile = null;
                foreach (var t in output.DestinationTiles)
                {
                    if (t.Id == id)
                    {
                        tile = t;
                        break;
                    }
                }

                byte x = (byte)(tile.DstBounds.X / output.TileWidth);
                byte y = (byte)(tile.DstBounds.Y / output.TileHeight);

                mp[i * 4 + 0] = x;
                mp[i * 4 + 1] = y;
                mp[i * 4 + 2] = 0;
                mp[i * 4 + 3] = 255;
            }

            BitmapContent bm = new PixelBitmapContent<Color>(output.MapColumns, output.MapRows);
            bm.SetPixelData(mp);
            Texture2DContent mt = new Texture2DContent();
            mt.Faces[0].Add(bm);
            output.TextureMap = mt;
        }

    }
}
