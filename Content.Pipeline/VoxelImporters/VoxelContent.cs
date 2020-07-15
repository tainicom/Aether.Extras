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
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;

namespace tainicom.Aether.Content.Pipeline
{
    public class VoxelContent
    {
        internal Vector3 GridSize = Vector3.Zero;
        internal Vector3 RealSize = Vector3.One;

        internal XYZI[] Voxels;
        internal uint[] Palette;

        internal void MarkSharedSides()
        {
            HashSet<Point3> blocks = new HashSet<Point3>(Voxels.Select<XYZI, Point3>((v) => { return v.Point; }));

            //for(int i = 0; i< Voxels.Length;i++)
            System.Threading.Tasks.Parallel.For(0, Voxels.Length, (i) =>
            {
                var vpt = Voxels[i].Point;

                if (blocks.Contains(new Point3(vpt.X - 1, vpt.Y, vpt.Z)))
                    Voxels[i].SharedSides |= Sides.Left;
                if (blocks.Contains(new Point3(vpt.X + 1, vpt.Y, vpt.Z)))
                    Voxels[i].SharedSides |= Sides.Right;

                if (blocks.Contains(new Point3(vpt.X, vpt.Y - 1, vpt.Z)))
                    Voxels[i].SharedSides |= Sides.Down;
                if (blocks.Contains(new Point3(vpt.X, vpt.Y + 1, vpt.Z)))
                    Voxels[i].SharedSides |= Sides.Up;

                if (blocks.Contains(new Point3(vpt.X, vpt.Y, vpt.Z + 1)))
                    Voxels[i].SharedSides |= Sides.Backward;
                if (blocks.Contains(new Point3(vpt.X, vpt.Y, vpt.Z - 1)))
                    Voxels[i].SharedSides |= Sides.Forward;
            //}
            });

            return;
        }

        internal void RemoveHiddenBlocks()
        {
            var visibleBlocks2 = Voxels.Where((v) => { return v.SharedSides != Sides.All; });
            Voxels = visibleBlocks2.ToArray();

            //HashSet<Point3> blocks = new HashSet<Point3>( Voxels.Select<XYZI, Point3>((v) => { return v.Point; }) );
            //HashSet<Point3> insideBlocks = new HashSet<Point3>();
            
            //for (int z = 1; z < GridSize.Z - 1; z++)
            //{
            //    for (int y = 1; y < GridSize.Y - 1; y++)
            //    {
            //        for (int x = 0; x < GridSize.X - 1; x++)
            //        {
            //            if (blocks.Contains(new Point3(x-1, y, z)) &&
            //                blocks.Contains(new Point3(x+1, y, z)) &&
            //                blocks.Contains(new Point3(x, y-1, z)) &&
            //                blocks.Contains(new Point3(x, y+1, z)) &&
            //                blocks.Contains(new Point3(x, y, z-1)) &&
            //                blocks.Contains(new Point3(x, y, z+1))
            //               )
            //            {
            //                insideBlocks.Add(new Point3(x, y, z));
            //            }
            //        }
            //    }
            //}

            //var visibleBlocks = Voxels.Where((v) => { return !insideBlocks.Contains(v.Point); });
            //Voxels = visibleBlocks.ToArray();

            return;
        }
    }

    internal struct Point3
    {
        public readonly short X, Y, Z;

        public Point3(byte x, byte y, byte z)
        {
            X = x;
            Y = y;
            Z = z;
        }

        public Point3(int x, int y, int z)
        {
            X = (byte)x;
            Y = (byte)y;
            Z = (byte)z;
        }

        public override int GetHashCode()
        {
            return ((X) ^ (Y*256) ^ (Z*(256*256)));
        }

        internal Point3 Add(ref Point3 right)
        {
            return new Point3(
                (short)(X + right.X),
                (short)(Y + right.Y),
                (short)(Z + right.Z)
                );
        }

        internal Vector3 ToVector3()
        {
            Vector3 result;
            result.X = X;
            result.Y = Y;
            result.Z = Z;
            return result;
        }
    }

    internal enum Sides
    {
        None = 0,
        Right = 1,
        Left = 2,
        Up = 4,
        Down = 8,
        Forward = 16,
        Backward = 32,

        All = 63
    }
}
