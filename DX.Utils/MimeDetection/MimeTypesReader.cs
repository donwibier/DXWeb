using DX.Utils.MimeDetection;
using System;
using System.Collections.Generic;
using System.IO;
using System.Xml;


namespace DX.Utils.MimDetection
{
	/// <summary>
	/// Summary description for MimeTypesReader.
	/// </summary>
	public sealed class MimeTypesReader
	{
		/// <summary>
		/// 
		/// </summary>
		public MimeTypesReader()
		{
		}

        internal MimeType[] Read(string filepath)
        {
            MimeType[] types = null;
            XmlDocument document = new XmlDocument();
            document.Load(filepath);

            types = Visit(document);
            return types;

        }

        internal MimeType[] Read(Stream inStream)
        {
            MimeType[] types = null!;
            XmlDocument document = new XmlDocument();
            document.Load(inStream);

            types = Visit(document);
            return types;
        }

		/// <summary>Scan through the document. </summary>
        private MimeType[] Visit(XmlDocument document)
        {
            MimeType[] types = null;
            XmlElement element = document.DocumentElement ?? null!;
            if ((element != null) && element.Name.Equals("mime-types"))
            {
                types = ReadMimeTypes(element);
            }
            return (types == null) ? (new MimeType[0]) : types;
        }

        private MimeType[] ReadMimeTypes(XmlElement element)
        {
            List<MimeType> types = new List<MimeType>();
            XmlNodeList nodes = element.ChildNodes;
            for (int i = 0; i < nodes.Count; i++)
            {
                XmlNode node = nodes.Item(i);
                if (Convert.ToInt16(node.NodeType) == (short)XmlNodeType.Element)
                {
                    XmlElement nodeElement = (XmlElement)node;
                    if (nodeElement.Name.Equals("mime-type"))
                    {
                        MimeType type = ReadMimeType(nodeElement);
                        if (type != null)
                            types.Add(type);
                    }
                }
            }
            return types.ToArray();// (MimeType[])SupportUtil.ToArray(types, new MimeType[types.Count]);
        }

		/// <summary>Read Element named mime-type. </summary>
        private MimeType ReadMimeType(XmlElement element)
        {
            string name = null!;
            string description = null!;
            MimeType type = null!;
            XmlNamedNodeMap attrs = element.Attributes;
            for (int i = 0; i < attrs.Count; i++)
            {
                XmlAttribute attr = (attrs.Item(i) as XmlAttribute)!;
                if (attr.Name.Equals("name"))
                    name = attr.Value;
                else if (attr.Name.Equals("description"))
                    description = attr.Value;
            }
            if ((name == null) || (name.Trim().Equals("")))
                return null!;

            try
            {                
                type = new MimeType(name);
            }
            catch /*(MimeTypeException mte)*/
            {
                // Mime Type not valid... just ignore it
                //Utils.LogException(mte);
                return null!;
            }

            type.Description = description;

            XmlNodeList nodes = element.ChildNodes;
            for (int i = 0; i < nodes.Count; i++)
            {
                XmlNode node = nodes.Item(i)!;
                if (Convert.ToInt16(node.NodeType) == (short)XmlNodeType.Element)
                {
                    XmlElement nodeElement = (XmlElement)node;
                    if (nodeElement.Name.Equals("ext"))
                    {
                        ReadExt(nodeElement, type);
                    }
                    else if (nodeElement.Name.Equals("magic"))
                    {
                        ReadMagic(nodeElement, type);
                    }
                }
            }
            return type;
        }

		/// <summary>Read Element named ext. </summary>
        private void ReadExt(XmlElement element, MimeType type)
        {
            XmlNodeList nodes = element.ChildNodes;
            for (int i = 0; i < nodes.Count; i++)
            {
                XmlNode node = nodes.Item(i)!;
                if (Convert.ToInt16(node.NodeType) == (short)XmlNodeType.Text)
                {
                    type.AddExtension(((XmlText)node).Data);
                }
            }
        }

		/// <summary>Read Element named magic. </summary>
        private void ReadMagic(XmlElement element, MimeType mimeType)
        {
            string offset = null!;
            string content = null!;
            string type = null!;
            XmlNamedNodeMap attrs = element.Attributes;
            for (int i = 0; i < attrs.Count; i++)
            {
                XmlAttribute attr = (attrs.Item(i) as XmlAttribute)!;
                if (attr.Name.Equals("offset"))
                {
                    offset = attr.Value;
                }
                else if (attr.Name.Equals("type"))
                {
                    type = attr.Value;
                    if (String.Compare(type, "byte", true) == 0)
                    {
                        type = "System.Byte";
                    }
                }
                else if (attr.Name.Equals("value"))
                {
                    content = attr.Value;
                }
            }
            if ((offset != null) && (content != null))
            {
                mimeType.AddMagic(Int32.Parse(offset), type, content);
            }
        }
	}
}