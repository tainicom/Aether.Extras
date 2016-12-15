
using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace Samples.Deferred
{
    public partial class QuadRenderComponent
    {      
        GraphicsDevice graphicsDevice; 
        VertexPositionTexture[] verts = null;
        short[] ib = null;
        
        public QuadRenderComponent(GraphicsDevice graphicsDevice)
        {
            this.graphicsDevice = graphicsDevice;
            
            verts = new VertexPositionTexture[]
                        {
                            new VertexPositionTexture(
                                new Vector3(0,0,0),
                                new Vector2(1,1)),
                            new VertexPositionTexture(
                                new Vector3(0,0,0),
                                new Vector2(0,1)),
                            new VertexPositionTexture(
                                new Vector3(0,0,0),
                                new Vector2(0,0)),
                            new VertexPositionTexture(
                                new Vector3(0,0,0),
                                new Vector2(1,0))
                        };

            ib = new short[] { 0, 1, 2, 2, 3, 0 };

            return;
        }
        
        public void Render(Vector2 v1, Vector2 v2)
        {
            graphicsDevice.BlendState = BlendState.Opaque;

            verts[0].Position.X = v2.X;
            verts[0].Position.Y = v1.Y;

            verts[1].Position.X = v1.X;
            verts[1].Position.Y = v1.Y;

            verts[2].Position.X = v1.X;
            verts[2].Position.Y = v2.Y;

            verts[3].Position.X = v2.X;
            verts[3].Position.Y = v2.Y;

            graphicsDevice.DrawUserIndexedPrimitives<VertexPositionTexture>
                (PrimitiveType.TriangleList, verts, 0, 4, ib, 0, 2);
        }
    }
}
