using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using DesperateDevs.Cli.Utils;
using DesperateDevs.Serialization;
using DesperateDevs.Serialization.Cli.Utils;
using DesperateDevs.Extensions;
using DesperateDevs.Reflection;

namespace Jenny.Generator.Cli
{
    public class FixCommand : AbstractPreferencesCommand
    {
        public override string Trigger => "fix";
        public override string Description => "Add missing keys and add available or remove unavailable plugins interactively";
        public override string Group => CommandGroups.Plugins;
        public override string Example => "fix";

        static bool _silent;

        public FixCommand() : base(typeof(FixCommand).FullName) { }

        protected override void Run()
        {
            _silent = _rawArgs.IsSilent();

            var config = _preferences.CreateAndConfigure<CodeGeneratorConfig>();
            ForceAddMissingKeys(config.DefaultProperties, _preferences);

            var instances = CodeGeneratorUtil.LoadFromPlugins(_preferences);
            // A test to check if all types can be resolved and instantiated.
            CodeGeneratorUtil.GetEnabledInstancesOf<IPreProcessor>(instances, config.PreProcessors);
            CodeGeneratorUtil.GetEnabledInstancesOf<IDataProvider>(instances, config.DataProviders);
            CodeGeneratorUtil.GetEnabledInstancesOf<ICodeGenerator>(instances, config.CodeGenerators);
            CodeGeneratorUtil.GetEnabledInstancesOf<IPostProcessor>(instances, config.PostProcessors);

            var askedRemoveKeys = new HashSet<string>();
            var askedAddKeys = new HashSet<string>();
            while (Fix(askedRemoveKeys, askedAddKeys, instances, config, _preferences)) { }

            RunDoctors();
            FixSearchPath(instances, config, _preferences);
        }

        void RunDoctors()
        {
            var doctors = AppDomain.CurrentDomain.GetInstancesOf<IDoctor>().ToArray();
            foreach (var doctor in doctors.OfType<IConfigurable>())
                doctor.Configure(_preferences);

            foreach (var doctor in doctors)
            {
                var diagnosis = doctor.Diagnose();
                if (diagnosis.Severity == DiagnosisSeverity.Error)
                {
                    if (_silent)
                    {
                        if (doctor.ApplyFix())
                        {
                            _preferences.Save();
                            _logger.Info($"💉  Applied fix: {diagnosis.Treatment}");
                        }
                    }
                    else
                    {
                        Console.WriteLine($"💉  Apply fix: {diagnosis.Treatment}");
                        Console.WriteLine($"to treat symptoms: {diagnosis.Symptoms} ? (y / n)");
                        if (PreferencesExtension.GetUserDecision() && doctor.ApplyFix())
                            _preferences.Save();
                    }
                }
            }
        }

        void FixSearchPath(ICodeGenerationPlugin[] instances, CodeGeneratorConfig config, Preferences preferences)
        {
            var requiredSearchPaths = instances
                .Select(instance => Path.GetDirectoryName(instance.GetType().Assembly.Location.MakePathRelativeTo(Directory.GetCurrentDirectory())))
                .Distinct()
                .Select(Path.GetFullPath)
                .OrderBy(path => path)
                .ToArray();

            var unusedPaths = config.SearchPaths
                .Where(path => !requiredSearchPaths.Contains(Path.GetFullPath(path)));

            foreach (var path in unusedPaths)
            {
                if (_silent)
                {
                    preferences.RemoveValue(
                        path,
                        config.SearchPaths,
                        values => config.SearchPaths = values.ToArray()
                    );
                }
                else
                {
                    preferences.AskRemoveValue(
                        "Remove unused search path",
                        path,
                        config.SearchPaths,
                        values => config.SearchPaths = values.ToArray()
                    );
                }
            }

            config.SearchPaths = config.SearchPaths.Distinct().ToArray();
            preferences.Save();
        }

        static void ForceAddMissingKeys(Dictionary<string, string> requiredProperties, Preferences preferences)
        {
            var requiredKeys = requiredProperties.Keys.ToArray();
            var missingKeys = preferences.GetMissingKeys(requiredKeys);

            foreach (var key in missingKeys)
            {
                if (_silent)
                    preferences.AddKey(key, requiredProperties[key]);
                else
                    preferences.NotifyForceAddKey("Will add missing key", key, requiredProperties[key]);
            }
        }

        bool Fix(HashSet<string> askedRemoveKeys, HashSet<string> askedAddKeys, ICodeGenerationPlugin[] instances, CodeGeneratorConfig config, Preferences preferences)
        {
            var changed = FixPlugins(askedRemoveKeys, askedAddKeys, instances, config, preferences);
            changed |= FixCollisions(askedAddKeys, config, preferences);

            ForceAddMissingKeys(CodeGeneratorUtil.GetDefaultProperties(instances, config), preferences);

            var requiredKeys = config.DefaultProperties
                .Merge(CodeGeneratorUtil.GetDefaultProperties(instances, config))
                .Keys
                .ToArray();

            RemoveUnusedKeys(askedRemoveKeys, requiredKeys, preferences);

            return changed;
        }

