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
using Microsoft.Xna.Framework.Content.Pipeline;

namespace tainicom.Aether.Content.Pipeline
{
    struct VoxHeader
    {
        static readonly int VoxSignature = (BitConverter.IsLittleEndian) ? 0x20584F56 : 0x564F5820;

        public readonly Int32 Signature;
        public readonly Int32 Version;

        public VoxHeader(System.IO.BinaryReader reader)
        {
            Signature = reader.ReadInt32();
            if (Signature != VoxHeader.VoxSignature)
                throw new InvalidContentException("This does not appear to be a VOX file");
            Version = reader.ReadInt32();
            if (Version != 150)
                throw new InvalidContentException("Unknown version");
        }
    }

    struct Chunk
    {
        public readonly int ChunkId;
        public readonly int NumBytesOfChunkContent;
        public readonly int NumBytesOfChildrenChunks;

        public Chunk(System.IO.BinaryReader reader)
        {
            ChunkId = reader.ReadInt32();
            NumBytesOfChunkContent = reader.ReadInt32();
            NumBytesOfChildrenChunks = reader.ReadInt32();
        }

        public const int MAIN = 0x4E49414D;
        public const int PACK = 0x4B434150;
        public const int SIZE = 0x455A4953;
        public const int XYZI = 0x495A5958;
        public const int RGBA = 0x41424752;
        public const int MATT = 0x5454414D;
    }

    struct XYZI
    {
        public Point3 Point;
        public byte ColorIndex;
        public Sides SharedSides;
    }

}


