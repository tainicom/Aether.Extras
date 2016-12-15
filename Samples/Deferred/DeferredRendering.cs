using System;
using tainicom.Aether.Shaders;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Content;

namespace Samples.Deferred
{
    public class DeferredRendering
    {
        GraphicsDevice _graphicsDevice;

        private QuadRenderComponent quadRenderer;

        public RenderTarget2D colorRT;  // color and specular intensity
        public RenderTarget2D normalRT; // normals + specular power
        public RenderTarget2D depthRT;  // depth
        public RenderTarget2D lightRT;  // lighting

        public DeferredBasicEffect basicEffect;
        private DeferredClearGBufferEffect clearBufferEffect;
        private DeferredPointLightEffect pointLightEffect;
        private DeferredSpotLightEffect spotLightEffect;
        private DeferredCombineEffect combineEffect;
        private Model sphereModel; //point ligt volume
        private Vector2 halfPixel;
        
        public DeferredRendering(GraphicsDevice graphicsDevice, ContentManager content)
        {
            _graphicsDevice = graphicsDevice;

            quadRenderer = new QuadRenderComponent(graphicsDevice);

            halfPixel = new Vector2()
            {
                X = 0.5f / (float)graphicsDevice.PresentationParameters.BackBufferWidth,
                Y = 0.5f / (float)graphicsDevice.PresentationParameters.BackBufferHeight
            };

            int backbufferWidth = graphicsDevice.PresentationParameters.BackBufferWidth;
            int backbufferHeight = graphicsDevice.PresentationParameters.BackBufferHeight;
            colorRT = new RenderTarget2D(graphicsDevice, backbufferWidth, backbufferHeight, false, SurfaceFormat.Color, DepthFormat.Depth24);
            normalRT = new RenderTarget2D(graphicsDevice, backbufferWidth, backbufferHeight, false, SurfaceFormat.Color, DepthFormat.None);
            depthRT = new RenderTarget2D(graphicsDevice, backbufferWidth, backbufferHeight, false, SurfaceFormat.Single, DepthFormat.None);
            lightRT = new RenderTarget2D(graphicsDevice, backbufferWidth, backbufferHeight, false, SurfaceFormat.Color, DepthFormat.None);

            basicEffect = new DeferredBasicEffect(graphicsDevice);
            clearBufferEffect = new DeferredClearGBufferEffect(graphicsDevice);
            combineEffect = new DeferredCombineEffect(graphicsDevice);
            pointLightEffect = new DeferredPointLightEffect(graphicsDevice);
            spotLightEffect = new DeferredSpotLightEffect(graphicsDevice);
            sphereModel = content.Load<Model>(@"sphere");           
        }
                
        internal void DrawRTs(SpriteBatch spriteBatch)
        {
            float scale = 1f / 8f;
            float height = colorRT.Height * scale;
            Vector2 pos = new Vector2(0, height);
            Vector2 off = new Vector2(0, height*4);
            spriteBatch.Draw(colorRT, pos * 0 + off, null, Color.White, 0f, Vector2.Zero, scale, SpriteEffects.None, 0);
            spriteBatch.Draw(normalRT,pos * 1 + off, null, Color.White, 0f, Vector2.Zero, scale, SpriteEffects.None, 0);
            spriteBatch.Draw(depthRT, pos * 2 + off, null, Color.White, 0f, Vector2.Zero, scale, SpriteEffects.None, 0);
            spriteBatch.Draw(lightRT, pos * 3 + off, null, Color.White, 0f, Vector2.Zero, scale, SpriteEffects.None, 0);
        }

        internal void SetGBuffer()
        {
            _graphicsDevice.SetRenderTargets(colorRT, normalRT, depthRT);
        }

        internal void ClearGBuffer()
        {
            _graphicsDevice.Clear(Color.Black);
            clearBufferEffect.CurrentTechnique.Passes[0].Apply();
            quadRenderer.Render(Vector2.One * -1, Vector2.One);
        }

        internal void ResolveGBuffer()
        {
            _graphicsDevice.SetRenderTargets(null);
        }

        internal void Combine()
        {
            _graphicsDevice.BlendState = BlendState.Opaque;
            _graphicsDevice.DepthStencilState = DepthStencilState.None;
            _graphicsDevice.RasterizerState = RasterizerState.CullCounterClockwise;
            _graphicsDevice.SetRenderTarget(null);
            _graphicsDevice.Clear(Color.Black);
            //Combine everything
            combineEffect.ColorMap = colorRT;
            combineEffect.LightMap = lightRT;
            combineEffect.HalfPixel = halfPixel;
            combineEffect.CurrentTechnique.Passes[0].Apply();
            quadRenderer.Render(Vector2.One * -1, Vector2.One);
        }