        static bool FixPlugins(HashSet<string> askedRemoveKeys, HashSet<string> askedAddKeys, ICodeGenerationPlugin[] instances, CodeGeneratorConfig config, Preferences preferences)
        {
            var changed = false;

            var unavailablePreProcessors = CodeGeneratorUtil.GetUnavailableNamesOf<IPreProcessor>(instances, config.PreProcessors);
            var unavailableDataProviders = CodeGeneratorUtil.GetUnavailableNamesOf<IDataProvider>(instances, config.DataProviders);
            var unavailableCodeGenerators = CodeGeneratorUtil.GetUnavailableNamesOf<ICodeGenerator>(instances, config.CodeGenerators);
            var unavailablePostProcessors = CodeGeneratorUtil.GetUnavailableNamesOf<IPostProcessor>(instances, config.PostProcessors);

            var availablePreProcessors = CodeGeneratorUtil.GetAvailableNamesOf<IPreProcessor>(instances, config.PreProcessors);
            var availableDataProviders = CodeGeneratorUtil.GetAvailableNamesOf<IDataProvider>(instances, config.DataProviders);
            var availableCodeGenerators = CodeGeneratorUtil.GetAvailableNamesOf<ICodeGenerator>(instances, config.CodeGenerators);
            var availablePostProcessors = CodeGeneratorUtil.GetAvailableNamesOf<IPostProcessor>(instances, config.PostProcessors);

            foreach (var value in unavailablePreProcessors)
            {
                if (!askedRemoveKeys.Contains(value))
                {
                    if (_silent)
                    {
                        preferences.RemoveValue(
                            value,
                            config.PreProcessors,
                            values => config.PreProcessors = values.ToArray()
                        );
                    }
                    else
                    {
                        preferences.AskRemoveValue(
                            "Remove unavailable pre processor",
                            value,
                            config.PreProcessors,
                            values => config.PreProcessors = values.ToArray()
                        );
                    }

                    askedRemoveKeys.Add(value);
                    changed = true;
                }
            }

            foreach (var value in unavailableDataProviders)
            {
                if (!askedRemoveKeys.Contains(value))
                {
                    if (_silent)
                    {
                        preferences.RemoveValue(
                            value,
                            config.DataProviders,
                            values => config.DataProviders = values.ToArray()
                        );
                    }
                    else
                    {
                        preferences.AskRemoveValue(
                            "Remove unavailable data provider",
                            value,
                            config.DataProviders,
                            values => config.DataProviders = values.ToArray()
                        );
                    }

                    askedRemoveKeys.Add(value);
                    changed = true;
                }
            }

            foreach (var value in unavailableCodeGenerators)
            {
                if (!askedRemoveKeys.Contains(value))
                {
                    if (_silent)
                    {
                        preferences.RemoveValue(
                            value,
                            config.CodeGenerators,
                            values => config.CodeGenerators = values.ToArray()
                        );
                    }
                    else
                    {
                        preferences.AskRemoveValue(
                            "Remove unavailable code generator",
                            value,
                            config.CodeGenerators,
                            values => config.CodeGenerators = values.ToArray()
                        );
                    }

                    askedRemoveKeys.Add(value);
                    changed = true;
                }
            }

            foreach (var value in unavailablePostProcessors)
            {
                if (!askedRemoveKeys.Contains(value))
                {
                    if (_silent)
                    {
                        preferences.RemoveValue(
                            value,
                            config.PostProcessors,
                            values => config.PostProcessors = values.ToArray()
                        );
                    }
                    else
                    {
                        preferences.AskRemoveValue(
                            "Remove unavailable post processor",
                            value,
                            config.PostProcessors,
                            values => config.PostProcessors = values.ToArray()
                        );
                    }

                    askedRemoveKeys.Add(value);
                    changed = true;
                }
            }

            foreach (var value in availablePreProcessors)
            {
                if (!askedAddKeys.Contains(value))
                {
                    if (_silent)
                    {
                        preferences.AddValue(
                            value,
                            config.PreProcessors,
                            values => config.PreProcessors = values.ToArray()
                        );
                    }
                    else
                    {
                        preferences.AskAddValue(
                            "Add available pre processor",
                            value,
                            config.PreProcessors,
                            values => config.PreProcessors = values.ToArray()
                        );
                    }

                    askedAddKeys.Add(value);
                    changed = true;
                }
            }

            foreach (var value in availableDataProviders)
            {
                if (!askedAddKeys.Contains(value))
                {
                    if (_silent)
                    {
                        preferences.AddValue(
                            value,
                            config.DataProviders,
                            values => config.DataProviders = values.ToArray()
                        );
                    }
                    else
                    {
                        preferences.AskAddValue(
                            "Add available data provider",
                            value,
                            config.DataProviders,
                            values => config.DataProviders = values.ToArray()
                        );
                    }

                    askedAddKeys.Add(value);
                    changed = true;
                }
            }

            foreach (var value in availableCodeGenerators)
            {
                if (!askedAddKeys.Contains(value))
                {
                    if (_silent)
                    {
                        preferences.AddValue(
                            value,
                            config.CodeGenerators,
                            values => config.CodeGenerators = values.ToArray()
                        );
                    }
                    else
                    {
                        preferences.AskAddValue(
                            "Add available code generator",
                            value,
                            config.CodeGenerators,
                            values => config.CodeGenerators = values.ToArray()
                        );
                    }

                    askedAddKeys.Add(value);
                    changed = true;
                }
            }

            foreach (var value in availablePostProcessors)
            {
                if (!askedAddKeys.Contains(value))
                {
                    if (_silent)
                    {
                        preferences.AddValue(
                            value,
                            config.PostProcessors,
                            values => config.PostProcessors = values.ToArray()
                        );
                    }
                    else
                    {
                        preferences.AskAddValue(
                            "Add available post processor",
                            value,
                            config.PostProcessors,
                            values => config.PostProcessors = values.ToArray()
                        );
                    }

                    askedAddKeys.Add(value);
                    changed = true;
                }
            }

            return changed;
        }

