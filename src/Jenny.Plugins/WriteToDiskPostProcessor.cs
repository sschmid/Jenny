using System.Collections.Generic;
using System.IO;
using DesperateDevs.Serialization;

namespace Jenny.Plugins
{
    public class WriteToDiskPostProcessor : IPostProcessor, IConfigurable
    {
        public string Name => "Write to disk";
        public int Order => 100;
        public bool RunInDryMode => false;

        public Dictionary<string, string> DefaultProperties => _targetDirectoryConfig.DefaultProperties;

        readonly TargetDirectoryConfig _targetDirectoryConfig = new TargetDirectoryConfig();

        public void Configure(Preferences preferences)
        {
            _targetDirectoryConfig.Configure(preferences);
        }

        public CodeGenFile[] PostProcess(CodeGenFile[] files)
        {
            foreach (var file in files)
            {
                var fileName = Path.Combine(_targetDirectoryConfig.TargetDirectory, file.FileName);
                var targetDir = Path.GetDirectoryName(fileName);
                if (!Directory.Exists(targetDir))
                    Directory.CreateDirectory(targetDir);

                File.WriteAllText(fileName, file.FileContent);
            }

            return files;
        }
    }
}
