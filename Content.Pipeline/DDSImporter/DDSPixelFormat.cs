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
    internal struct DDSPixelFormat
    {
        public Int32 Size;
        public PF_Flags Flags;
        public PF_FourCC FourCC;
        public Int32 RGBBitCount;
        public Int32 RBitMask;
        public Int32 GBitMask;
        public Int32 BBitMask;
        public Int32 ABitMask;

        public DDSPixelFormat(System.IO.BinaryReader reader)
        {
            Size = reader.ReadInt32();
            Flags = (PF_Flags)reader.ReadInt32();
            FourCC = (PF_FourCC)reader.ReadInt32();
            RGBBitCount = reader.ReadInt32();
            RBitMask = reader.ReadInt32();
            GBitMask = reader.ReadInt32();
            BBitMask = reader.ReadInt32();
            ABitMask = reader.ReadInt32();
        }

    }

    internal enum PF_Flags : int
    {
        ALPHAPIXELS = 0x00001,
        ALPHA = 0x00002,
        FOURCC = 0x00004,
        RGB = 0x00040,
        YUV = 0x00200,
        LUMINANCE = 0x20000,
    }

    internal enum PF_FourCC : int
    {
        DXT1 = 0x31545844,
        DXT2 = 0x32545844,
        DXT3 = 0x33545844,
        DXT4 = 0x34545844,
        DXT5 = 0x35545844,
        DX10 = 0x30315844,
    }

}
