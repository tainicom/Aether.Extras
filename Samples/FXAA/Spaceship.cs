using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;

namespace Samples.FXAA
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
    }
}
