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

using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Xml;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content.Pipeline;
using Microsoft.Xna.Framework.Content.Pipeline.Graphics;
using Microsoft.Xna.Framework.Graphics;

namespace tainicom.Aether.Content.Pipeline
{
    [ContentImporter(".tmx", DisplayName = "Atlas Importer - Aether", DefaultProcessor = "AtlasProcessor")]
    public class AtlasImporter : ContentImporter<TextureAtlasContent>
    {
        public override TextureAtlasContent Import(string filename, ContentImporterContext context)
        {
            TextureAtlasContent output;
            
            if (Path.GetExtension(filename) == ".tmx")
                output = ImportTMX(filename, context);
            else
                throw new InvalidContentException("File type not supported");

            PackSprites(output);
            RenderAtlas(output);
            
            return output;
        }
        
        private static TextureAtlasContent ImportTMX(string filename, ContentImporterContext context)
        {
            TextureAtlasContent output = new TextureAtlasContent();
            output.Identity = new ContentIdentity(filename);

            XmlDocument xmlDoc = new XmlDocument();
            xmlDoc.Load(filename);

            var map = xmlDoc.DocumentElement;
            var orientation = GetAttribute(map, "orientation");
            if (orientation != "orthogonal")
                throw new InvalidContentException("Invalid orientation. Only 'orthogonal' is supported for atlases.");
            output.Renderorder = GetAttribute(map, "renderorder");
            output.MapColumns = GetAttributeAsInt(map, "width").Value;
            output.MapRows = GetAttributeAsInt(map, "height").Value;
            output.TileWidth = GetAttributeAsInt(map, "tilewidth").Value;
            output.TileHeight = GetAttributeAsInt(map, "tileheight").Value;
            output.Width = output.MapColumns * output.TileWidth;
            output.Height = output.MapRows * output.TileHeight;

            XmlNode tileset = map["tileset"];
            output.Firstgid = GetAttributeAsInt(tileset, "firstgid").Value;

            if (tileset.Attributes["source"] != null)
            {
                var tsxFilename = tileset.Attributes["source"].Value;
                var baseDirectory = Path.GetDirectoryName(filename);
                tsxFilename = Path.Combine(baseDirectory, tsxFilename);
                var sourceSprites = ImportTSX(tsxFilename, context);
                output.SourceSprites.AddRange(sourceSprites);
                context.AddDependency(tsxFilename);
            }
            else
            {
                var rootDirectory = Path.GetDirectoryName(filename);
                var sourceSprites = ImportTileset(tileset, context, rootDirectory);
                output.SourceSprites.AddRange(sourceSprites);
            }

            XmlNode layerNode = map["layer"];
            var layerColumns = Convert.ToInt32(map.Attributes["width"].Value, CultureInfo.InvariantCulture);
            var layerRows = Convert.ToInt32(map.Attributes["height"].Value, CultureInfo.InvariantCulture);
            output.LayerColumns = layerColumns;
            output.LayerRows = layerRows;

            XmlNode layerDataNode = layerNode["data"];
            var encoding = layerDataNode.Attributes["encoding"].Value;
            if (encoding != "csv")
                throw new InvalidContentException("Invalid encoding. Only 'csv' is supported for data.");
            var data = layerDataNode.InnerText;
            var dataStringList = data.Split(',');
            var mapData = new int[dataStringList.Length];
            for (int i = 0; i < dataStringList.Length; i++)
                mapData[i] = Convert.ToInt32(dataStringList[i].Trim(), CultureInfo.InvariantCulture);
            output.MapData = mapData;

            return output;
        }

