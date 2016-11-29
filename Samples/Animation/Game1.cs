using System;
using System.Diagnostics;
using System.Globalization;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using tainicom.Aether.Animation;

namespace Samples.Animation
{
    public class Game1 : Microsoft.Xna.Framework.Game
    {
        GraphicsDeviceManager graphics;
        SpriteBatch spriteBatch;
        SpriteFont font;
        Model _model;
        Animations _animations;

        public Game1()
        {
            graphics = new GraphicsDeviceManager(this);
            Content.RootDirectory = "Content";
        }

        protected override void Initialize()
        {
            base.Initialize();
        }

        protected override void LoadContent()
        {
            spriteBatch = new SpriteBatch(GraphicsDevice);
            font = Content.Load<SpriteFont>("font");
            
            _model = Content.Load<Model>("Dude/dude");

            _animations = _model.GetAnimations();
            var clip = _animations.Clips["Take 001"];
            _animations.SetClip(clip);
        }

        protected override void UnloadContent()
        {
        }

        protected override void Update(GameTime gameTime)
        {
            // Allows the game to exit
            if (GamePad.GetState(PlayerIndex.One).Buttons.Back == ButtonState.Pressed)
                this.Exit();

            _animations.Update(gameTime.ElapsedGameTime, true, Matrix.Identity);

            base.Update(gameTime);
        }

        private Vector3 Position = Vector3.Zero;
        private float Zoom = 100f;
        private float RotationY = 0.0f;
        private float RotationX = 0.0f;
        private Matrix gameWorldRotation = Matrix.Identity;


        Stopwatch sw = new Stopwatch();
        double msecMin = double.MaxValue;
        double msecMax = 0;
        double avg = 0;
        double acc = 0;
        int c;

        protected override void Draw(GameTime gameTime)
        {
            GraphicsDevice.Clear(Color.Black);
            
            GraphicsDevice.BlendState = BlendState.Opaque;
            GraphicsDevice.RasterizerState = RasterizerState.CullCounterClockwise;
            GraphicsDevice.DepthStencilState = DepthStencilState.Default;
            GraphicsDevice.SamplerStates[0] = SamplerState.LinearWrap;

            var m = _model;

            float aspectRatio = graphics.GraphicsDevice.Viewport.AspectRatio;
            Matrix[] transforms = new Matrix[m.Bones.Count];
            m.CopyAbsoluteBoneTransformsTo(transforms);
            Matrix projection = Matrix.CreatePerspectiveFieldOfView(MathHelper.ToRadians(45.0f), aspectRatio, 0.01f, 500.0f);
            Matrix view = Matrix.CreateLookAt(
                new Vector3(0.0f, 35.0f, -Zoom),
                new Vector3(0.0f, 35.0f, 0), 
                Vector3.Up);

            sw.Reset();
            sw.Start();
            foreach (ModelMesh mesh in m.Meshes)
            {
                foreach (var part in mesh.MeshParts)
                {
                    ((BasicEffect)part.Effect).SpecularColor = Vector3.Zero;
                    ConfigureEffectMatrices((IEffectMatrices)part.Effect, Matrix.Identity, view, projection);
                    ConfigureEffectLighting((IEffectLights)part.Effect);

                    // animate vertices on CPU
                    part.UpdateVertices(_animations.AnimationTransforms);
                }
                mesh.Draw();
            }
            sw.Stop();

            double msec = sw.Elapsed.TotalMilliseconds;
            msecMin = Math.Min(msecMin, msec);
            if(avg != 0 )
                msecMax = Math.Max(msecMax, msec);
            acc += msec; c++;
            if(c>60*2)
            {
                avg = acc/c;
                acc = c = 0;
            }

            spriteBatch.Begin();
            spriteBatch.DrawString(font, msec.ToString("#0.000",CultureInfo.InvariantCulture) + "ms", new Vector2(32, GraphicsDevice.Viewport.Height - 130), Color.White);
            spriteBatch.DrawString(font, avg.ToString("#0.000",CultureInfo.InvariantCulture) + "ms (avg)", new Vector2(32, GraphicsDevice.Viewport.Height - 100), Color.White);
            spriteBatch.DrawString(font, msecMin.ToString("#0.000",CultureInfo.InvariantCulture) + "ms (min)", new Vector2(32, GraphicsDevice.Viewport.Height - 70), Color.White);
            spriteBatch.DrawString(font, msecMax.ToString("#0.000",CultureInfo.InvariantCulture) + "ms (max)", new Vector2(32, GraphicsDevice.Viewport.Height - 40), Color.White);
            spriteBatch.End();


            base.Draw(gameTime);
        }

        private void ConfigureEffectMatrices(IEffectMatrices effect, Matrix world, Matrix view, Matrix projection)
        {
            effect.World = world;
            effect.View = view;
            effect.Projection = projection;
        }

        private void ConfigureEffectLighting(IEffectLights effect)
        {
            effect.EnableDefaultLighting();
            effect.DirectionalLight0.Direction = Vector3.Backward;
            effect.DirectionalLight0.Enabled = true;
            effect.DirectionalLight1.Enabled = false;
            effect.DirectionalLight2.Enabled = false;
        }

    }
}
