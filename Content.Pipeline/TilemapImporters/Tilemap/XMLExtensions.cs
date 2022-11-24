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

using System.Globalization;
using Microsoft.Xna.Framework;

namespace System.Xml
{
    public static class XMLExtensions
    {

        public static string GetAttribute(this XmlNode xmlNode, string attributeName)
        {
            var attribute = xmlNode.Attributes[attributeName];
            if (attribute == null) return null;
            return attribute.Value;
        }

        public static int? GetAttributeAsInt(this XmlNode xmlNode, string attributeName)
        {
            var attribute = xmlNode.Attributes[attributeName];
            if (attribute == null) return null;
            return Int32.Parse(attribute.Value, CultureInfo.InvariantCulture);
        }

        public static Color? GetAttributeAsColor(this XmlNode xmlNode, string attributeName)
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
