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
using System.Runtime.InteropServices;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics.PackedVector;

namespace tainicom.Aether.Content.Pipeline
{
     
    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    internal struct DXT1Block
    {
        Bgr565 color0;
        Bgr565 color1;
        byte idx0123;
        byte idx4567;
        byte idx89AB;
        byte idxCDEF;
        
        internal Color GetColor(int colorIndex)
        {
            var H = colorIndex / 4;
            var L = colorIndex % 4;
            int idx = 0;

            switch (H)
            {
                case 0:
                    idx = idx0123;
                    break;
                case 1:
                    idx = idx4567;
                    break;
                case 2:
                    idx = idx89AB;
                    break;
                case 3:
                    idx = idxCDEF;
                    break;
            }

            idx = (idx >> (L * 2));
            idx = idx & 0x03;

            switch (idx)
            {
                case 0: return new Color(color0.ToVector3());
                case 1: return new Color(color1.ToVector3());
                    
                //case 2: return new Color((2 * color0.ToVector3() + color1.ToVector3()) / 3);
                //case 3: return new Color((color0.ToVector3() + 2 * color1.ToVector3()) / 3);

                case 2: return (color0.PackedValue > color1.PackedValue) ? 
                        new Color((2 * color0.ToVector3() + color1.ToVector3()) / 3) : 
                        new Color((color0.ToVector3() + color1.ToVector3()) / 2f);
                case 3: return (color0.PackedValue > color1.PackedValue) ? 
                        new Color((2 * color1.ToVector3() + color0.ToVector3()) / 3) : 
                        Color.Transparent;

                default: throw new NotSupportedException();
            }


        }
    }
}
