using System;
using System.Linq;

namespace Jenny.Plugins
{
    public class ConsoleWriteLinePostProcessor : IPostProcessor
    {
        public string Name => "Console.WriteLine generated files";
        public int Order => 200;
        public bool RunInDryMode => true;

        public CodeGenFile[] PostProcess(CodeGenFile[] files)
        {
            Console.WriteLine(files.Aggregate(
                string.Empty,
                (acc, file) => $"{acc}{file.FileName} - {file.GeneratorName}\n")
            );

            return files;
        }
    }
}
