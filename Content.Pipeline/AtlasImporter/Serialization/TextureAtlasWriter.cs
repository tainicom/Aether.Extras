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
using Microsoft.Xna.Framework.Content.Pipeline;
using Microsoft.Xna.Framework.Content.Pipeline.Graphics;
using Microsoft.Xna.Framework.Content.Pipeline.Serialization.Compiler;
using Microsoft.Xna.Framework.Graphics;

namespace tainicom.Aether.Content.Pipeline.Serialization
{
    [ContentTypeWriter]
    class TextureAtlasWriter : ContentTypeWriter<TextureAtlasContent>
    {
        protected override void Write(ContentWriter output, TextureAtlasContent atlas)
        {
            output.WriteRawObject((Texture2DContent)atlas.Texture);
            
            // write Sprites
            output.Write(atlas.DestinationSprites.Count);
            foreach(var name in atlas.Sprites.Keys)
            {
                var sprite = atlas.Sprites[name];
                output.Write(name);
                output.Write(sprite.Bounds.X);
                output.Write(sprite.Bounds.Y);
                output.Write(sprite.Bounds.Width);
                output.Write(sprite.Bounds.Height);
            }
            
            return;
        }
        
        public override string GetRuntimeReader(TargetPlatform targetPlatform)
        {
            return "tainicom.Aether.Graphics.Content.TextureAtlasReader, Aether.Atlas";
        }
        
    }
}
