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

using System.Collections.Generic;
using Microsoft.Xna.Framework.Content.Pipeline;
using Microsoft.Xna.Framework.Content.Pipeline.Graphics;

namespace tainicom.Aether.Content.Pipeline
{
    public class TextureAtlasContent : Texture2DContent
    {
        public Texture2DContent Texture { get { return this; } }
        public readonly Dictionary<string, SpriteContent> Sprites = new Dictionary<string, SpriteContent>();

        internal readonly List<SpriteContent> SourceSprites = new List<SpriteContent>();
        internal readonly List<SpriteContent> DestinationSprites = new List<SpriteContent>();

        internal int MapColumns, MapRows;
        internal int TileWidth, TileHeight;
        internal int Width, Height;

        internal string Renderorder;
        internal int Firstgid;
        internal int LayerColumns, LayerRows;
        internal int[] MapData;
        
        
    }
}
