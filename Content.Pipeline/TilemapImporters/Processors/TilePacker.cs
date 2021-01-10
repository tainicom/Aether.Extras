#region License
//   Copyright 2021 Kastellanos Nikolaos
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
using Microsoft.Xna.Framework;

namespace tainicom.Aether.Content.Pipeline
{
	internal static class TilePacker
	{
		internal static IList<TileContent> ArrangeGlyphs(IList<TileContent> sourceTiles,
            int tileWidth, int tileHeight,
            bool requirePOT, bool requireSquare)
		{
            // copy tiles to destTiles
            var destTiles = new List<TileContent>();
            for (int i = 0; i < sourceTiles.Count; i++)
            {
                var srcTile = sourceTiles[i];
                var dstTile = new TileContent(srcTile);
                destTiles.Add(dstTile);
            }
            
            for (int i = 0; i < destTiles.Count; i++)
			{
                var dstTile = destTiles[i];
                dstTile.DstBounds.Width = tileWidth;
                dstTile.DstBounds.Height = tileHeight;
            }
            
			// Sort so the largest glyphs get arranged first.
			destTiles.Sort(CompareTileSizes);

			// Work out how big the output bitmap should be.
			int outputWidth = EstimateOutputWidth(destTiles);
            outputWidth = MakeValidTextureSize(outputWidth, requirePOT);
            int outputHeight = 0;

			// Choose positions for each glyph, one at a time.
			for (int i = 0; i < destTiles.Count; i++)
			{
				PositionGlyph(destTiles, i, outputWidth);
                outputHeight = Math.Max(outputHeight, destTiles[i].DstBounds.Y + destTiles[i].DstBounds.Height);
            }

			// Create the merged output bitmap.
			outputHeight = MakeValidTextureSize(outputHeight, requirePOT);
			if (requireSquare)
            {
				outputHeight = Math.Max(outputWidth, outputHeight);
				outputWidth = outputHeight;
			}

            return destTiles;

        }
        
		static void PositionGlyph(List<TileContent> glyphs, int index, int outputWidth)
		{
			int x = 0;
			int y = 0;

			while (true)
			{
				// Is this position free for us to use?
				int intersects = FindIntersectingTile(glyphs, index, x, y);

				if (intersects < 0)
				{
                    glyphs[index].DstBounds.X = x;
                    glyphs[index].DstBounds.Y = y;
                    return;
				}

				// Skip past the existing glyph that we collided with.
                x = glyphs[intersects].DstBounds.X + glyphs[intersects].DstBounds.Width;

                // If we ran out of room to move to the right, try the next line down instead.
                //if (x + glyphs[index].SrcBounds.Width > outputWidth)
                if (x + glyphs[index].DstBounds.Width > outputWidth)
                {
					x = 0;
					y++;
				}
			}
		}
        
		// Checks if a proposed glyph position collides with anything that we already arranged.
		static int FindIntersectingTile(List<TileContent> glyphs, int index, int x, int y)
		{
            var bounds = glyphs[index].DstBounds;

			for (int i = 0; i < index; i++)
			{
                var other = glyphs[i].DstBounds;

                if (other.X >= x + bounds.Width)
					continue;
				if (other.X + other.Width <= x)
					continue;
				if (other.Y >= y + bounds.Height)
					continue;
				if (other.Y + other.Height <= y)
					continue;

				return i;
			}

			return -1;
		}

		static int CompareTileSizes(TileContent a, TileContent b)
		{
			var res = b.DstBounds.Height.CompareTo(a.DstBounds.Height);
            if (res == 0)
                res = b.DstBounds.Width.CompareTo(a.DstBounds.Width);
  		    return res;
		}

		static int EstimateOutputWidth(IList<TileContent> sourceGlyphs)
		{
			int maxWidth = 0;
			int totalSize = 0;

			foreach (var sourceGlyph in sourceGlyphs)
			{
				maxWidth = Math.Max(maxWidth, sourceGlyph.DstBounds.Width);
				totalSize += sourceGlyph.DstBounds.Width * sourceGlyph.DstBounds.Height;
			}

			int width = Math.Max((int)Math.Sqrt(totalSize), maxWidth);
            return width;
		}

		// Rounds a value up to the next larger valid texture size.
		static int MakeValidTextureSize(int value, bool requirePowerOfTwo)
		{
			// In case we want to compress the texture, make sure the size is a multiple of 4.
			const int blockSize = 4;

			if (requirePowerOfTwo)
			{
				// Round up to a power of two.
				int powerOfTwo = blockSize;

				while (powerOfTwo < value)
					powerOfTwo <<= 1;

				return powerOfTwo;
			}
			else
			{
				// Round up to the specified block size.
				return (value + blockSize - 1) & ~(blockSize - 1);
			}
		}
	}

}

