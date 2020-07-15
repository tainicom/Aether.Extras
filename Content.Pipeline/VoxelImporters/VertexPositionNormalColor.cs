#region License 
//   Copyright 2020 Kastellanos Nikolaos
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
using Microsoft.Xna.Framework.Graphics;

namespace tainicom.Aether.Content.Pipeline
{
    public struct VertexPositionNormalColor : IVertexType, IEquatable<VertexPositionNormalColor>
    {
        public Vector3 Position;
        public Vector3 Normal;
        public Color Color;

        #region IVertexType Members
        public readonly static VertexDeclaration VertexDeclaration = new VertexDeclaration(
                new VertexElement[] 
                {
                    new VertexElement( 0, VertexElementFormat.Vector3, VertexElementUsage.Position, 0),
                    new VertexElement(12, VertexElementFormat.Vector3, VertexElementUsage.Normal, 0),
                    new VertexElement(24, VertexElementFormat.Color, VertexElementUsage.Color, 0)
                });

        VertexDeclaration IVertexType.VertexDeclaration
        {
            get { return VertexDeclaration; }
        }
        #endregion

        public VertexPositionNormalColor(Vector3 position, Vector3 normal, Color color)
        {
            this.Position = position;
            this.Normal = normal;
            this.Color = color;
        }

        public override int GetHashCode()
        {
            return (Position.X.GetHashCode() ^ Position.Y.GetHashCode() ^ Position.Z.GetHashCode()
                  ^ Normal.X.GetHashCode() ^ Normal.Y.GetHashCode() ^ Normal.Z.GetHashCode()
                  ^ Color.GetHashCode()
                 );
        }
        
        public override bool Equals(object obj)
        {
            if (obj is VertexPositionNormalColor)
                return Equals((VertexPositionNormalColor)obj);
            else
                return false;
        }

        public bool Equals(VertexPositionNormalColor other)
        {
            return (Position.X.Equals(other.Position.X) && Position.Y.Equals(other.Position.Y) && Position.Z.Equals(other.Position.Z)
                  && Normal.X.Equals(other.Normal.X) && Normal.Y.Equals(other.Normal.Y) && Normal.Z.Equals(other.Normal.Z)
                  && Color.PackedValue.Equals(other.Color.PackedValue)
                 );
        }

        bool IEquatable<VertexPositionNormalColor>.Equals(VertexPositionNormalColor other)
        {
            return (Position.X.Equals(other.Position.X) && Position.Y.Equals(other.Position.Y) && Position.Z.Equals(other.Position.Z)
                  && Normal.X.Equals(other.Normal.X) && Normal.Y.Equals(other.Normal.Y) && Normal.Z.Equals(other.Normal.Z)
                  && Color.PackedValue.Equals(other.Color.PackedValue)
                 );
        }
    }
}
