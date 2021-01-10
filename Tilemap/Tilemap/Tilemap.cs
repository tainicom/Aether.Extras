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

using System.Collections.Generic;
using Microsoft.Xna.Framework.Graphics;
using tainicom.Aether.Shaders;

namespace tainicom.Aether.Graphics
{
    public class Tilemap
    {
        public Texture2D TextureAtlas { get; internal set; }
        public Texture2D TextureMap { get; internal set; }
        public TilemapEffect Effect { get; internal set;}
        public readonly Dictionary<string, Tile> Sprites;

        internal Tilemap()
        {
            this.Sprites = new Dictionary<string, Tile>();
        }
    }
}
