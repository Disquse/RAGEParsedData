using System.Xml;
using System.Globalization;

namespace RAGEParsedData
{
    public enum EContentType // rage__eContentType
    {
        CONTENT_TYPE_STRING = 0,
        CONTENT_TYPE_VECTOR = 1,
        CONTENT_TYPE_FLOAT = 2,
        CONTENT_TYPE_INT = 3,
        CONTENT_TYPE_BOOL = 4,
    };

    public enum EFileType // not RAGE stuff
    {
        FILE_TYPE_PARSED,
        FILE_TYPE_CONVERTED,
    };

    public class Attribute : BaseData // rage__sysParsedDataFile__Attribute
    {
        public string Name { get; set; } = string.Empty;
        public string ValueHash { get; set; } = string.Empty;
        public uint ContentIndex { get; set; }
        public EContentType ContentType { get; set; }
        public bool IsEnabled { get; set; }

        public AttributeString? ValueString { get; set; }
        public AttributeVector? ValueVector { get; set; }
        public float? ValueFloat { get; set; }
        public int? ValueInt { get; set; }
        public bool? ValueBool { get; set; }

        public override void LoadParsed(XmlNode node)
        {
            Name = ReadInnerText(node, "name");
            ValueHash = ReadInnerText(node, "valueHash");
            ContentIndex = uint.Parse(ReadAttribute(node, "contentIndex"));
            IsEnabled = ReadAttribute(node, "isEnabled").ToLowerInvariant() == "true";

            var contentType = Jenkins.FindString(ReadInnerText(node, "contentType"));

            if (Enum.TryParse(typeof(EContentType), contentType, true, out var result))
            {
                ContentType = (EContentType?)result ?? throw new Exception("Failed to parse content type");
            }
            else throw new Exception("Failed to parse content type");
        }

        public override void LoadConverted(XmlNode node)
        {
            var innerText = node.InnerText;

            Name = node.Name;
            IsEnabled = true;

            if (innerText.StartsWith("[") && innerText.EndsWith("]") && innerText.Split(",").Length == 3)
            {
                ContentType = EContentType.CONTENT_TYPE_VECTOR;
                ValueVector = new AttributeVector();
                ValueVector.LoadConverted(node);

                ValueHash = "0xDEADC0DE"; // TODO
            }
            else if (innerText.ToLower() == "true" || innerText.ToLower() == "false")
            {
                ContentType = EContentType.CONTENT_TYPE_BOOL;
                ValueBool = innerText.ToLower() == "true";

                ValueHash = $"0x{Jenkins.GenHash(innerText.ToLower())}";
            }
            else if (innerText.Contains('.') && float.TryParse(innerText, NumberStyles.Float, null, out var valueFloat))
            {
                ContentType = EContentType.CONTENT_TYPE_FLOAT;
                ValueFloat = valueFloat;

                ValueHash = "0xBAADF00D"; // TODO
            }
            else if (int.TryParse(innerText, NumberStyles.Integer, null, out var valueInt))
            {
                ContentType = EContentType.CONTENT_TYPE_INT;
                ValueInt = valueInt;

                ValueHash = $"0x{Jenkins.GenHash(valueInt.ToString(CultureInfo.InvariantCulture))}";
            }
            else
            {
                ContentType = EContentType.CONTENT_TYPE_STRING;
                ValueString = new AttributeString
                {
                    Value = innerText
                };

                ValueHash = $"0x{Jenkins.GenHash(innerText.ToLower()):X}";
            }
        }

        public string GetName() => Jenkins.FindString(Name);
    }

    public class AttributeVector : BaseData // rage__sysParsedDataFile__AttribContentVec
    {
        public float ValueX { get; set; }
        public float ValueY { get; set; }
        public float ValueZ { get; set; }

        public override void LoadParsed(XmlNode node)
        {
            var intX = int.Parse(ReadAttribute(node, "x"));
            var intY = int.Parse(ReadAttribute(node, "y"));
            var intZ = int.Parse(ReadAttribute(node, "z"));

            ValueX = BitConverter.Int32BitsToSingle(intX);
            ValueY = BitConverter.Int32BitsToSingle(intY);
            ValueZ = BitConverter.Int32BitsToSingle(intZ);
        }