        private static List<SpriteContent> ImportTileset(XmlNode tileset, ContentImporterContext context, string baseDirectory)
        {
            List<SpriteContent> images = new List<SpriteContent>();

            if (tileset["tileoffset"] != null)
                throw new InvalidContentException("tileoffset is not supported.");

            foreach (XmlNode tileNode in tileset.ChildNodes)
            {
                if (tileNode.Name != "tile") continue;
                var tileId = GetAttributeAsInt(tileNode, "id").Value;
                if (tileId != images.Count)
                    throw new InvalidContentException("Invalid id");
                XmlNode imageNode = tileNode["image"];


                //var format = GetAttribute(imageNode, "format");
                var imageSource = GetAttribute(imageNode, "source");
                var fullImageSource = Path.Combine(baseDirectory, imageSource);
                TextureImporter txImporter = new TextureImporter();
                var textureContent = (Texture2DContent)txImporter.Import(fullImageSource, context);
                textureContent.Name = Path.GetFileNameWithoutExtension(fullImageSource);

                var source = new SpriteContent();
                source.Texture = textureContent;
                source.Bounds.Location = Point.Zero;
                source.Bounds.Width  = textureContent.Mipmaps[0].Width;
                source.Bounds.Height = textureContent.Mipmaps[0].Height;

                var transKeyColor = GetAttributeAsColor(imageNode, "trans");
                if (transKeyColor != null)
                    foreach (var mips in textureContent.Faces)
                        foreach (var mip in mips)
                            ((PixelBitmapContent<Color>)mip).ReplaceColor(transKeyColor.Value, Color.Transparent);

                images.Add(source);
            }

            return images;
        }

        private static void PackSprites(TextureAtlasContent output)
        {
            for (int y = 0; y < output.LayerRows; y++)
            {
                for (int x = 0; x < output.LayerColumns; x++)
                {
                    var posX = x * output.TileWidth;
                    var posY = y * output.TileHeight;

                    var tilegId = output.MapData[y * output.LayerColumns + x];
                    if (tilegId == 0) continue;
                    tilegId -= output.Firstgid;

                    SpriteContent srcSprite = output.SourceSprites[tilegId];

                    if (output.Renderorder == "right-down" || output.Renderorder == "right-up")
                        posX += (output.TileWidth - srcSprite.Bounds.Width);
                    if (output.Renderorder == "right-down" || output.Renderorder == "left-down")
                        posY += (output.TileHeight - srcSprite.Bounds.Height);

                    var newSprite = new SpriteContent(srcSprite);
                    newSprite.Bounds.Location = new Point(posX, posY);

                    output.DestinationSprites.Add(newSprite);
                    var name = srcSprite.Texture.Name;
                    output.Sprites.Add(name, newSprite);
                }
            }
        }

        private static List<SpriteContent> ImportTSX(string tsxFilename, ContentImporterContext context)
        {
            XmlDocument xmlDoc = new XmlDocument();
            xmlDoc.Load(tsxFilename);
            XmlNode tileset = xmlDoc.DocumentElement;
            var baseDirectory = Path.GetDirectoryName(tsxFilename);
            return ImportTileset(tileset, context, baseDirectory);
        }

        private static void RenderAtlas(TextureAtlasContent output)
        {
            var outputBmp = new PixelBitmapContent<Color>(output.Width, output.Height);
            foreach (var sprite in output.DestinationSprites)
            {
                var srcBmp = sprite.Texture.Faces[0][0];
                var srcRect = new Rectangle(0, 0, srcBmp.Width, srcBmp.Height);
                BitmapContent.Copy(srcBmp, srcRect, outputBmp, sprite.Bounds);
            }
            var mipmapChain = new MipmapChain(outputBmp);
            output.Texture.Mipmaps = mipmapChain;
        }
        
        private static string GetAttribute(XmlNode xmlNode, string attributeName)
        {
            var attribute = xmlNode.Attributes[attributeName];
            if(attribute==null) return null;
            return attribute.Value;
        }
        
        private static int? GetAttributeAsInt(XmlNode xmlNode, string attributeName)
        {
            var attribute = xmlNode.Attributes[attributeName];
            if (attribute == null) return null;
            return Int32.Parse(attribute.Value, CultureInfo.InvariantCulture);
        }

        private static Color? GetAttributeAsColor(XmlNode xmlNode, string attributeName)
        {
            var attribute = xmlNode.Attributes[attributeName];
            if (attribute == null) return null;
            attribute.Value = attribute.Value.TrimStart(new char[] { '#' });
            return new Color(
                Int32.Parse(attribute.Value.Substring(0, 2), System.Globalization.NumberStyles.HexNumber),
                Int32.Parse(attribute.Value.Substring(2, 2), System.Globalization.NumberStyles.HexNumber),
                Int32.Parse(attribute.Value.Substring(4, 2), System.Globalization.NumberStyles.HexNumber));
        }
    }
}
