using System.Collections.Generic;
using System.IO;
using System.Text.RegularExpressions;
using DesperateDevs.Serialization;

namespace Jenny.Plugins
{
    public class TargetFrameworkProfilePreProcessor : IPreProcessor, IConfigurable
    {
        public string Name => "Fix Target Framework Profile";
        public int Order => 0;
        public bool RunInDryMode => true;

        public Dictionary<string, string> DefaultProperties => _projectPathConfig.DefaultProperties;

        readonly ProjectPathConfig _projectPathConfig = new ProjectPathConfig();

        public void Configure(Preferences preferences)
        {
            _projectPathConfig.Configure(preferences);
        }

        public void PreProcess()
        {
            var project = File.ReadAllText(_projectPathConfig.ProjectPath);
            project = Regex.Replace(project, @"\s*<TargetFrameworkProfile>Unity Subset v3.5</TargetFrameworkProfile>", string.Empty);
            project = Regex.Replace(project, @"\s*<TargetFrameworkProfile>Unity Full v3.5</TargetFrameworkProfile>", string.Empty);
            File.WriteAllText(_projectPathConfig.ProjectPath, project);
        }
    }
}
