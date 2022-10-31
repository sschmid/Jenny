using System.Linq;
using DesperateDevs.Serialization.Cli.Utils;

namespace Jenny.Generator.Cli
{
    public class ScanCommand : AbstractPreferencesCommand
    {
        public override string Trigger => "scan";
        public override string Description => "Scan and print available types found in specified plugins";
        public override string Group => CommandGroups.Plugins;
        public override string Example => "scan";

        public ScanCommand() : base(typeof(ScanCommand).FullName) { }

        protected override void Run()
        {
            var instances = CodeGeneratorUtil.LoadFromPlugins(_preferences);

            var orderedTypes = instances
                .Select(instance => instance.GetType())
                .OrderBy(type => type.Assembly.GetName().Name)
                .ThenBy(type => type.FullName);

            foreach (var type in orderedTypes)
                _logger.Info($"{type.Assembly.GetName().Name}: {type}");
        }
    }
}
