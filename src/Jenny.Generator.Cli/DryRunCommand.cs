using DesperateDevs.Serialization.Cli.Utils;

namespace Jenny.Generator.Cli
{
    public class DryRunCommand : AbstractPreferencesCommand
    {
        public override string Trigger => "dry";
        public override string Description => "Run the code generator in dry mode";
        public override string Group => CommandGroups.CodeGeneration;
        public override string Example => "dry";

        public DryRunCommand() : base(typeof(DryRunCommand).FullName) { }

        protected override void Run()
        {
            var codeGenerator = CodeGeneratorUtil.CodeGeneratorFromPreferences(_preferences);
            codeGenerator.OnProgress += (title, info, progress) =>
            {
                var p = (int)(progress * 100);
                _logger.Debug($"{title}: {info} ({p}%)");
            };
            codeGenerator.DryRun();
        }
    }
}
