using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;

namespace Samples.SLMC
{
    public class Game1 : Microsoft.Xna.Framework.Game
    {
        GraphicsDeviceManager graphics;
        SpriteBatch spriteBatch;
        SpriteFont font;

        KeyboardState previousKeyboardState;
        
        int mipLevel = 0;
        Rectangle rtSize;
        RenderTarget2D rt;

        Texture2D tx;
        
        public Game1()
        {
            graphics = new GraphicsDeviceManager(this);
            Content.RootDirectory = "Content";

            graphics.GraphicsProfile = GraphicsProfile.HiDef;
            graphics.PreferredBackBufferWidth = 800;
            graphics.PreferredBackBufferHeight = 480;
        }

        protected override void LoadContent()
        {            
            spriteBatch = new SpriteBatch(GraphicsDevice);
            font = Content.Load<SpriteFont>("font");
            
            tx = Content.Load<Texture2D>("b_c0123");

            rtSize = new Rectangle(0, 0, tx.Width * (1+4), tx.Height);
            graphics.PreferredBackBufferWidth = (int)rtSize.Width;
            graphics.PreferredBackBufferHeight = (int)rtSize.Height;
            graphics.ApplyChanges();

            rt = new RenderTarget2D(GraphicsDevice, rtSize.Width, rtSize.Height);
        }
        
        /// <param name="gameTime">Provides a snapshot of timing values.</param>
        protected override void Update(GameTime gameTime)
        {
            KeyboardState keyState = Keyboard.GetState();
            GamePadState gamePadState = GamePad.GetState(PlayerIndex.One);

            if (keyState.IsKeyDown(Keys.Escape) || gamePadState.Buttons.Back == ButtonState.Pressed)
                Exit();

            if (keyState.IsKeyDown(Keys.OemPlus) && !previousKeyboardState.IsKeyDown(Keys.OemPlus) && mipLevel < tx.LevelCount-1)
                mipLevel++;
            if (keyState.IsKeyDown(Keys.Add) && !previousKeyboardState.IsKeyDown(Keys.Add) && mipLevel < tx.LevelCount-1)
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
            var mipSize = rtSize;
            mipSize.Width /= mipLevel2;
            mipSize.Height /= mipLevel2;

            GraphicsDevice.SetRenderTarget(rt);
            GraphicsDevice.Clear(Color.Black);

            spriteBatch.Begin();
            {
                var destRect = new Rectangle(0, 0, tx.Width, tx.Height);
                destRect.X /= mipLevel2;
                destRect.Y /= mipLevel2;
                destRect.Width /= mipLevel2;
                destRect.Height /= mipLevel2;
                
                // draw all channels
                destRect.X = (tx.Width * 0) / mipLevel2;
                spriteBatch.Draw(tx, destRect, Color.White);

                // draw each channels                 
                destRect.X = (tx.Width * 1) / mipLevel2;
                spriteBatch.Draw(tx, destRect, new Color(1f, 0f, 0f, 0f));
                destRect.X = (tx.Width * 2) / mipLevel2;
                spriteBatch.Draw(tx, destRect, new Color(0f, 1f, 0f, 0f));
                destRect.X = (tx.Width * 3) / mipLevel2;
                spriteBatch.Draw(tx, destRect, new Color(0f, 0f, 1f, 0f));
                destRect.X = (tx.Width * 4) / mipLevel2;
                spriteBatch.Draw(tx, destRect, new Color(0f, 0f, 0f, 1f)); // NOTE: alpha channel is not visible                
            }
            spriteBatch.End();

            
            GraphicsDevice.SetRenderTarget(null);
            GraphicsDevice.Clear(Color.CornflowerBlue);
            spriteBatch.Begin(SpriteSortMode.Deferred, null, SamplerState.PointWrap, null, null);
            spriteBatch.Draw(rt, rtSize, mipSize, Color.White);
            spriteBatch.End();


            spriteBatch.Begin();
            spriteBatch.DrawString(font, String.Format("[+/-] MipLevel - ({0})", mipLevel), new Vector2(11, 11), Color.Black);
            spriteBatch.DrawString(font, String.Format("[+/-] MipLevel - ({0})", mipLevel), new Vector2(10, 10), Color.White);
            spriteBatch.End();

            base.Draw(gameTime);
        }
        
    }
}
