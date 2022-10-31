using System.Collections.Generic;
using System.IO;
using DesperateDevs.Serialization;
using Sherlog;

namespace Jenny.Plugins
{
    public class CleanTargetDirectoryPostProcessor : IPostProcessor, IConfigurable
    {
        public string Name => "Clean target directory";
        public int Order => 0;
        public bool RunInDryMode => false;

        public Dictionary<string, string> DefaultProperties => _targetDirectoryConfig.DefaultProperties;

        readonly Logger _logger = Logger.GetLogger(typeof(CleanTargetDirectoryPostProcessor));

        readonly TargetDirectoryConfig _targetDirectoryConfig = new TargetDirectoryConfig();

        public void Configure(Preferences preferences)
        {
            _targetDirectoryConfig.Configure(preferences);
        }

        public CodeGenFile[] PostProcess(CodeGenFile[] files)
        {
            CleanDir();
            return files;
        }

        void CleanDir()
        {
            if (Directory.Exists(_targetDirectoryConfig.TargetDirectory))
            {
                var files = new DirectoryInfo(_targetDirectoryConfig.TargetDirectory).GetFiles("*.cs", SearchOption.AllDirectories);
                foreach (var file in files)
                {
                    try
                    {
                        File.Delete(file.FullName);
                    }
                    catch
                    {
                        _logger.Error($"Could not delete file {file}");
                    }
                }
            }
            else
            {
                Directory.CreateDirectory(_targetDirectoryConfig.TargetDirectory);
            }
        }
    }
}