        public override void LoadConverted(XmlNode node)
        {
            var elements = node.InnerText[1..^1].Split(",");
            ValueX = float.Parse(elements[0].Trim());
            ValueY = float.Parse(elements[1].Trim());
            ValueZ = float.Parse(elements[2].Trim());
        }

        public override string ToString()
        {
            return $"[{ValueX:0.0#######}, {ValueY:0.0#######}, {ValueZ:0.0#######}]";
        }
    }

    public class AttributeString : BaseData // not RAGE stuff, just for code consistency
    {
        public string Value { get; set; } = string.Empty;

        public override void LoadParsed(XmlNode node)
        {
            Value = node.InnerText;
        }

        public override void LoadConverted(XmlNode node)
        {
            Value = node.InnerText;
        }

        public override string ToString()
        {
            return Value;
        }
    }

    public class DataNode : BaseData // rage__sysParsedDataFile__DataNode
    {
        public string Name { get; set; } = string.Empty;
        public ushort ParentIndex { get; set; }
        public ushort ChildIndex { get; set; }
        public ushort SiblingIndex { get; set; }
        public ushort PreviousSiblingIndex { get; set; }
        public ushort AttributeStart { get; set; }
        public byte AttributeCount { get; set; }
        public bool IsEnabled { get; set; }

        public int Index { get; set; }
        public DataNode? Parent { get; set; }
        public List<DataNode> Childs { get; set; } = new();
        public List<Attribute> Attributes { get; set; } = new();

        public override void LoadParsed(XmlNode node)
        {
            Name = node.SelectSingleNode("name")?.InnerText ?? string.Empty;
            ParentIndex = ushort.Parse(ReadAttribute(node, "parentIndex"));
            ChildIndex = ushort.Parse(ReadAttribute(node, "childIndex"));
            SiblingIndex = ushort.Parse(ReadAttribute(node, "siblingIndex"));
            PreviousSiblingIndex = ushort.Parse(ReadAttribute(node, "previousSiblingIndex"));
            AttributeStart = ushort.Parse(ReadAttribute(node, "attributeStart"));
            AttributeCount = byte.Parse(ReadAttribute(node, "attributeCount"));
            IsEnabled = ReadAttribute(node, "isEnabled") == "true";
        }

        public override void LoadConverted(XmlNode node)
        {
            ParentIndex = 0xFFFF;
            ChildIndex = 0xFFFF;
            SiblingIndex = 0xFFFF;
            PreviousSiblingIndex = 0xFFFF;
            AttributeStart = 0xFFFF;
            AttributeCount = 0;
            IsEnabled = true;

            // Not required...
        }

        public string GetName() => Jenkins.FindString(Name);

        public override string ToString()
        {
            return $"Node \"{Name}\": {Index} ({Childs.Count} children, parent = {Parent?.GetName() ?? "none"})";
        }
    }

    public class DataFile : BaseData // rage__sysParsedDataFile__Data
    {
        public List<Attribute> Attributes { get; set; } = new();
        public List<AttributeString> Strings { get; set; } = new();
        public List<AttributeVector> Vectors { get; set; } = new();
        public List<DataNode> DataNodes { get; set; } = new();

        public override void LoadParsed(XmlNode node)
        {
            BuildParsed(node);
        }

