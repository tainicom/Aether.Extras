#region License
//   Copyright 2015-2016 Kastellanos Nikolaos
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

using Microsoft.Xna.Framework.Content.Pipeline;
using Microsoft.Xna.Framework.Content.Pipeline.Graphics;
using Microsoft.Xna.Framework.Content.Pipeline.Processors;

namespace tainicom.Aether.Content.Pipeline
{
    [ContentProcessor(DisplayName = "RawModelProcessor - Aether")]
    class RawModelProcessor : ModelProcessor
    {
        public override ModelContent Process(NodeContent input, ContentProcessorContext context)
        {
            ModelContent model = base.Process(input, context);

            foreach(var mesh in model.Meshes)
                foreach(var part in mesh.MeshParts)
                    Proccess(part);

            return model;
        }

        private void Proccess(ModelMeshPartContent part)
        {
            var vertexData = part.VertexBuffer.VertexData;

            object indexData;
            int indexSize = part.IndexBuffer.Count;
            if (part.VertexBuffer.VertexData.Length < short.MaxValue)
            {
                short[] indexData16 = new short[indexSize];
                for (int i = 0; i < indexSize; i++)
                    indexData16[i] = (short)part.IndexBuffer[i];
                indexData = indexData16;
            }
            else
            {   
                int[] indexData32 = new int[indexSize];
                part.IndexBuffer.CopyTo(indexData32, 0);
                indexData = indexData32;
            }

            object[] tag;
            tag = new object[] { vertexData, indexData };

            part.Tag = tag;
        }
    }
}
