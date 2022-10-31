using System;
using System.Collections.Generic;
using System.Linq;
using DesperateDevs.Serialization;
using DesperateDevs.Reflection;
using Jenny.Generator;

namespace Jenny.Plugins.Unity
{
    public class WarnIfCompilationErrorsDoctor : IDoctor, IConfigurable
    {
        public string Name => "Warn If Compilation Errors";
        public int Order => 0;
        public bool RunInDryMode => true;

        public Dictionary<string, string> DefaultProperties => new Dictionary<string, string>();

        readonly CodeGeneratorConfig _codeGeneratorConfig = new CodeGeneratorConfig();

        public void Configure(Preferences preferences)
        {
            _codeGeneratorConfig.Configure(preferences);
        }

        public Diagnosis Diagnose()
        {
            var isStandalone = AppDomain.CurrentDomain
                .GetAllTypes()
                .Any(type => type.FullName.StartsWith("Jenny.Generator.Cli"));

            if (isStandalone)
            {
                var typeName = typeof(WarnIfCompilationErrorsPreProcessor).FullName;
                if (_codeGeneratorConfig.PreProcessors.Contains(typeName))
                {
                    return new Diagnosis(
                        $"{typeName} uses Unity APIs but is used outside of Unity!",
                        $"Remove {typeName} from CodeGenerator.PreProcessors",
                        DiagnosisSeverity.Error
                    );
                }
            }

            return Diagnosis.Healthy;
        }

        public bool ApplyFix()
        {
            var preProcessorList = _codeGeneratorConfig.PreProcessors.ToList();
            var removed = preProcessorList.Remove(typeof(WarnIfCompilationErrorsPreProcessor).FullName);
            if (removed)
            {
                _codeGeneratorConfig.PreProcessors = preProcessorList.ToArray();
                return true;
            }
            else
            {
                return false;
            }
        }
    }
}