        public override void LoadConverted(XmlNode node)
        {
            BuildConvertedNode(node, new DataNode());

            foreach (var dataNode in DataNodes)
            {
                var nodeIndex = (ushort)DataNodes.IndexOf(dataNode);

                if (dataNode.Parent == DataNodes[0])
                {
                    // Some weird logic, no idea how it was meant to work originally...
                    dataNode.ParentIndex = 0xFFFF;
                }
                else
                {
                    var parentIndex = dataNode.Parent != null ? DataNodes.IndexOf(dataNode.Parent) : -1;
                    dataNode.ParentIndex = (ushort)((parentIndex == -1) ? 0xFFFF : parentIndex);
                }

                if (dataNode.Childs.Count > 0)
                {
                    for (var i = 0; i < dataNode.Childs.Count; i++)
                    {
                        var childNode = dataNode.Childs[i];

                        if (i == 0)
                        {
                            dataNode.ChildIndex = (ushort)DataNodes.IndexOf(childNode);
                        }

                        if (i < (dataNode.Childs.Count - 1))
                        {
                            childNode.SiblingIndex = (ushort)DataNodes.IndexOf(dataNode.Childs[i + 1]);
                        }

                        if (i > 0)
                        {
                            childNode.PreviousSiblingIndex = (ushort)DataNodes.IndexOf(dataNode.Childs[i - 1]);
                        }

                        childNode.ParentIndex = nodeIndex;
                    }
                }
                else
                {
                    dataNode.ChildIndex = 0xFFFF;
                }

                if (dataNode.Attributes.Count > 0)
                {
                    var attributeIndex = Attributes.IndexOf(dataNode.Attributes[0]);
                    dataNode.AttributeStart = (ushort)attributeIndex;
                    dataNode.AttributeCount = (byte)dataNode.Attributes.Count;
                }
                else
                {
                    dataNode.AttributeStart = (ushort)((dataNode.Parent == null) ? 0xFFFF : 0);
                    dataNode.AttributeCount = 0;
                }
            }
        }

        private void BuildConvertedNode(XmlNode node, DataNode parentData)
        {
            DataNodes.Add(parentData);

            parentData.LoadConverted(node);
            parentData.Name = node.Name;

            if (node.Attributes != null)
            {
                foreach (var attributeItem in node.Attributes)
                {
                    if (attributeItem is not XmlAttribute xmlAttribute) continue;

                    var attribute = new Attribute();
                    attribute.LoadConverted(xmlAttribute);

                    switch (attribute.ContentType)
                    {
                        case EContentType.CONTENT_TYPE_VECTOR:
                            if (attribute.ValueVector != null)
                            {
                                var attributeEntry = Attributes.Find(_ =>
                                {
                                    var left = _.ValueVector;
                                    if (left == null) return false;

                                    var right = attribute.ValueVector;
                                    return left.ValueX == right.ValueX && left.ValueY == right.ValueY && left.ValueZ == right.ValueZ;
                                });

                                if (attributeEntry?.ValueVector == null)
                                {
                                    Vectors.Add(attribute.ValueVector);
                                    attribute.ContentIndex = (uint)(Vectors.Count - 1);
                                }
                                else
                                {
                                    attribute.ValueVector = attributeEntry.ValueVector;
                                    attribute.ContentIndex = (uint)Vectors.IndexOf(attributeEntry.ValueVector);
                                }
                            }
                            break;
                        case EContentType.CONTENT_TYPE_STRING:
                            if (attribute.ValueString != null)
                            {
                                var attributeEntry = Attributes.Find(_ => _.ValueString?.ToString() == attribute.ValueString.ToString());
                                if (attributeEntry?.ValueString == null)
                                {
                                    Strings.Add(attribute.ValueString);
                                    attribute.ContentIndex = (uint)(Strings.Count - 1);
                                }
                                else
                                {
                                    attribute.ValueString = attributeEntry.ValueString;
                                    attribute.ContentIndex = (uint)Strings.IndexOf(attribute.ValueString);
                                }
                            }
                            break;
                        case EContentType.CONTENT_TYPE_BOOL:
                            attribute.ContentIndex = (uint)((attribute.ValueBool == true) ? 1 : 0);
                            break;
                        case EContentType.CONTENT_TYPE_INT:
                            attribute.ContentIndex = (uint)(attribute.ValueInt ?? 0);
                            break;
                        case EContentType.CONTENT_TYPE_FLOAT:
                            attribute.ContentIndex = BitConverter.SingleToUInt32Bits(attribute.ValueFloat ?? 0.0f);
                            break;
                        default:
                            throw new Exception("Invalid content type");
                    }

                    parentData.Attributes.Add(attribute);
                    Attributes.Add(attribute);
                }
            }

            foreach (var childNode in node.ChildNodes)
            {
                if (childNode is not XmlElement xmlElement) continue;

                var childData = new DataNode
                {
                    Parent = parentData
                };

                parentData.Childs.Add(childData);
                BuildConvertedNode(xmlElement, childData);
            }
        }

