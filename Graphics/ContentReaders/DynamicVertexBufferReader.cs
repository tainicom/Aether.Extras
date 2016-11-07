#region License
//   Copyright 2016 Kastellanos Nikolaos
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

using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;

namespace tainicom.Aether.Graphics.Content
{
    public class DynamicVertexBufferReader : ContentTypeReader<DynamicVertexBuffer>
    {
        protected override DynamicVertexBuffer Read(ContentReader input, DynamicVertexBuffer buffer)
        {   
            IGraphicsDeviceService graphicsDeviceService = (IGraphicsDeviceService)input.ContentManager.ServiceProvider.GetService(typeof(IGraphicsDeviceService));
            var device = graphicsDeviceService.GraphicsDevice;

            // read standard VertexBuffer
            var declaration = input.ReadRawObject<VertexDeclaration>();
            var vertexCount = (int)input.ReadUInt32();
            int dataSize = vertexCount * declaration.VertexStride;
            byte[] data = new byte[dataSize];
            input.Read(data, 0, dataSize);

            // read extras
            bool IsWriteOnly = input.ReadBoolean();
            
            if (buffer == null)
            {
                BufferUsage usage = (IsWriteOnly) ? BufferUsage.WriteOnly : BufferUsage.None;
                buffer = new DynamicVertexBuffer(device, declaration, vertexCount, usage);
            }
            buffer.SetData(data, 0, dataSize);

            return buffer;
        }
    }
}
