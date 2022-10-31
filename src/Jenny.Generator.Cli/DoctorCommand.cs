using System;
using System.Linq;
using DesperateDevs.Serialization;
using DesperateDevs.Serialization.Cli.Utils;
using DesperateDevs.Reflection;

namespace Jenny.Generator.Cli
{
    public class DoctorCommand : AbstractPreferencesCommand
    {
        public override string Trigger => "doctor";
        public override string Description => "Check the config for potential problems";
        public override string Group => CommandGroups.Plugins;
        public override string Example => "doctor";

        public DoctorCommand() : base(typeof(DoctorCommand).FullName) { }

        protected override void Run()
        {
            new StatusCommand().Run(_program, _rawArgs);

            Diagnose();

            _logger.Info("Dry Run");
            CodeGeneratorUtil
                .CodeGeneratorFromPreferences(_preferences)
                .DryRun();

            _logger.Info("👨‍🔬  No problems detected. Happy coding :)");
        }

        void Diagnose()
        {
            var doctors = AppDomain.CurrentDomain.GetInstancesOf<IDoctor>().ToArray();
            foreach (var doctor in doctors.OfType<IConfigurable>())
                doctor.Configure(_preferences);

            var diagnoses = doctors
                .Select(doctor => doctor.Diagnose())
                .ToArray();

            foreach (var diagnosis in diagnoses.Where(d => d.Severity == DiagnosisSeverity.Hint))
            {
                _logger.Info($"👨‍⚕️  Symptoms: {diagnosis.Symptoms}");
                _logger.Info($"💊  Treatment: {diagnosis.Treatment}");
            }

            foreach (var diagnosis in diagnoses.Where(d => d.Severity == DiagnosisSeverity.Warning))
            {
                _logger.Warn($"👨‍⚕️  Symptoms: {diagnosis.Symptoms}");
                _logger.Warn($"💊  Treatment: {diagnosis.Treatment}");
            }

            var errors = string.Join("\n", diagnoses
                .Where(d => d.Severity == DiagnosisSeverity.Error)
                .Select(d => $"👨‍⚕️  Symptoms: {d.Symptoms}\n💊  Treatment: {d.Treatment}")
                .ToArray());

            if (!string.IsNullOrEmpty(errors))
                throw new Exception($"{errors}\nUse 'jenny fix' to apply treatments");
        }
    }
}
