#region License
//   Copyright 2019 Kastellanos Nikolaos
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
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Xml;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content.Pipeline;
using Microsoft.Xna.Framework.Content.Pipeline.Graphics;
using Microsoft.Xna.Framework.Graphics;

namespace tainicom.Aether.Content.Pipeline
{
    [ContentImporter(".slmc", DisplayName = "SLMC Importer - Aether", DefaultProcessor = "SLMCProcessor")]
    public class SLMCImporter : ContentImporter<TextureContent>
    {
        public override TextureContent Import(string filename, ContentImporterContext context)
        {
            Texture2DContent output;

            if (Path.GetExtension(filename) != ".slmc")
                throw new InvalidContentException("File type not supported.");

            var images = ImportSLMC(filename, context);

            if (images.Count < 1)
                throw new InvalidContentException("Element 'channels' must have at least one 'image'.");
            if (images.Count > 4)
                throw new InvalidContentException("No more than 4 images are supported.");

            int width = images[0].Mipmaps[0].Width;
            int height = images[0].Mipmaps[0].Height;
            // validate size
            foreach (var image in images)
            {
                if (image.Mipmaps[0].Width != width|| image.Mipmaps[0].Height != height)
                    throw new InvalidContentException("Images must be of the same size.");
            }

            var pixelCount = width * height;
            var byteCount = pixelCount * 4;
            byte[] data = new byte[byteCount];

            for (int i = 0; i < images.Count; i++)
            {
                var image = images[i];
                var face = image.Faces[0][0];
                var pixelData = face.GetPixelData();

                for (int d = 0; d < pixelCount; d++)
                {
                    data[d * 4 + i] = pixelData[d * 4];
                }
            }

            var bitmap = new PixelBitmapContent<Color>(width, height);
            bitmap.SetPixelData(data);

            output = new Texture2DContent();
            output.Faces[0].Add(bitmap);
            
            return output;
        }

        private IList<Texture2DContent> ImportSLMC(string filename, ContentImporterContext context)
        {
            XmlDocument xmlDoc = new XmlDocument();
            xmlDoc.Load(filename);
            
            var channels = xmlDoc.DocumentElement;

            if (channels.Name != "channels")
                throw new InvalidContentException(String.Format("Root element must be 'channels'."));

            TextureImporter txImporter = new TextureImporter();
            List<Texture2DContent> images = new List<Texture2DContent>();
            
            foreach (XmlNode imageNode in channels.ChildNodes)
            {
                if (imageNode.Name != "image")
                    throw new InvalidContentException(String.Format("Element '{0}' not supported in 'channels'.", imageNode.Name));

                var imageSource = GetAttribute(imageNode, "source");
                var fullImageSource = Path.Combine(Path.GetDirectoryName(filename), imageSource);
                var textureContent = (Texture2DContent)txImporter.Import(fullImageSource, context);
                textureContent.Name = Path.GetFileNameWithoutExtension(fullImageSource);

                images.Add(textureContent);
            }

            return images;
        }
        
        private static string GetAttribute(XmlNode xmlNode, string attributeName)
        {
            var attribute = xmlNode.Attributes[attributeName];
            if (attribute == null) return null;
            return attribute.Value;
        }

        internal static int GetBitmapSize(SurfaceFormat format, int width, int height)
        {
            int pixels = width * height;

            switch (format)
            {
                case SurfaceFormat.Dxt1:
                    return Math.Max(8, (pixels / 2));
                default:
                    throw new NotImplementedException();
            }
        }        
    }

}
