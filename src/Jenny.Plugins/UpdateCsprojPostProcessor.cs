using System.Collections.Generic;
using System.IO;
using System.Text.RegularExpressions;
using System.Linq;
using DesperateDevs.Serialization;
using DesperateDevs.Extensions;

namespace Jenny.Plugins
{
    public class UpdateCsprojPostProcessor : IPostProcessor, IConfigurable
    {
        public string Name => "Update .csproj";
        public int Order => 96;
        public bool RunInDryMode => false;

        public Dictionary<string, string> DefaultProperties =>
            _projectPathConfig.DefaultProperties.Merge(_targetDirectoryConfig.DefaultProperties);

        readonly ProjectPathConfig _projectPathConfig = new ProjectPathConfig();
        readonly TargetDirectoryConfig _targetDirectoryConfig = new TargetDirectoryConfig();

        public void Configure(Preferences preferences)
        {
            _projectPathConfig.Configure(preferences);
            _targetDirectoryConfig.Configure(preferences);
        }

        public CodeGenFile[] PostProcess(CodeGenFile[] files)
        {
            var project = File.ReadAllText(_projectPathConfig.ProjectPath);
            project = RemoveExistingGeneratedEntries(project);
            project = AddGeneratedEntries(project, files);
            File.WriteAllText(_projectPathConfig.ProjectPath, project);
            return files;
        }

        string RemoveExistingGeneratedEntries(string project)
        {
            var escapedTargetDirectory = _targetDirectoryConfig.TargetDirectory
                .Replace("/", "\\")
                .Replace("\\", "\\\\");

            var unixTargetDirectory = _targetDirectoryConfig.TargetDirectory.ToUnixPath();

            project = Regex.Replace(project, $@"\s*<Compile Include=""{escapedTargetDirectory}.* \/>", string.Empty);
            project = Regex.Replace(project, $@"\s*<Compile Include=""{unixTargetDirectory}.* \/>", string.Empty);
            return Regex.Replace(project, @"\s*<ItemGroup>\s*<\/ItemGroup>", string.Empty);
        }

        string AddGeneratedEntries(string project, CodeGenFile[] files)
        {
            var entries = string.Join("\r\n", files.Select(file =>
            {
                var path = Path.Combine(_targetDirectoryConfig.TargetDirectory, file.FileName).ToUnixPath();
                return $@"    <Compile Include=""{path}"" />";
            }));

            return new Regex(@"<\/ItemGroup>").Replace(project, $@"</ItemGroup>
  <ItemGroup>
{entries}
  </ItemGroup>", 1);
        }
    }
}