        private void BuildParsed(XmlNode rootNode)
        {
            ReadNodePath(rootNode, "attributes/Item", Attributes, EFileType.FILE_TYPE_PARSED);
            ReadNodePath(rootNode, "attributeValueStringStore/Item", Strings, EFileType.FILE_TYPE_PARSED);
            ReadNodePath(rootNode, "attributeValueVecStore/Item", Vectors, EFileType.FILE_TYPE_PARSED);
            ReadNodePath(rootNode, "dataNodes/Item", DataNodes, EFileType.FILE_TYPE_PARSED);

            foreach (var attribute in Attributes)
            {
                var contentIndex = (int)attribute.ContentIndex;
                if (contentIndex < 0) continue;

                switch (attribute.ContentType)
                {
                    case EContentType.CONTENT_TYPE_STRING:
                        attribute.ValueString = (Strings.Count > contentIndex) ? Strings[contentIndex] : null;
                        break;
                    case EContentType.CONTENT_TYPE_VECTOR:
                        attribute.ValueVector = (Vectors.Count > contentIndex) ? Vectors[contentIndex] : null;
                        break;
                    case EContentType.CONTENT_TYPE_FLOAT:
                        attribute.ValueFloat = BitConverter.Int32BitsToSingle(contentIndex);
                        break;
                    case EContentType.CONTENT_TYPE_INT:
                        attribute.ValueInt = contentIndex;
                        break;
                    case EContentType.CONTENT_TYPE_BOOL:
                        attribute.ValueBool = contentIndex != 0;
                        break;
                    default:
                        throw new Exception("Invalid content type");
                }
            }

            foreach (var node in DataNodes)
            {
                if (node.ParentIndex != 0xFFFF)
                {
                    var parent = DataNodes[node.ParentIndex];

                    if (node.Parent != null && node.Parent != parent)
                    {
                        throw new Exception("Duplicated parent child");
                    }

                    node.Parent = parent;

                    if (!node.Parent.Childs.Contains(node))
                    {
                        node.Parent.Childs.Add(node);
                    }
                }

                if (node.ChildIndex != 0xFFFF)
                {
                    var child = DataNodes[node.ChildIndex];

                    if (child.Parent != null && child.Parent != node)
                    {
                        throw new Exception("Duplicated child parent");
                    }

                    child.Parent = node;

                    if (!node.Childs.Contains(child))
                    {
                        node.Childs.Add(child);
                    }
                }

                if (node.AttributeStart != 0xFFFF && node.AttributeCount > 0)
                {
                    node.Attributes = Attributes.GetRange(node.AttributeStart, node.AttributeCount);
                }
            }

            foreach (var node in DataNodes)
            {
                node.Index = DataNodes.IndexOf(node);

                if (node.ChildIndex == 0xFFFF && node.Childs.Count > 0)
                {
                    throw new Exception("Duplicated child");
                }
            }
        }
    }

    public abstract class BaseData // mostly shared code
    {
        public virtual void LoadParsed(XmlNode node) => throw new NotImplementedException();

        public virtual void LoadConverted(XmlNode node) => throw new NotImplementedException();

        protected static string ReadAttribute(XmlNode node, string name, string attribute = "value")
        {
            var singleNode = node?.SelectSingleNode(name);
            return singleNode?.Attributes?[attribute]?.InnerText ?? string.Empty;
        }

        protected static string ReadInnerText(XmlNode node, string name)
        {
            var singleNode = node?.SelectSingleNode(name);
            return singleNode?.InnerText ?? string.Empty;
        }

        protected static void ReadNodePath<T>(XmlNode node, string path, List<T> container, EFileType fileType) where T : BaseData, new()
        {
            var selectedNodes = node.SelectNodes(path);
            if (selectedNodes == null) return;

            foreach (var selectedNode in selectedNodes)
            {
                if (selectedNode is not XmlNode xmlNode) continue;

                var attribute = new T();

                switch (fileType)
                {
                    case EFileType.FILE_TYPE_PARSED:
                        attribute.LoadParsed(xmlNode);
                        break;
                    case EFileType.FILE_TYPE_CONVERTED:
                        attribute.LoadConverted(xmlNode);
                        break;
                    default:
                        throw new Exception("Invalid file type");
                }

                container.Add(attribute);
            }
        }
    }
}
