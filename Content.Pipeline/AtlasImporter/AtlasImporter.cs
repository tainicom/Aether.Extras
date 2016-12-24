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
using tainicom.Aether.Content.Pipeline.Atlas;

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
            var renderorder = GetAttribute(map, "renderorder");
            var mapColumns = GetAttributeAsInt(map, "width").Value;
            var mapRows = GetAttributeAsInt(map, "height").Value;
            var tileWidth = GetAttributeAsInt(map, "tilewidth").Value;
            var tileHeight = GetAttributeAsInt(map, "tileheight").Value;
            output.Width = mapColumns * tileWidth;
            output.Height = mapRows * tileHeight;
            
            XmlNode tileset = map["tileset"];
            if (tileset.Attributes["source"] != null)
                throw new InvalidContentException("External Tileset is not supported.");
            if (tileset["tileoffset"] != null)
                throw new InvalidContentException("tileoffset is not supported.");
            var firstgid = GetAttributeAsInt(tileset, "firstgid").Value;

            Dictionary<int, SourceContent> images = new Dictionary<int, SourceContent>();
            TextureImporter txImporter = new TextureImporter();
            
            foreach (XmlNode tileNode in tileset.ChildNodes)
            {
                if (tileNode.Name != "tile") continue;
                var tileId = GetAttributeAsInt(tileNode, "id").Value;
                XmlNode imageNode = tileNode["image"];
                
                var source = new SourceContent();

                //var format = GetAttribute(imageNode, "format");
                var imageSource = GetAttribute(imageNode, "source");
                var fullImageSource = Path.Combine(Path.GetDirectoryName(filename), imageSource);
                var textureContent = (Texture2DContent)txImporter.Import(fullImageSource, context);
                textureContent.Name = Path.GetFileNameWithoutExtension(fullImageSource);

                source.Texture = textureContent;

                var width = GetAttributeAsInt(imageNode, "width");
                var height = GetAttributeAsInt(imageNode, "height");

                source.Width = width ?? textureContent.Mipmaps[0].Width;
                source.Height = height ?? textureContent.Mipmaps[0].Height;

                var transKeyColor = GetAttributeAsColor(imageNode, "trans");
                if (transKeyColor != null)
                    foreach (var mips in textureContent.Faces)
                        foreach (var mip in mips)
                            ((PixelBitmapContent<Color>)mip).ReplaceColor(transKeyColor.Value, Color.Transparent);

                images.Add(firstgid + tileId, source);
            }

            output.Textures.AddRange(images.Values);
            
            XmlNode layerNode = map["layer"];
            var layerColumns = Convert.ToInt32(map.Attributes["width"].Value, CultureInfo.InvariantCulture);
            var layerRows = Convert.ToInt32(map.Attributes["height"].Value, CultureInfo.InvariantCulture);

            XmlNode layerDataNode = layerNode["data"];
            var encoding = layerDataNode.Attributes["encoding"].Value;
            if (encoding!="csv")
                throw new InvalidContentException("Invalid encoding. Only 'csv' is supported for data.");
            var data = layerDataNode.InnerText;
            var dataList = data.Split(',');
            for (int i = 0; i < dataList.Length; i++)
                dataList[i] = dataList[i].Trim();
            
            for(int y=0;y<layerRows;y++)
            {
                for(int x=0;x<layerColumns;x++ )
                {
                    var posX = x * tileWidth;
                    var posY = y * tileHeight;
                    
                    var tilegId = Convert.ToInt32(dataList[y * layerColumns + x], CultureInfo.InvariantCulture);
                    SourceContent image; 
                    if (!images.TryGetValue(tilegId, out image))
                        continue;

                    if (renderorder == "right-down" || renderorder == "right-up")
                        posX += (tileWidth - image.Width);
                    if (renderorder == "right-down" || renderorder == "left-down")
                        posY += (tileHeight - image.Height);

                    var sprite = new SpriteContent();
                    sprite.Name = image.Texture.Name;
                    sprite.Texture = image.Texture;
                    sprite.DestinationRectangle = new Rectangle(posX, posY, image.Width, image.Height);

                    output.Sprites.Add(sprite);
                }
            }
            
            return output;
        }

        private static void RenderAtlas(TextureAtlasContent output)
        {
            var outputBmp = new PixelBitmapContent<Color>(output.Width, output.Height);
            foreach (var sprite in output.Sprites)
            {
                var srcBmp = sprite.Texture.Faces[0][0];
                var srcRect = new Rectangle(0, 0, srcBmp.Width, srcBmp.Height);
                BitmapContent.Copy(srcBmp, srcRect, outputBmp, sprite.DestinationRectangle);
            }
            var mipmapChain = new MipmapChain(outputBmp);
            output.Mipmaps = mipmapChain;
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
