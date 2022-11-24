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


using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content.Pipeline.Graphics;

namespace tainicom.Aether.Content.Pipeline
{
    public class TileContent
    {
        internal TilesetContent Tileset;
        internal int Id;
        public TextureContent SrcTexture;
        public Rectangle SrcBounds;

        public Rectangle DstBounds;

        public TileContent()
        {
            this.Tileset = null;
            this.Id = -1;
            this.SrcTexture = null;
            this.SrcBounds = Rectangle.Empty;
            this.DstBounds = Rectangle.Empty;
        }

        public TileContent(TileContent other)
        {
            this.Tileset = other.Tileset;
            this.Id = other.Id;
            this.SrcTexture = other.SrcTexture;
            this.SrcBounds = other.SrcBounds;
            this.DstBounds = other.DstBounds;
        }

        public override string ToString()
        {
            return string.Format("{{SrcBounds:{0}, DstBounds{1}}}", SrcBounds, DstBounds);
        }
    }
}
