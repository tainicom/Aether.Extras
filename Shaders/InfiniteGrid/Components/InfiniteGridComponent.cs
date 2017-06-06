#region License
//   Copyright 2017 Kastellanos Nikolaos
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

namespace tainicom.Aether.Shaders.Components
{
    public partial class InfiniteGridComponent : IDrawable, IGameComponent
    {   
        private readonly GraphicsDevice GraphicsDevice;
        private readonly ContentManager _content;

        private VertexBuffer _quadVertexBuffer;
        private IndexBuffer _quadIndexBuffer;
        private InfiniteGridEffect _gridEffect;

        public int DrawOrder { get; set; }
        public bool Visible { get; set; }
        // TODO: Implement IDrawable Events
        public event EventHandler<EventArgs> DrawOrderChanged;
        public event EventHandler<EventArgs> VisibleChanged;

        public Matrix Projection;
        public Matrix View;
        public Matrix EditMatrix;

        public InfiniteGridComponent(GraphicsDevice graphicsDevice, ContentManager content)
        {
            this.GraphicsDevice = graphicsDevice;
            this._content = content;
        }
                
        public void Initialize()
        {
            this.LoadContent();
        }

        protected void LoadContent()
        {
            _gridEffect = new InfiniteGridEffect(this.GraphicsDevice);
            
            var vertices = new VertexPositionNormalTexture[4];
            vertices[0].Position = new Vector3(-1f, -1f, 1f);
            vertices[1].Position = new Vector3(+1f, -1f, 1f);
            vertices[2].Position = new Vector3(+1f, +1f, 1f);
            vertices[3].Position = new Vector3(-1f, +1f, 1f);
            vertices[0].Normal = Vector3.Forward;
            vertices[1].Normal = Vector3.Forward;
            vertices[2].Normal = Vector3.Forward;
            vertices[3].Normal = Vector3.Forward;
            vertices[0].TextureCoordinate = new Vector2(0,0);
            vertices[1].TextureCoordinate = new Vector2(0,1);
            vertices[2].TextureCoordinate = new Vector2(1,0);
            vertices[3].TextureCoordinate = new Vector2(1,1);
            short[] indices = new short[] { 0, 1, 2, 2, 3, 0 };
            _quadVertexBuffer = new VertexBuffer(GraphicsDevice, VertexPositionNormalTexture.VertexDeclaration, 4, BufferUsage.None);
            _quadIndexBuffer = new IndexBuffer(GraphicsDevice, IndexElementSize.SixteenBits, indices.Length, BufferUsage.None);
            _quadVertexBuffer.SetData(vertices);
            _quadIndexBuffer.SetData(indices);
        }
        
        protected void UnloadContent()
        {
        }

        public virtual void Draw(GameTime gameTime)
        {
            this.GraphicsDevice.BlendState = BlendState.AlphaBlend;
            this.GraphicsDevice.DepthStencilState = DepthStencilState.None;
            this.GraphicsDevice.RasterizerState = RasterizerState.CullNone;
            this.GraphicsDevice.SamplerStates[0] = SamplerState.LinearClamp;

            this._gridEffect.SetDefaultParameters(GraphicsDevice.Viewport, this.Projection, this.View, this.EditMatrix);
            
            this.GraphicsDevice.SetVertexBuffer(_quadVertexBuffer);
            this.GraphicsDevice.Indices = _quadIndexBuffer;

            if (GraphicsDevice.GraphicsProfile == GraphicsProfile.Reach)
            {
                this._gridEffect.CurrentTechnique = this._gridEffect.Techniques["HorzLinesATechnique"];
                this._gridEffect.CurrentTechnique.Passes[0].Apply();
                this.GraphicsDevice.DrawIndexedPrimitives(PrimitiveType.TriangleList, 0, 0, 4, 0, 2);

                this._gridEffect.CurrentTechnique = this._gridEffect.Techniques["VertLinesATechnique"];
                this._gridEffect.CurrentTechnique.Passes[0].Apply();
                this.GraphicsDevice.DrawIndexedPrimitives(PrimitiveType.TriangleList, 0, 0, 4, 0, 2);

                this._gridEffect.CurrentTechnique = this._gridEffect.Techniques["HorzLinesBTechnique"];
                this._gridEffect.CurrentTechnique.Passes[0].Apply();
                this.GraphicsDevice.DrawIndexedPrimitives(PrimitiveType.TriangleList, 0, 0, 4, 0, 2);

                this._gridEffect.CurrentTechnique = this._gridEffect.Techniques["VertLinesBTechnique"];
                this._gridEffect.CurrentTechnique.Passes[0].Apply();
                this.GraphicsDevice.DrawIndexedPrimitives(PrimitiveType.TriangleList, 0, 0, 4, 0, 2);
            }
            else
            {
                this._gridEffect.CurrentTechnique = this._gridEffect.Techniques["GridTechnique"];
                this._gridEffect.CurrentTechnique.Passes[0].Apply();
                this.GraphicsDevice.DrawIndexedPrimitives(PrimitiveType.TriangleList, 0, 0, 4, 0, 2);
            }
            
            return;
        }
        
    }
}
