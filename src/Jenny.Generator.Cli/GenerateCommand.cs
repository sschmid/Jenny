using System;
using System.Diagnostics;
using DesperateDevs.Serialization.Cli.Utils;

namespace Jenny.Generator.Cli
{
    public class GenerateCommand : AbstractPreferencesCommand
    {
        public override string Trigger => "gen";
        public override string Description => "Generate files based on properties file";
        public override string Group => CommandGroups.CodeGeneration;
        public override string Example => "gen";

        public GenerateCommand() : base(typeof(GenerateCommand).FullName) { }

        protected override void Run()
        {
            _logger.Info($"Generating using {_preferences.PropertiesPath}");

            var watch = new Stopwatch();
            watch.Start();

            var codeGenerator = CodeGeneratorUtil.CodeGeneratorFromPreferences(_preferences);

            codeGenerator.OnProgress += (title, info, progress) =>
            {
                var p = (int)(progress * 100);
                _logger.Debug($"{title}: {info} ({p}%)");
            };

            var files = codeGenerator.Generate();

            watch.Stop();

            _logger.Info($"[{DateTime.Now.ToLongTimeString()}] Generated {files.Length} files in {(watch.ElapsedMilliseconds / 1000f):0.0} seconds");
        }
    }
}
