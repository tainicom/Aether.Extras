using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using tainicom.Aether.Atlas;

namespace Samples.Atlas
{
    public class Game1 : Microsoft.Xna.Framework.Game
    {
        GraphicsDeviceManager graphics;
        SpriteBatch spriteBatch;
        SpriteFont font;

        KeyboardState previousKeyboardState;

        int mipLevel = 4;
        bool showAtlas = false;
        bool useGenerateBitmap = true;
        bool useMipmapPerSprite = true;
        Rectangle atlasSize = new Rectangle(0, 0, 1024, 512);
        RenderTarget2D rt;


        TextureAtlas atlasMipmapPerSprite;
        TextureAtlas atlasMipmap;
        TextureAtlas atlasNoMipmap;
        
        public Game1()
        {
            graphics = new GraphicsDeviceManager(this);
            Content.RootDirectory = "Content";

            graphics.PreferredBackBufferWidth = atlasSize.Width;
            graphics.PreferredBackBufferHeight = atlasSize.Height;
        }

        protected override void LoadContent()
        {            
            spriteBatch = new SpriteBatch(GraphicsDevice);
            font = Content.Load<SpriteFont>("font");

            rt = new RenderTarget2D(GraphicsDevice, atlasSize.Width, atlasSize.Height);

            atlasMipmapPerSprite = Content.Load<TextureAtlas>("atlasMipmapPerSprite");
            atlasMipmap = Content.Load<TextureAtlas>("atlasMipmap");
            atlasNoMipmap = Content.Load<TextureAtlas>("atlasNoMipmap");
        }
        
        /// <param name="gameTime">Provides a snapshot of timing values.</param>
        protected override void Update(GameTime gameTime)
        {
            KeyboardState keyState = Keyboard.GetState();
            GamePadState gamePadState = GamePad.GetState(PlayerIndex.One);

            if (keyState.IsKeyDown(Keys.Escape) || gamePadState.Buttons.Back == ButtonState.Pressed)
                Exit();

            if (keyState.IsKeyDown(Keys.F1) && !previousKeyboardState.IsKeyDown(Keys.F1))
                useMipmapPerSprite = !useMipmapPerSprite;
            if (keyState.IsKeyDown(Keys.F2) && !previousKeyboardState.IsKeyDown(Keys.F2))
                useGenerateBitmap = !useGenerateBitmap;
            if (keyState.IsKeyDown(Keys.F3) && !previousKeyboardState.IsKeyDown(Keys.F3))
                showAtlas = !showAtlas;
            if (keyState.IsKeyDown(Keys.OemPlus) && !previousKeyboardState.IsKeyDown(Keys.OemPlus) && mipLevel < 10)
                mipLevel++;
            if (keyState.IsKeyDown(Keys.Add) && !previousKeyboardState.IsKeyDown(Keys.Add) && mipLevel < 10)
                mipLevel++;
            if (keyState.IsKeyDown(Keys.OemMinus) && !previousKeyboardState.IsKeyDown(Keys.OemMinus) && mipLevel > 0)
                mipLevel--;
            if (keyState.IsKeyDown(Keys.Subtract) && !previousKeyboardState.IsKeyDown(Keys.Subtract) && mipLevel > 0)
                mipLevel--;
            
            previousKeyboardState = keyState;
            base.Update(gameTime);
        }
        
        protected override void Draw(GameTime gameTime)
        {
            GraphicsDevice.BlendState = BlendState.Opaque;
            GraphicsDevice.DepthStencilState = DepthStencilState.Default;

            int mipLevel2 = (int)Math.Pow(2, mipLevel);
            var mipSize = atlasSize;
            mipSize.Width /= mipLevel2;
            mipSize.Height /= mipLevel2;
            
            GraphicsDevice.SetRenderTarget(rt);
            GraphicsDevice.Clear(Color.Black);

            var currentAtlas = (useGenerateBitmap) ? (useMipmapPerSprite ? atlasMipmapPerSprite : atlasMipmap) : atlasNoMipmap;

            spriteBatch.Begin();

            if (showAtlas)
            {
                spriteBatch.Draw(currentAtlas.Texture, mipSize, Color.White);
            }
            else
            {
                var sprite18 = currentAtlas.Sprites["18"];
                var destRect = new Rectangle(128, 128, sprite18.SourceRectangle.Width, sprite18.SourceRectangle.Height);
                destRect.X /= mipLevel2;
                destRect.Y /= mipLevel2;
                destRect.Width = Math.Max(1, destRect.Width / mipLevel2);
                destRect.Height = Math.Max(1, destRect.Height / mipLevel2);
                spriteBatch.Draw(sprite18, destRect, Color.White);

                var spriteMushroom_2 = currentAtlas.Sprites["Mushroom_2"];
                destRect = new Rectangle(256 + 128, 128, spriteMushroom_2.SourceRectangle.Width, spriteMushroom_2.SourceRectangle.Height);
                destRect.X /= mipLevel2;
                destRect.Y /= mipLevel2;
                destRect.Width = Math.Max(1, destRect.Width / mipLevel2);
                destRect.Height = Math.Max(1, destRect.Height / mipLevel2);
                spriteBatch.Draw(spriteMushroom_2, destRect, Color.White);
                
                var sprite10 = currentAtlas.Sprites["10"];
                destRect = new Rectangle(512, 128, sprite10.SourceRectangle.Width, sprite10.SourceRectangle.Height);
                destRect.X /= mipLevel2;
                destRect.Y /= mipLevel2;
                destRect.Width = Math.Max(1, destRect.Width / mipLevel2);
                destRect.Height = Math.Max(1, destRect.Height / mipLevel2);
                spriteBatch.Draw(sprite10, destRect, Color.White);

            }
            spriteBatch.End();


            GraphicsDevice.SetRenderTarget(null);
            GraphicsDevice.Clear(Color.CornflowerBlue);
            spriteBatch.Begin(SpriteSortMode.Deferred, null, SamplerState.PointWrap, null, null);
            spriteBatch.Draw(rt, atlasSize, mipSize, Color.White);
            spriteBatch.End();

            spriteBatch.Begin();
            spriteBatch.DrawString(font, String.Format("[F1] MipmapPerSprite - ({0})", useMipmapPerSprite ? "ON" : "OFF"), new Vector2(20, 20), Color.White);
            spriteBatch.DrawString(font, String.Format("[F2] GenerateMipmap - ({0})", useGenerateBitmap ? "ON" : "OFF"), new Vector2(20, 40), Color.White);
            spriteBatch.DrawString(font, String.Format("[F3] {0}", showAtlas? "Show Sprites" : "Show Atlas"), new Vector2(20, 60), Color.White);
            spriteBatch.DrawString(font, String.Format("[+/-] MipLevel - ({0})", mipLevel), new Vector2(20, 80), Color.White);
            spriteBatch.End();

            base.Draw(gameTime);
        }
        
    }
}
