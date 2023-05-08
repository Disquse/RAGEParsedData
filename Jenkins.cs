using System.Globalization;

namespace RAGEParsedData
{
    public static class Jenkins
    {
        public static Dictionary<uint, string> Strings { get; set; } = new();

        private static readonly List<string> FieldStrings = new()
        {
            "rage__sysParsedDataFile__Data",
            "attributes",
            "attributeValueStringStore",
            "attributeValueVecStore",
            "dataNodes",
            "valueHash",
            "contentIndex",
            "contentType",
            "isEnabled",
            "childIndex",
            "siblingIndex",
            "previousSiblingIndex",
            "attributeStart",
            "attributeCount",
        };

        private static readonly List<string> DefaultStrings = new()
        {
            "CONTENT_TYPE_STRING",
            "CONTENT_TYPE_VECTOR",
            "CONTENT_TYPE_FLOAT",
            "CONTENT_TYPE_INT",
            "CONTENT_TYPE_BOOL",
        };

        public static uint GenHash(string text)
        {
            uint h = 0;            

            foreach (var t in text)
            {
                h += (byte)t;
                h += h << 10;
                h ^= h >> 6;
            }

            h += h << 3;
            h ^= h >> 11;
            h += h << 15;

            return h;
        }

        public static string FindString(uint hash)
        {
            return Strings.TryGetValue(hash, out var result) ? result : $"UNK_{hash:X}";
        }

        public static string FindString(string str)
        {
            if (!str.StartsWith("0x"))
            {
                return str;
            }

            var hash = uint.Parse(str.Replace("0x", ""), NumberStyles.HexNumber);

            return FindString(hash);
        }

        public static bool LoadStrings(string path)
        {
            Strings.Clear();

            if (!File.Exists(path))
            {
                return false;
            }

            foreach (var rawLine in File.ReadAllLines(path))
            {
                var cleanLine = rawLine.Trim();

                if (cleanLine.StartsWith("//") || cleanLine.Length == 0)
                {
                    continue;
                }

                AddStringCaseInsensitive(cleanLine);
            }

            return true;
        }

        public static string ReplaceHashFields(string content)
        {
            // HACK: In case if files were extracted from OpenIV, replace hashed fields
            foreach (var entry in FieldStrings)
            {
                var hash = GenHash(entry);
                var prefix = entry.StartsWith("rage__") ? "UNK_TYPE" : "UNK_MEMBER";
                var search = $"{prefix}_0x{hash:X08}";

                content = content.Replace(search, entry);
            }

            return content;
        }

        public static void AddDefaultStrings() => DefaultStrings.ForEach(AddStringCaseInsensitive);

        private static void AddStringCaseInsensitive(string str)
        {
            var lowerCase = GenHash(str.ToLower());
            var anyCase = GenHash(str);

            if (!Strings.ContainsKey(lowerCase)) Strings.Add(lowerCase, str.ToLower());
            if (!Strings.ContainsKey(anyCase)) Strings.Add(anyCase, str);
        }
    }
}
