using DesperateDevs.Serialization;
using DesperateDevs.Serialization.Cli.Utils;

namespace Jenny.Generator.Cli
{
    public class AutoImportCommand : AbstractPreferencesCommand
    {
        public override string Trigger => "auto-import";
        public override string Description => "Find and import all plugins";
        public override string Group => CommandGroups.Plugins;
        public override string Example => "auto-import";

        public AutoImportCommand() : base(typeof(AutoImportCommand).FullName) { }

        protected override void Run()
        {
            _logger.Debug(_preferences.ToString());
            AutoImport();
            new FixCommand().Run(_program, _rawArgs);
        }

        void AutoImport()
        {
            var config = _preferences.CreateAndConfigure<CodeGeneratorConfig>();
            var searchPaths = CodeGeneratorUtil.BuildSearchPaths(config.SearchPaths, new[] {"."});
            CodeGeneratorUtil.AutoImport(config, searchPaths);
            _preferences.Save();
        }
    }
}
