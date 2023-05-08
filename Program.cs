namespace RAGEParsedData
{
    internal enum EConvertCommand
    {
        Invalid = 0,
        Parse = 1,
        Convert = 2,
    }

    internal class Program
    {
        private const string StringsFile = "strings.txt";

        static void PrintHelp()
        {
            Console.WriteLine("Passed invalid arguments!\n");
            Console.WriteLine("Convert RAGE parsed data to readable XML:");
            Console.WriteLine("  RAGEParsedData.exe convert \"C:\\parseddata\" \"C:\\converted\"");
            Console.WriteLine("Convert readable XML to RAGE parsed data:");
            Console.WriteLine("  RAGEParsedData.exe parse \"C:\\converted\" \"C:\\parseddata\"");
            Console.WriteLine("\nPress any key to exit");
            Console.ReadKey();
        }

        static void Main(string[] arguments)
        {
            if (arguments.Length < 3)
            {
                PrintHelp();
                return;
            }

            var command = arguments[0] switch
            {
                "parse" => EConvertCommand.Parse,
                "convert" => EConvertCommand.Convert,
                _ => EConvertCommand.Invalid
            };

            if (command == EConvertCommand.Invalid)
            {
                PrintHelp();
                return;
            }

            var input = arguments[1];
            var output = arguments[2];

            if (!Directory.Exists(input))
            {
                Console.WriteLine($"Input directory \"{input}\" doesn't exist");
                return;
            }

            var files = Directory.GetFiles(input, "*.xml");

            if (files.Length == 0)
            {
                Console.WriteLine($"Input directory \"{input}\" has no XML files");
                return;
            }

            if (!Directory.Exists(output))
            {
                var status = Directory.CreateDirectory(output);

                if (!status.Exists)
                {
                    Console.WriteLine($"Failed to create output directory \"{output}\"");
                    return;
                }
            }

            if (File.Exists(StringsFile))
            {
                Console.WriteLine($"Loading strings from \"{StringsFile}\"");
                Jenkins.LoadStrings(StringsFile);
            }
            else
            {
                Console.WriteLine($"Strings file not found, put strings in \"{StringsFile}\"");
            }

            Jenkins.AddDefaultStrings();

            foreach (var path in files)
            {
                var fileName = Path.GetFileName(path);
                Console.WriteLine($"Processing {fileName}");

                var content = command switch
                {
                    EConvertCommand.Convert => Converter.ConvertToXml(path),
                    EConvertCommand.Parse => Converter.ConvertFromXml(path),
                    _ => throw new ArgumentOutOfRangeException(),
                };

                if (content == null) continue;

                var outPath = Path.Join(output, fileName);
                content.Save(outPath);
            }
        }
    }
}
