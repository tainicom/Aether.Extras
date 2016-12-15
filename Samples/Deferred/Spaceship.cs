using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using tainicom.Aether.Shaders;

namespace Samples.Deferred
{
    public class Spaceship
    {
        Model model;
                
        public void Load(ContentManager content)
        {
            model = content.Load<Model>("spaceship");
        }
        
        public void Draw(Matrix world, Matrix view, Matrix projection)
        {
            foreach (ModelMesh mesh in model.Meshes)
            {
                foreach (BasicEffect effect in mesh.Effects)
                {
                    effect.World = world;
                    effect.View = view;
                    effect.Projection = projection;
                    effect.EnableDefaultLighting();

                    effect.DiffuseColor = new Vector3(0.6f);
                    effect.EmissiveColor = new Vector3(0.6f);
                }
                mesh.Draw();
            }
        }

        internal void DrawDeferred(GraphicsDevice device, DeferredBasicEffect deferredBasicEffect, Matrix world, Matrix view, Matrix projection)
        {
            deferredBasicEffect.World = world;
            deferredBasicEffect.View = view;
            deferredBasicEffect.Projection = projection;

            foreach (var mesh in model.Meshes)
            {
                foreach (var part in mesh.MeshParts)
                {
                    deferredBasicEffect.CurrentTechnique.Passes[0].Apply();
                    device.Textures[0] = ((BasicEffect)part.Effect).Texture;

                    device.SetVertexBuffer(part.VertexBuffer);
                    device.Indices = part.IndexBuffer;
                    device.DrawIndexedPrimitives(PrimitiveType.TriangleList, 
                            part.VertexOffset, 0, part.NumVertices,
                            part.StartIndex, part.PrimitiveCount);
                }
            }
        }
    }
}
