using System;
using System.Collections.Generic;
using System.Linq;

namespace Jenny.Generator
{
    public delegate void GeneratorProgress(string title, string info, float progress);

    public class CodeGenerator
    {
        public static string DefaultPropertiesPath => "Jenny.properties";

        public event GeneratorProgress OnProgress;

        readonly IPreProcessor[] _preProcessors;
        readonly IDataProvider[] _dataProviders;
        readonly ICodeGenerator[] _codeGenerators;
        readonly IPostProcessor[] _postProcessors;

        readonly Dictionary<string, object> _objectCache;

        bool _cancel;

        public CodeGenerator(
            IPreProcessor[] preProcessors,
            IDataProvider[] dataProviders,
            ICodeGenerator[] codeGenerators,
            IPostProcessor[] postProcessors)
        {
            _preProcessors = preProcessors.OrderBy(i => i.Order).ToArray();
            _dataProviders = dataProviders.OrderBy(i => i.Order).ToArray();
            _codeGenerators = codeGenerators.OrderBy(i => i.Order).ToArray();
            _postProcessors = postProcessors.OrderBy(i => i.Order).ToArray();
            _objectCache = new Dictionary<string, object>();
        }

        public CodeGenFile[] DryRun() => Generate(
            "[Dry Run] ",
            _preProcessors.Where(i => i.RunInDryMode).ToArray(),
            _dataProviders.Where(i => i.RunInDryMode).ToArray(),
            _codeGenerators.Where(i => i.RunInDryMode).ToArray(),
            _postProcessors.Where(i => i.RunInDryMode).ToArray()
        );

        public CodeGenFile[] Generate() => Generate(
            string.Empty,
            _preProcessors,
            _dataProviders,
            _codeGenerators,
            _postProcessors
        );

        CodeGenFile[] Generate(string messagePrefix,
            IPreProcessor[] preProcessors,
            IDataProvider[] dataProviders,
            ICodeGenerator[] codeGenerators,
            IPostProcessor[] postProcessors)
        {
            _cancel = false;

            _objectCache.Clear();

            var cachables = ((ICodeGenerationPlugin[])preProcessors)
                .Concat(dataProviders)
                .Concat(codeGenerators)
                .Concat(postProcessors)
                .OfType<ICachable>();

            foreach (var cachable in cachables)
                cachable.ObjectCache = _objectCache;

            var total = preProcessors.Length + dataProviders.Length + codeGenerators.Length + postProcessors.Length;
            var progress = 0;

            foreach (var preProcessor in preProcessors)
            {
                if (_cancel) return Array.Empty<CodeGenFile>();
                progress += 1;
                OnProgress?.Invoke($"{messagePrefix}Pre Processing", preProcessor.Name, (float)progress / total);
                preProcessor.PreProcess();
            }

            var data = new List<CodeGeneratorData>();
            foreach (var dataProvider in dataProviders)
            {
                if (_cancel) return Array.Empty<CodeGenFile>();
                progress += 1;
                OnProgress?.Invoke($"{messagePrefix}Creating model", dataProvider.Name, (float)progress / total);
                data.AddRange(dataProvider.GetData());
            }

            var files = new List<CodeGenFile>();
            var dataArray = data.ToArray();
            foreach (var generator in codeGenerators)
            {
                if (_cancel) return Array.Empty<CodeGenFile>();
                progress += 1;
                OnProgress?.Invoke($"{messagePrefix}Creating files", generator.Name, (float)progress / total);
                files.AddRange(generator.Generate(dataArray));
            }

            var generatedFiles = files.ToArray();
            foreach (var postProcessor in postProcessors)
            {
                if (_cancel) return Array.Empty<CodeGenFile>();
                progress += 1;
                OnProgress?.Invoke($"{messagePrefix}Post Processing", postProcessor.Name, (float)progress / total);
                generatedFiles = postProcessor.PostProcess(generatedFiles);
            }

            return generatedFiles;
        }

        public void Cancel() => _cancel = true;
    }
}