        bool FixCollisions(HashSet<string> askedAddKeys, CodeGeneratorConfig config, Preferences preferences)
        {
            var changed = FixDuplicates(askedAddKeys, config.PreProcessors, values =>
            {
                config.PreProcessors = values;
                return config.PreProcessors;
            }, preferences);

            changed = FixDuplicates(askedAddKeys, config.DataProviders, values =>
            {
                config.DataProviders = values;
                return config.DataProviders;
            }, preferences) | changed;

            changed = FixDuplicates(askedAddKeys, config.CodeGenerators, values =>
            {
                config.CodeGenerators = values;
                return config.CodeGenerators;
            }, preferences) | changed;

            return FixDuplicates(askedAddKeys, config.PostProcessors, values =>
            {
                config.PostProcessors = values;
                return config.PostProcessors;
            }, preferences) | changed;
        }

        bool FixDuplicates(HashSet<string> askedAddKeys, string[] values, Func<string[], string[]> updateAction, Preferences preferences)
        {
            var changed = false;
            var duplicates = GetDuplicates(values);

            foreach (var duplicate in duplicates)
            {
                Console.WriteLine($"⚠️  Potential plugin collision: {duplicate}");
                Console.WriteLine("0: Keep all (no changes)");

                var collisions = values
                    .Where(name => name.EndsWith(duplicate))
                    .ToArray();

                PrintCollisions(collisions);
                var inputChars = GetInputChars(collisions);
                var keyChar = PreferencesExtension.GetGenericUserDecision(inputChars);
                if (keyChar != '0')
                {
                    var index = int.Parse(keyChar.ToString()) - 1;
                    var keep = collisions[index];

                    foreach (var collision in collisions)
                    {
                        if (collision != keep)
                        {
                            preferences.RemoveValue(
                                collision,
                                values,
                                result => values = updateAction(result.ToArray())
                            );
                            askedAddKeys.Add(collision);
                            changed = true;
                        }
                    }
                }
            }

            return changed;
        }

        static string[] GetDuplicates(string[] values)
        {
            var shortNames = values
                .Select(name => name.TypeName())
                .ToArray();

            return values
                .Where(name => shortNames.Count(n => n == name.TypeName()) > 1)
                .Select(name => name.TypeName())
                .Distinct()
                .OrderBy(name => name.TypeName())
                .ToArray();
        }

        void PrintCollisions(string[] collisions)
        {
            for (var i = 0; i < collisions.Length; i++)
                Console.WriteLine($"{i + 1}: Keep {collisions[i]}");
        }

        static char[] GetInputChars(string[] collisions)
        {
            var chars = new char[collisions.Length + 1];
            for (var i = 0; i < collisions.Length; i++)
                chars[i] = (i + 1).ToString()[0];

            chars[chars.Length - 1] = '0';
            return chars;
        }

        static void RemoveUnusedKeys(HashSet<string> askedRemoveKeys, string[] requiredKeys, Preferences preferences)
        {
            var unusedKeys = preferences.GetUnusedKeys(requiredKeys);
            foreach (var key in unusedKeys)
            {
                if (!askedRemoveKeys.Contains(key))
                {
                    if (_silent)
                        preferences.RemoveKey(key);
                    else
                        preferences.AskRemoveKey("Remove unused key", key);

                    askedRemoveKeys.Add(key);
                }
            }
        }
    }
}
