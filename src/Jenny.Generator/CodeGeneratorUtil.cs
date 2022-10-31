using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using DesperateDevs.Serialization;
using DesperateDevs.Extensions;
using DesperateDevs.Reflection;
using Sherlog;

namespace Jenny.Generator
{
    public static class CodeGeneratorUtil
    {
        static readonly Logger _logger = Logger.GetLogger(typeof(CodeGeneratorUtil).FullName);

        public static CodeGenerator CodeGeneratorFromPreferences(Preferences preferences)
        {
            var instances = LoadFromPlugins(preferences);
            var config = preferences.CreateAndConfigure<CodeGeneratorConfig>();

            var preProcessors = GetEnabledInstancesOf<IPreProcessor>(instances, config.PreProcessors);
            var dataProviders = GetEnabledInstancesOf<IDataProvider>(instances, config.DataProviders);
            var codeGenerators = GetEnabledInstancesOf<ICodeGenerator>(instances, config.CodeGenerators);
            var postProcessors = GetEnabledInstancesOf<IPostProcessor>(instances, config.PostProcessors);

            Configure(preProcessors, preferences);
            Configure(dataProviders, preferences);
            Configure(codeGenerators, preferences);
            Configure(postProcessors, preferences);

            return new CodeGenerator(preProcessors, dataProviders, codeGenerators, postProcessors);
        }

        static void Configure(ICodeGenerationPlugin[] plugins, Preferences preferences)
        {
            foreach (var plugin in plugins.OfType<IConfigurable>())
                plugin.Configure(preferences);
        }

        public static ICodeGenerationPlugin[] LoadFromPlugins(Preferences preferences)
        {
            var config = preferences.CreateAndConfigure<CodeGeneratorConfig>();
            var resolver = new AssemblyResolver(config.SearchPaths);
            foreach (var path in config.Plugins)
                resolver.Load(path);

            resolver.Dispose();

            return resolver.GetTypes()
                .GetNonAbstractTypes<ICodeGenerationPlugin>()
                .Select(type =>
                {
                    try
                    {
                        return (ICodeGenerationPlugin)Activator.CreateInstance(type);
                    }
                    catch (TypeLoadException exception)
                    {
                        _logger.Warn(exception.Message);
                    }

                    return null;
                })
                .Where(instance => instance != null)
                .ToArray();
        }

        public static T[] GetOrderedInstancesOf<T>(ICodeGenerationPlugin[] instances) where T : ICodeGenerationPlugin => instances
            .OfType<T>()
            .OrderBy(instance => instance.Order)
            .ThenBy(instance => instance.GetType().ToCompilableString())
            .ToArray();

        public static string[] GetOrderedTypeNamesOf<T>(ICodeGenerationPlugin[] instances) where T : ICodeGenerationPlugin =>
            GetOrderedInstancesOf<T>(instances)
                .Select(instance => instance.GetType().ToCompilableString())
                .ToArray();

        public static T[] GetEnabledInstancesOf<T>(ICodeGenerationPlugin[] instances, string[] typeNames) where T : ICodeGenerationPlugin =>
            GetOrderedInstancesOf<T>(instances)
                .Where(instance => typeNames.Contains(instance.GetType().ToCompilableString()))
                .ToArray();

        public static string[] GetAvailableNamesOf<T>(ICodeGenerationPlugin[] instances, string[] typeNames) where T : ICodeGenerationPlugin =>
            GetOrderedTypeNamesOf<T>(instances)
                .Where(typeName => !typeNames.Contains(typeName))
                .ToArray();

        public static string[] GetUnavailableNamesOf<T>(ICodeGenerationPlugin[] instances, string[] typeNames) where T : ICodeGenerationPlugin
        {
            var orderedTypeNames = GetOrderedTypeNamesOf<T>(instances);
            return typeNames
                .Where(typeName => !orderedTypeNames.Contains(typeName))
                .ToArray();
        }

        public static Dictionary<string, string> GetDefaultProperties(ICodeGenerationPlugin[] instances, CodeGeneratorConfig config) =>
            new Dictionary<string, string>().Merge(
                GetEnabledInstancesOf<IPreProcessor>(instances, config.PreProcessors).OfType<IConfigurable>()
                    .Concat(GetEnabledInstancesOf<IDataProvider>(instances, config.DataProviders).OfType<IConfigurable>())
                    .Concat(GetEnabledInstancesOf<ICodeGenerator>(instances, config.CodeGenerators).OfType<IConfigurable>())
                    .Concat(GetEnabledInstancesOf<IPostProcessor>(instances, config.PostProcessors).OfType<IConfigurable>())
                    .Select(instance => instance.DefaultProperties));

        public static string[] BuildSearchPaths(string[] searchPaths, string[] additionalSearchPaths) => searchPaths
            .Concat(additionalSearchPaths)
            .Where(Directory.Exists)
            .ToArray();

        public static void AutoImport(CodeGeneratorConfig config, params string[] searchPaths)
        {
            var assemblyPaths = AssemblyResolver
                .LoadAssemblies(true, searchPaths)
                .GetTypes()
                .GetNonAbstractTypes<ICodeGenerationPlugin>()
                .Select(type => type.Assembly)
                .Distinct()
                .Select(assembly => assembly.CodeBase.MakePathRelativeTo(Directory.GetCurrentDirectory()))
                .ToArray();

            var currentFullPaths = new HashSet<string>(config.SearchPaths.Select(Path.GetFullPath));
            var newPaths = assemblyPaths
                .Select(Path.GetDirectoryName)
                .Where(path => !currentFullPaths.Contains(path));

            config.SearchPaths = config.SearchPaths
                .Concat(newPaths)
                .Distinct()
                .OrderBy(path => path)
                .ToArray();

            config.Plugins = assemblyPaths
                .Select(Path.GetFileNameWithoutExtension)
                .Distinct()
                .OrderBy(plugin => plugin)
                .ToArray();
        }
    }
}
