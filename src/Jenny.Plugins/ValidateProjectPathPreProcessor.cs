using System;
using System.Collections.Generic;
using System.IO;
using DesperateDevs.Serialization;

namespace Jenny.Plugins
{
    public class ValidateProjectPathPreProcessor : IPreProcessor, IConfigurable
    {
        public string Name => "Validate Project Path";
        public int Order => -10;
        public bool RunInDryMode => true;

        public Dictionary<string, string> DefaultProperties => _projectPathConfig.DefaultProperties;

        readonly ProjectPathConfig _projectPathConfig = new ProjectPathConfig();

        public void Configure(Preferences preferences)
        {
            _projectPathConfig.Configure(preferences);
        }

        public void PreProcess()
        {
            if (!File.Exists(_projectPathConfig.ProjectPath))
            {
                throw new Exception(
                    @"Could not find file '" + _projectPathConfig.ProjectPath + "\'\n" +
                    "Press \"Assets -> Open C# Project\" to create the project and make sure that \"Project Path\" is set to the created *.csproj."
                );
            }
        }
    }
}
