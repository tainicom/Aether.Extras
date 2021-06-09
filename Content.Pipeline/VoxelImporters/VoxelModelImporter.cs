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

using System;
using System.Collections.Generic;
using System.IO;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content.Pipeline;
using Microsoft.Xna.Framework.Content.Pipeline.Graphics;
using Microsoft.Xna.Framework.Graphics;

namespace tainicom.Aether.Content.Pipeline
{
    [ContentImporter(".vox", DisplayName = "Voxel Model Importer - Aether", DefaultProcessor = "ModelProcessor")]
    public class VoxelModelImporter : ContentImporter<NodeContent>
    {
        public override NodeContent Import(string filename, ContentImporterContext context)
        {
            VoxelContent voxel = null;

            if (Path.GetExtension(filename) == ".vox")
            {
                VoxImporter voxelImporter = new VoxImporter();
                voxel = voxelImporter.ImportVox(filename, context);
            }
            else
            {
                throw new InvalidContentException("File type not supported.");
            }
            
            voxel.MarkSharedSides();

            voxel.RemoveHiddenBlocks();
            
            NodeContent output = VoxelProcess(voxel, context);
            
            return output;
        }

        /// <summary>
        /// Import a VOX file as Model
        /// </summary>
        private NodeContent VoxelProcess(VoxelContent voxel, ContentImporterContext context)
        {
            XYZI[] voxels = voxel.Voxels;
            uint[] palette = voxel.Palette;

            var scale = voxel.RealSize / voxel.GridSize;
            Vector3 centerOffset = new Vector3(1f, 1f, 1f) * (voxel.RealSize / -2f);

            var corner000 = new Point3(0, 0, 0);
            var corner100 = new Point3(1, 0, 0);
            var corner010 = new Point3(0, 1, 0);
            var corner110 = new Point3(1, 1, 0);
            var corner001 = new Point3(0, 0, 1);
            var corner101 = new Point3(1, 0, 1);
            var corner011 = new Point3(0, 1, 1);
            var corner111 = new Point3(1, 1, 1);


            var Forward = Vector3.Forward;
            var Backward = Vector3.Backward;
            var Left = Vector3.Left;
            var Right = Vector3.Right;
            var Up = Vector3.Up;
            var Down = Vector3.Down;

            for (int i = 0; i < voxels.Length; i++)
            {
                var pt000 = voxels[i].Point.Add(ref corner000);
                var pt100 = voxels[i].Point.Add(ref corner100);
                var pt010 = voxels[i].Point.Add(ref corner010);
                var pt110 = voxels[i].Point.Add(ref corner110);
                var pt001 = voxels[i].Point.Add(ref corner001);
                var pt101 = voxels[i].Point.Add(ref corner101);
                var pt011 = voxels[i].Point.Add(ref corner011);
                var pt111 = voxels[i].Point.Add(ref corner111);

                // back
                var p0 = pt000.ToVector3();
                var p1 = pt100.ToVector3();
                var p2 = pt010.ToVector3();
                var p3 = pt110.ToVector3();

                // front
                var p4 = pt001.ToVector3();
                var p5 = pt101.ToVector3();
                var p6 = pt011.ToVector3();
                var p7 = pt111.ToVector3();

                Vector3.Multiply(ref p0, ref scale, out p0); Vector3.Add(ref p0, ref centerOffset, out p0);
                Vector3.Multiply(ref p1, ref scale, out p1); Vector3.Add(ref p1, ref centerOffset, out p1);
                Vector3.Multiply(ref p2, ref scale, out p2); Vector3.Add(ref p2, ref centerOffset, out p2);
                Vector3.Multiply(ref p3, ref scale, out p3); Vector3.Add(ref p3, ref centerOffset, out p3);
                Vector3.Multiply(ref p4, ref scale, out p4); Vector3.Add(ref p4, ref centerOffset, out p4);
                Vector3.Multiply(ref p5, ref scale, out p5); Vector3.Add(ref p5, ref centerOffset, out p5);
                Vector3.Multiply(ref p6, ref scale, out p6); Vector3.Add(ref p6, ref centerOffset, out p6);
                Vector3.Multiply(ref p7, ref scale, out p7); Vector3.Add(ref p7, ref centerOffset, out p7);

                vertex.Color.PackedValue = palette[voxels[i].ColorIndex];

                if ((voxels[i].SharedSides & Sides.Forward) == 0)
                {
                    vertex.Normal = Forward;
                    AddVertex(ref p1);
                    AddVertex(ref p3);
                    AddVertex(ref p0);

                    AddVertex(ref p0);
                    AddVertex(ref p3);
                    AddVertex(ref p2);
                }
                if ((voxels[i].SharedSides & Sides.Backward) == 0)
                {
                    vertex.Normal = Backward;
                    AddVertex(ref p4);
                    AddVertex(ref p6);
                    AddVertex(ref p5);

                    AddVertex(ref p5);
                    AddVertex(ref p6);
                    AddVertex(ref p7);
                }

                if ((voxels[i].SharedSides & Sides.Left) == 0)
                {
                    vertex.Normal = Left;
                    AddVertex(ref p2);
                    AddVertex(ref p6);
                    AddVertex(ref p0);

                    AddVertex(ref p0);
                    AddVertex(ref p6);
                    AddVertex(ref p4);
                }
                if ((voxels[i].SharedSides & Sides.Right) == 0)
                {
                    vertex.Normal = Right;
                    AddVertex(ref p1);
                    AddVertex(ref p5);
                    AddVertex(ref p3);

                    AddVertex(ref p3);
                    AddVertex(ref p5);
                    AddVertex(ref p7);
                }

                if ((voxels[i].SharedSides & Sides.Up) == 0)
                {
                    vertex.Normal = Up;
                    AddVertex(ref p7);
                    AddVertex(ref p6);
                    AddVertex(ref p3);

                    AddVertex(ref p3);
                    AddVertex(ref p6);
                    AddVertex(ref p2);
                }
                if ((voxels[i].SharedSides & Sides.Down) == 0)
                {
                    vertex.Normal = Down;
                    AddVertex(ref p5);
                    AddVertex(ref p1);
                    AddVertex(ref p4);

                    AddVertex(ref p4);
                    AddVertex(ref p1);
                    AddVertex(ref p0);
                }
            }

            MeshContent mesh = new MeshContent();
            mesh.Name = "voxel";

            for (int pi = 0; pi < this.vertices.Count; pi++)
            {
                mesh.Positions.Add(this.vertices[pi].Position);
            }

            var geom = new GeometryContent();
            mesh.Geometry.Add(geom);
            BasicMaterialContent material = new BasicMaterialContent();
            geom.Material = material;

            for (int pi = 0; pi < this.vertices.Count; pi++)
            {
                geom.Vertices.Add(pi);
            }

            for (int ii = 0; ii < this.indices.Count; ii++)
            {
                geom.Indices.Add(this.indices[ii]);
            }

            List<Vector3> normals = new List<Vector3>();
            List<Color> colors = new List<Color>();

            for (int vi = 0; vi < this.vertices.Count; vi++)
            {
                var vertex = vertices[vi];
                normals.Add(vertex.Normal);
                colors.Add(vertex.Color);
            }

            geom.Vertices.Channels.Add<Vector3>(VertexChannelNames.Normal(0), normals);
            geom.Vertices.Channels.Add<Color>(VertexChannelNames.Color(0), colors);

            return mesh;
        }

        List<VertexPositionNormalColor> vertices = new List<VertexPositionNormalColor>();
        List<int> indices = new List<int>();
        Dictionary<VertexPositionNormalColor, int> vertexMap = new Dictionary<VertexPositionNormalColor, int>();
        VertexPositionNormalColor vertex;

        private int AddVertex(ref Vector3 position)
        {
            vertex.Position = position;

            int vertIndx;
            if (!vertexMap.TryGetValue(vertex, out vertIndx))
            {
                vertIndx = vertices.Count;
                vertices.Add(vertex);
                vertexMap.Add(vertex, vertIndx);
            }

            indices.Add(vertIndx);

            return vertIndx;
        }

    }
}