        public void DrawPointLight(Vector3 lightPosition, Color color, float lightRadius, float lightIntensity, Matrix view, Matrix projection, Vector3 cameraPosition)
        {
            //set the G-Buffer parameters
            pointLightEffect.ColorMap = colorRT;
            pointLightEffect.NormalMap = normalRT;
            pointLightEffect.DepthMap = depthRT;

            //compute the light world matrix
            //scale according to light radius, and translate it to light position
            Matrix sphereWorldMatrix = Matrix.CreateScale(lightRadius) * Matrix.CreateTranslation(lightPosition);
            pointLightEffect.World = sphereWorldMatrix;
            pointLightEffect.View = view;
            pointLightEffect.Projection = projection;
            //light position
            pointLightEffect.LightPosition = lightPosition;

            //set the color, radius and Intensity
            pointLightEffect.Color = color.ToVector3();
            pointLightEffect.LightRadius = lightRadius;
            pointLightEffect.LightIntensity = lightIntensity;

            //parameters for specular computations
            pointLightEffect.CameraPosition = cameraPosition;
            pointLightEffect.InvertViewProjection = Matrix.Invert(view * projection);
            //size of a halfpixel, for texture coordinates alignment
            pointLightEffect.HalfPixel = halfPixel;

            _graphicsDevice.RasterizerState = RasterizerState.CullNone;
            _graphicsDevice.DepthStencilState = DepthStencilState.None;
            _graphicsDevice.BlendState = BlendState.AlphaBlend;

            pointLightEffect.CurrentTechnique.Passes[0].Apply();
            foreach (ModelMesh mesh in sphereModel.Meshes)
            {
                foreach (ModelMeshPart meshPart in mesh.MeshParts)
                {
                    _graphicsDevice.Indices = meshPart.IndexBuffer;
                    _graphicsDevice.SetVertexBuffer(meshPart.VertexBuffer);

                    _graphicsDevice.DrawIndexedPrimitives(PrimitiveType.TriangleList, 0, 0, meshPart.NumVertices, meshPart.StartIndex, meshPart.PrimitiveCount);
                }
            }

            _graphicsDevice.RasterizerState = RasterizerState.CullCounterClockwise;
            _graphicsDevice.DepthStencilState = DepthStencilState.Default;
            _graphicsDevice.BlendState = BlendState.Opaque;
        }

        public void DrawSpotLight(Vector3 lightPosition, Color color, float lightRadius, float lightIntensity,
            Vector3 lightDirection, float innerAngle, float outerAngle, Matrix view, Matrix projection, Vector3 cameraPosition)
        {
            //set the G-Buffer parameters
            spotLightEffect.ColorMap = colorRT;
            spotLightEffect.NormalMap = normalRT;
            spotLightEffect.DepthMap = depthRT;

            //compute the light world matrix
            //scale according to light radius, and translate it to light position
            Matrix sphereWorldMatrix = Matrix.CreateScale(lightRadius) * Matrix.CreateTranslation(lightPosition);
            spotLightEffect.World = sphereWorldMatrix;
            spotLightEffect.View = view;
            spotLightEffect.Projection = projection;

            //light position
            spotLightEffect.LightPosition = lightPosition;

            //light direction
            spotLightEffect.LightDirection = lightDirection;

            //spot light
            if (innerAngle == outerAngle) innerAngle -= innerAngle / 360f;
            spotLightEffect.InnerAngleCos = (float)Math.Cos(innerAngle);
            spotLightEffect.OuterAngleCos = (float)Math.Cos(outerAngle);

            //set the color, radius and Intensity
            spotLightEffect.Color = color.ToVector3();
            spotLightEffect.LightRadius = lightRadius;
            spotLightEffect.LightIntensity = lightIntensity;

            //parameters for specular computations
            spotLightEffect.CameraPosition = cameraPosition;
            spotLightEffect.InvertViewProjection = Matrix.Invert(view * projection);
            //size of a halfpixel, for texture coordinates alignment
            spotLightEffect.HalfPixel = halfPixel;

            _graphicsDevice.RasterizerState = RasterizerState.CullNone;
            _graphicsDevice.DepthStencilState = DepthStencilState.None;
            _graphicsDevice.BlendState = BlendState.AlphaBlend;

            spotLightEffect.CurrentTechnique.Passes[0].Apply();
            foreach (ModelMesh mesh in sphereModel.Meshes)
            {
                foreach (ModelMeshPart meshPart in mesh.MeshParts)
                {
                    _graphicsDevice.Indices = meshPart.IndexBuffer;
                    _graphicsDevice.SetVertexBuffer(meshPart.VertexBuffer);

                    _graphicsDevice.DrawIndexedPrimitives(PrimitiveType.TriangleList, 0, 0, meshPart.NumVertices, meshPart.StartIndex, meshPart.PrimitiveCount);
                }
            }

            _graphicsDevice.RasterizerState = RasterizerState.CullCounterClockwise;
            _graphicsDevice.DepthStencilState = DepthStencilState.Default;
            _graphicsDevice.BlendState = BlendState.Opaque;
        }

    }
}
