using System.Xml;
using System.Xml.Linq;

namespace RAGEParsedData
{
    public static class Converter
    {
        public static XmlDocument ReadDocument(string path)
        {
            var text = File.ReadAllText(path);
            var content = Jenkins.ReplaceHashFields(text);

            XmlDocument document = new();
            document.LoadXml(content);

            return document;
        }

        public static XElement? ConvertFromXml(string path)
        {
            var xmlDocument = ReadDocument(path);
            XmlNode? documentRoot = xmlDocument.DocumentElement;
            if (documentRoot == null) return null;

            DataFile dataFile = new();
            dataFile.LoadConverted(documentRoot);

            XElement attributesElement = new("attributes");
            XElement attributeValueStringStoreElement = new("attributeValueStringStore");
            XElement attributeValueVecStoreElement = new("attributeValueVecStore");
            XElement dataNodesElement = new("dataNodes");

            foreach (var entry in dataFile.Vectors)
            {
                attributeValueVecStoreElement.Add(
                    new XElement("Item",
                        new XElement("x", new XAttribute("value", BitConverter.SingleToInt32Bits(entry.ValueX))),
                        new XElement("y", new XAttribute("value", BitConverter.SingleToInt32Bits(entry.ValueY))),
                        new XElement("z", new XAttribute("value", BitConverter.SingleToInt32Bits(entry.ValueZ)))
                    )
                );
            }

            foreach (var entry in dataFile.Strings)
            {
                attributeValueStringStoreElement.Add(new XElement("Item", new XText(entry.Value)));
            }

            foreach (var entry in dataFile.Attributes)
            {
                attributesElement.Add(
                    new XElement("Item",
                        new XElement("name", new XText(entry.Name.StartsWith("UNK_") ? entry.Name.Replace("UNK_", "0x") : entry.Name)),
                        new XElement("valueHash", new XText(entry.ValueHash)),
                        new XElement("contentIndex", new XAttribute("value", entry.ContentIndex)),
                        new XElement("contentType", new XText(entry.ContentType.ToString())),
                        new XElement("isEnabled", new XAttribute("value", entry.IsEnabled ? "true" : "false"))
                    )
                );
            }

            foreach (var entry in dataFile.DataNodes)
            {
                dataNodesElement.Add(
                    new XElement("Item",
                        new XElement("name", new XText(entry.Name.StartsWith("UNK_") ? entry.Name.Replace("UNK_", "0x") : entry.Name)),
                        new XElement("parentIndex", new XAttribute("value", entry.ParentIndex)),
                        new XElement("childIndex", new XAttribute("value", entry.ChildIndex)),
                        new XElement("siblingIndex", new XAttribute("value", entry.SiblingIndex)),
                        new XElement("previousSiblingIndex", new XAttribute("value", entry.PreviousSiblingIndex)),
                        new XElement("attributeStart", new XAttribute("value", entry.AttributeStart)),
                        new XElement("attributeCount", new XAttribute("value", entry.AttributeCount)),
                        new XElement("isEnabled", new XAttribute("value", entry.IsEnabled ? "true" : "false"))
                    )
                );
            }

            XElement rootElement = new("rage__sysParsedDataFile__Data");
            rootElement.Add(attributesElement);
            rootElement.Add(attributeValueStringStoreElement);
            rootElement.Add(attributeValueVecStoreElement);
            rootElement.Add(dataNodesElement);

            return rootElement;
        }

        public static XElement? ConvertToXml(string path)
        {
            var xmlDocument = ReadDocument(path);
            XmlNode? documentRoot = xmlDocument.DocumentElement;
            if (documentRoot == null) return null;

            DataFile dataFile = new();
            dataFile.LoadParsed(documentRoot);

            static XElement ProcessNode(XElement? parent, DataNode dataNode)
            {
                var element = new XElement(dataNode.GetName());

                foreach (var attribute in dataNode.Attributes)
                {
                    var attributeName = attribute.GetName();

                    switch (attribute.ContentType)
                    {
                        case EContentType.CONTENT_TYPE_STRING:
                            element.Add(new XAttribute(attributeName, attribute.ValueString?.ToString() ?? "invalid"));
                            break;
                        case EContentType.CONTENT_TYPE_VECTOR:
                            element.Add(new XAttribute(attributeName, attribute.ValueVector?.ToString() ?? "invalid"));
                            break;
                        case EContentType.CONTENT_TYPE_FLOAT:
                            element.Add(new XAttribute(attributeName, $"{attribute.ValueFloat ?? 0.0f:0.0#######}"));
                            break;
                        case EContentType.CONTENT_TYPE_INT:
                            element.Add(new XAttribute(attributeName, attribute.ValueInt ?? 0));
                            break;
                        case EContentType.CONTENT_TYPE_BOOL:
                            element.Add(new XAttribute(attributeName, attribute.ValueBool ?? false));
                            break;
                        default:
                            throw new Exception($"Failed to parse type of {attributeName}");
                    }
                }

                parent?.Add(element);

                if (dataNode.Childs.Count > 0)
                {
                    foreach (var child in dataNode.Childs)
                    {
                        ProcessNode(element, child);
                    }
                }

                return element;
            }

            return ProcessNode(null, dataFile.DataNodes[0]);
        }
    }
}
