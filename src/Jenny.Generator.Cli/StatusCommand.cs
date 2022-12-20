using System;
using System.Collections.Generic;
using System.Linq;
using DesperateDevs.Serialization;
using DesperateDevs.Serialization.Cli.Utils;
using DesperateDevs.Extensions;

namespace Jenny.Generator.Cli
{
    public class StatusCommand : AbstractPreferencesCommand
    {
        public override string Trigger => "status";
        public override string Description => "List available and unavailable plugins";
        public override string Group => CommandGroups.Plugins;
        public override string Example => "status";

        public StatusCommand() : base(typeof(StatusCommand).FullName) { }

        protected override void Run()
        {
            var config = _preferences.CreateAndConfigure<CodeGeneratorConfig>();

            _logger.Debug(_preferences.ToString());

            ICodeGenerationPlugin[] instances = null;
            Dictionary<string, string> defaultProperties = null;

            try
            {
                instances = CodeGeneratorUtil.LoadFromPlugins(_preferences);
                defaultProperties = CodeGeneratorUtil.GetDefaultProperties(instances, config);
            }
            catch (Exception)
            {
                PrintKeyStatus(
                    config.DefaultProperties.Keys.ToArray(),
                    _preferences
                );
                throw;
            }

            var requiredKeys = config.DefaultProperties
                .Merge(defaultProperties).Keys.ToArray();

            PrintKeyStatus(requiredKeys, _preferences);
            PrintPluginStatus(instances, config);
            PrintCollisions(config);
        }

        void PrintKeyStatus(string[] requiredKeys, Preferences preferences)
        {
            var unusedKeys = preferences.GetUnusedKeys(requiredKeys);
            foreach (var key in unusedKeys)
                _logger.Info($"ℹ️️  Unused key: {key}");

            foreach (var key in preferences.GetMissingKeys(requiredKeys))
                _logger.Warn($"⚠️  Missing key: {key}");
        }

        void PrintPluginStatus(ICodeGenerationPlugin[] instances, CodeGeneratorConfig config)
        {
            PrintUnavailable(CodeGeneratorUtil.GetUnavailableNamesOf<IPreProcessor>(instances, config.PreProcessors));
            PrintUnavailable(CodeGeneratorUtil.GetUnavailableNamesOf<IDataProvider>(instances, config.DataProviders));
            PrintUnavailable(CodeGeneratorUtil.GetUnavailableNamesOf<ICodeGenerator>(instances, config.CodeGenerators));
            PrintUnavailable(CodeGeneratorUtil.GetUnavailableNamesOf<IPostProcessor>(instances, config.PostProcessors));

            PrintAvailable(CodeGeneratorUtil.GetAvailableNamesOf<IPreProcessor>(instances, config.PreProcessors));
            PrintAvailable(CodeGeneratorUtil.GetAvailableNamesOf<IDataProvider>(instances, config.DataProviders));
            PrintAvailable(CodeGeneratorUtil.GetAvailableNamesOf<ICodeGenerator>(instances, config.CodeGenerators));
            PrintAvailable(CodeGeneratorUtil.GetAvailableNamesOf<IPostProcessor>(instances, config.PostProcessors));
        }

        void PrintUnavailable(string[] names)
        {
            foreach (var name in names)
                _logger.Warn($"⚠️  Unavailable: {name}");
        }

        void PrintAvailable(string[] names)
        {
            foreach (var name in names)
                _logger.Info($"ℹ️  Available: {name}");
        }

        void PrintCollisions(CodeGeneratorConfig config)
        {
            PrintDuplicates(config.PreProcessors);
            PrintDuplicates(config.DataProviders);
            PrintDuplicates(config.CodeGenerators);
            PrintDuplicates(config.PostProcessors);
        }

        void PrintDuplicates(string[] names)
        {
            var shortNames = names
                .Select(name => name.TypeName())
                .ToArray();

            var duplicates = names
                .Where(name => shortNames.Count(n => n == name.TypeName()) > 1)
                .OrderBy(name => name.TypeName());

            foreach (var duplicate in duplicates)
                _logger.Warn($"⚠️  Potential collision detected: {duplicate.TypeName()} -> {duplicate}");
        }
    }
}
