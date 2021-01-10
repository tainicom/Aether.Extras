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
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using tainicom.Aether.Graphics;
using tainicom.Aether.Shaders;

namespace tainicom.Aether.Graphics.Content
{
    public class TilemapReader : ContentTypeReader<Tilemap>
    {
        protected override Tilemap Read(ContentReader input, Tilemap existingInstance)
        {
            IGraphicsDeviceService graphicsDeviceService = (IGraphicsDeviceService)input.ContentManager.ServiceProvider.GetService(typeof(IGraphicsDeviceService));
            var device = graphicsDeviceService.GraphicsDevice;


            Tilemap output = existingInstance ?? new Tilemap();

            // read standard Texture2D
            output.TextureAtlas = ReadTexture2D(input, output.TextureAtlas);

            output.TextureMap = ReadTexture2D(input, output.TextureMap);

            // read Sprites
            var count = input.ReadInt32();
            for (int i = 0; i < count; i++)
            {
                var name = input.ReadString();
                var bounds = new Rectangle(input.ReadInt32(), input.ReadInt32(), input.ReadInt32(), input.ReadInt32());
                output.Sprites[name] = new Tile(output.TextureAtlas, bounds);
            }

            var tilemapEffect = new TilemapEffect(device);
            tilemapEffect.Alpha = 1f;
            tilemapEffect.VertexColorEnabled = true;
            tilemapEffect.DiffuseColor = new Vector3(1f, 1f, 1f);
            tilemapEffect.World = Matrix.Identity;
            tilemapEffect.View = Matrix.Identity;
            tilemapEffect.Projection = Matrix.Identity;

            tilemapEffect.Texture = output.TextureMap;
            tilemapEffect.TextureAtlas = output.TextureAtlas;
            tilemapEffect.MapSize = new Vector2(output.TextureMap.Width, output.TextureMap.Height);
            tilemapEffect.AtlasSize = new Vector2(
                output.TextureAtlas.Width / output.Sprites["null"].Bounds.Width,
                output.TextureAtlas.Height / output.Sprites["null"].Bounds.Height);
            output.Effect = tilemapEffect;

            return output;
        }

        private Texture2D ReadTexture2D(ContentReader input, Texture2D existingInstance)
        {
            Texture2D output = null;
            try
            {
                output = input.ReadRawObject<Texture2D>(existingInstance);
            }
            catch(NotSupportedException)
            {
                var assembly = typeof(Microsoft.Xna.Framework.Content.ContentTypeReader).Assembly;
                var texture2DReaderType = assembly.GetType("Microsoft.Xna.Framework.Content.Texture2DReader");
                var texture2DReader = (ContentTypeReader)Activator.CreateInstance(texture2DReaderType, true);
                output = input.ReadRawObject<Texture2D>(texture2DReader, existingInstance);
            }
            
            return output;
        }
    }
}
