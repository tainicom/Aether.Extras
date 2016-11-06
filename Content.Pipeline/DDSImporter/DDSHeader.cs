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

namespace tainicom.Aether.Content.Pipeline
{
    internal struct DDSHeader
    {   
        static readonly int DDSignature = (BitConverter.IsLittleEndian) ? 0x20534444:0x44445320;

        public Int32 Signature;
        public Int32 Size;
        public DDS_Flags Flags;
        public Int32 Height;
        public Int32 Width;
        public Int32 PitchOrLinearSize;
        public Int32 Depth;
        public Int32 MipMapCount;
        public Int32 Reserved0, Reserved1, Reserved2, Reserved3;
        public Int32 Reserved4, Reserved5, Reserved6, Reserved7;
        public Int32 Reserved8, Reserved9, ReservedA;
        public DDSPixelFormat PixelFormat;
        public DDS_Caps Caps;
        public DDS_Caps2 Caps2;
        public Int32 Caps3;
        public Int32 Caps4;
        public Int32 ReservedB;

        public DDSHeader(System.IO.BinaryReader reader)
        {
            Signature = reader.ReadInt32();
            if (Signature != DDSHeader.DDSignature)
                throw new Exception("This does not appear to be a DDS file");
            Size = reader.ReadInt32();
            Flags = (DDS_Flags)reader.ReadInt32();
            Height = reader.ReadInt32();
            Width = reader.ReadInt32();
            PitchOrLinearSize = reader.ReadInt32();
            Depth = reader.ReadInt32();
            MipMapCount = reader.ReadInt32();
            Reserved0 = reader.ReadInt32();
            Reserved1 = reader.ReadInt32();
            Reserved2 = reader.ReadInt32();
            Reserved3 = reader.ReadInt32();
            Reserved4 = reader.ReadInt32();
            Reserved5 = reader.ReadInt32();
            Reserved6 = reader.ReadInt32();
            Reserved7 = reader.ReadInt32();
            Reserved8 = reader.ReadInt32();
            Reserved9 = reader.ReadInt32();
            ReservedA = reader.ReadInt32();
            PixelFormat = new DDSPixelFormat(reader);
            Caps = (DDS_Caps)reader.ReadInt32();
            Caps2 = (DDS_Caps2)reader.ReadInt32();
            Caps3 = reader.ReadInt32();
            Caps4 = reader.ReadInt32();
            ReservedB = reader.ReadInt32();
        }
    }

    [Flags]
    internal enum DDS_Flags: int
    {
        CAPS       = 0x000001,
        HEIGHT     = 0x000002,
        WIDTH      = 0x000004,
        PITCH      = 0x000008,
        PIXELFORMAT= 0x001000,
        MIPMAPCOUNT= 0x020000,
        LINEARSIZE = 0x080000,
        DEPTH      = 0x800000,
    }

    [Flags]
    internal enum DDS_Caps : int
    {
        COMPLEX = 0x000008,
        MIPMAP  = 0x400000,
        TEXTURE = 0x001000,
    }

    [Flags]
    internal enum DDS_Caps2 : int
    {
        CUBEMAP            = 0x000200,
        CUBEMAP_POSITIVEX  = 0x000400,
        CUBEMAP_NEGATIVEX  = 0x000800,
        CUBEMAP_POSITIVEY  = 0x001000,
        CUBEMAP_NEGATIVEY  = 0x002000,
        CUBEMAP_POSITIVEZ  = 0x004000,
        CUBEMAP_NEGATIVEZ  = 0x008000,
        VOLUME             = 0x200000,
    }
    
}
