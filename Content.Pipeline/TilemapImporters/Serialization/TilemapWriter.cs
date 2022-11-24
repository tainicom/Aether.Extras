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

using Microsoft.Xna.Framework.Content.Pipeline;
using Microsoft.Xna.Framework.Content.Pipeline.Graphics;
using Microsoft.Xna.Framework.Content.Pipeline.Serialization.Compiler;
using Microsoft.Xna.Framework.Graphics;
using System;

namespace tainicom.Aether.Content.Pipeline.Serialization
{
    [ContentTypeWriter]
    class TextureAtlasWriter : ContentTypeWriter<TilemapContent>
    {
        protected override void Write(ContentWriter output, TilemapContent atlas)
        {
            output.WriteRawObject((Texture2DContent)atlas.TextureAtlas);

            output.WriteRawObject((Texture2DContent)atlas.TextureMap);

            // write Sprites
            output.Write(atlas.DestinationTiles.Count);
            foreach(var name in atlas.Tiles.Keys)
            {
                var sprite = atlas.Tiles[name];
                output.Write(name);
                output.Write(sprite.DstBounds.X);
                output.Write(sprite.DstBounds.Y);
                output.Write(sprite.DstBounds.Width);
                output.Write(sprite.DstBounds.Height);
            }
            
            return;
        }
        
        public override string GetRuntimeReader(TargetPlatform targetPlatform)
        {
            return "tainicom.Aether.Graphics.Content.TilemapReader, Aether.Tilemap";
        }
        
    }
}
