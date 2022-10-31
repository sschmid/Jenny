using DesperateDevs.Extensions;

namespace Jenny
{
    public class CodeGenFile
    {
        public string FileName { get; set; }

        public string FileContent
        {
            get => _fileContent;
            set => _fileContent = value.ToUnixLineEndings();
        }

        public string GeneratorName { get; set; }

        string _fileContent;

        public CodeGenFile(string fileName, string fileContent, string generatorName)
        {
            FileName = fileName;
            FileContent = fileContent;
            GeneratorName = generatorName;
        }
    }
}
