using System;
using System.Collections.Generic;
using System.Linq;
using DesperateDevs.Serialization;
using DesperateDevs.Reflection;
using Jenny.Generator;

namespace Jenny.Plugins.Unity
{
    public class DebugLogDoctor : IDoctor, IConfigurable
    {
        public string Name => "Debug.Log";
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
                var typeName = typeof(DebugLogPostProcessor).FullName;
                if (_codeGeneratorConfig.PostProcessors.Contains(typeName))
                {
                    return new Diagnosis(
                        $"{typeName} uses Unity APIs but is used outside of Unity!",
                        $"Remove {typeName} from CodeGenerator.PostProcessors",
                        DiagnosisSeverity.Error
                    );
                }
            }

            return Diagnosis.Healthy;
        }

        public bool ApplyFix()
        {
            var postProcessorList = _codeGeneratorConfig.PostProcessors.ToList();
            var removed = postProcessorList.Remove(typeof(DebugLogPostProcessor).FullName);
            if (removed)
            {
                _codeGeneratorConfig.PostProcessors = postProcessorList.ToArray();
                return true;
            }
            else
            {
                return false;
            }
        }
    }
}
