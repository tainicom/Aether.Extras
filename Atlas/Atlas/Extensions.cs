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

using Microsoft.Xna.Framework;
using tainicom.Aether.Atlas;

namespace Microsoft.Xna.Framework.Graphics
{
    public static class Extensions
    {
        public static void Draw(this SpriteBatch spriteBatch, Sprite sprite, Vector2 position, Color color)
        {
            spriteBatch.Draw(sprite.Texture, position, sprite.SourceRectangle, color);         
        }
        
        public static void Draw(this SpriteBatch spriteBatch, Sprite sprite, Rectangle destinationRectangle, Color color)
        {
            spriteBatch.Draw(sprite.Texture, destinationRectangle, sprite.SourceRectangle, color);
        }
    }
}
