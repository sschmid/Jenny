using System.Collections.Generic;
using System.Linq;
using FluentAssertions;
using Jenny;
using Xunit;

namespace Jenny.Generator.Tests
{
    public class CodeGeneratorTests
    {
        [Fact]
        public void ExecutesPreProcessorsDataProvidersGeneratorsAndPostProcessors()
        {
            var preStr = new List<string>();
            var generator = new CodeGenerator(
                new IPreProcessor[] {new Pre1PreProcessor(preStr)},
                new IDataProvider[] {new Data_1_2_Provider()},
                new ICodeGenerator[] {new DataFile1CodeGenerator()},
                new IPostProcessor[] {new Processed1PostProcessor()}
            );

            var files = generator.Generate();

            preStr.Should().HaveCount(1);
            preStr[0].Should().Be("Pre1");

            files.Should().HaveCount(2);

            files[0].FileName.Should().Be("Test1File0-Processed1");
            files[0].FileContent.Should().Be("data1");

            files[1].FileName.Should().Be("Test1File1-Processed1");
            files[1].FileContent.Should().Be("data2");
        }

        [Fact]
        public void UsesReturnedCodeGenFiles()
        {
            var generator = new CodeGenerator(
                new IPreProcessor[] {new Pre1PreProcessor(new List<string>())},
                new IDataProvider[] {new Data_1_2_Provider()},
                new ICodeGenerator[] {new DataFile1CodeGenerator()},
                new IPostProcessor[] {new Processed1PostProcessor(), new NoFilesPostProcessor()}
            );

            var files = generator.Generate();
            files.Should().HaveCount(1);
            files[0].FileName.Should().Be("Test1File0-Processed1");
        }

        [Fact]
        public void SkipsPluginsWhichDoNotRunInDryRun()
        {
            var preStr = new List<string>();
            var generator = new CodeGenerator(
                new IPreProcessor[] {new Pre1PreProcessor(preStr), new DisabledPreProcessor(preStr)},
                new IDataProvider[] {new Data_1_2_Provider(), new DisabledDataProvider()},
                new ICodeGenerator[] {new DataFile1CodeGenerator(), new DisabledCodeGenerator()},
                new IPostProcessor[] {new Processed1PostProcessor(), new DisabledPostProcessor()}
            );

            var files = generator.DryRun();

            preStr.Should().HaveCount(1);
            preStr[0].Should().Be("Pre1");

            files.Should().HaveCount(2);

            files[0].FileName.Should().Be("Test1File0-Processed1");
            files[1].FileName.Should().Be("Test1File1-Processed1");
        }

        [Fact]
        public void RunsPreProcessorsBasedOnOrder()
        {
            var preStr = new List<string>();
            var generator = new CodeGenerator(
                new IPreProcessor[] {new Pre2PreProcessor(preStr), new Pre1PreProcessor(preStr)},
                new IDataProvider[] {new Data_1_2_Provider()},
                new ICodeGenerator[] {new DataFile1CodeGenerator()},
                new IPostProcessor[] {new Processed1PostProcessor()}
            );

            generator.Generate();

            preStr.Should().HaveCount(2);
            preStr[0].Should().Be("Pre1");
            preStr[1].Should().Be("Pre2");
        }

        [Fact]
        public void RunsDataProviderBasedOnOrder()
        {
            var generator = new CodeGenerator(
                new IPreProcessor[] {new Pre1PreProcessor(new List<string>())},
                new IDataProvider[] {new Data_3_4_Provider(), new Data_1_2_Provider()},
                new ICodeGenerator[] {new DataFile1CodeGenerator()},
                new IPostProcessor[] {new Processed1PostProcessor()}
            );

            var files = generator.Generate();

            files.Should().HaveCount(4);

            files[0].FileName.Should().Be("Test1File0-Processed1");
            files[0].FileContent.Should().Be("data1");

            files[1].FileName.Should().Be("Test1File1-Processed1");
            files[1].FileContent.Should().Be("data2");

            files[2].FileName.Should().Be("Test1File2-Processed1");
            files[2].FileContent.Should().Be("data3");

            files[3].FileName.Should().Be("Test1File3-Processed1");
            files[3].FileContent.Should().Be("data4");
        }

        [Fact]
        public void RunsCodeGeneratorsBasedOnOrder()
        {
            var generator = new CodeGenerator(
                new IPreProcessor[] {new Pre1PreProcessor(new List<string>())},
                new IDataProvider[] {new Data_1_2_Provider()},
                new ICodeGenerator[] {new DataFile2CodeGenerator(), new DataFile1CodeGenerator()},
                new IPostProcessor[] {new Processed1PostProcessor()}
            );

            var files = generator.Generate();

            files.Should().HaveCount(4);

            files[0].FileName.Should().Be("Test1File0-Processed1");
            files[1].FileName.Should().Be("Test1File1-Processed1");
            files[2].FileName.Should().Be("Test2File0-Processed1");
            files[3].FileName.Should().Be("Test2File1-Processed1");
        }

        [Fact]
        public void RunsPostProcessorsBasedOnOrder()
        {
            var generator = new CodeGenerator(
                new IPreProcessor[] {new Pre1PreProcessor(new List<string>())},
                new IDataProvider[] {new Data_1_2_Provider()},
                new ICodeGenerator[] {new DataFile1CodeGenerator()},
                new IPostProcessor[] {new Processed2PostProcessor(), new Processed1PostProcessor()}
            );

            var files = generator.Generate();

            files.Should().HaveCount(2);

            files[0].FileName.Should().Be("Test1File0-Processed1-Processed2");
            files[1].FileName.Should().Be("Test1File1-Processed1-Processed2");
        }

        [Fact]
        public void CancelsRun()
        {
            var generator = new CodeGenerator(
                new IPreProcessor[] {new Pre1PreProcessor(new List<string>())},
                new IDataProvider[] {new Data_1_2_Provider()},
                new ICodeGenerator[] {new DataFile1CodeGenerator()},
                new IPostProcessor[] {new Processed1PostProcessor()}
            );

            generator.OnProgress += delegate { generator.Cancel(); };

            var files = generator.Generate();

            files.Should().HaveCount(0);
        }

        [Fact]
        public void CancelsDryRun()
        {
            var generator = new CodeGenerator(
                new IPreProcessor[] {new Pre1PreProcessor(new List<string>())},
                new IDataProvider[] {new Data_1_2_Provider()},
                new ICodeGenerator[] {new DataFile1CodeGenerator()},
                new IPostProcessor[] {new Processed1PostProcessor()}
            );

            generator.OnProgress += delegate { generator.Cancel(); };

            var files = generator.DryRun();

            files.Should().HaveCount(0);
        }

        [Fact]
        public void CanGenerateAgainAfterCancel()
        {
            var generator = new CodeGenerator(
                new IPreProcessor[] {new Pre1PreProcessor(new List<string>())},
                new IDataProvider[] {new Data_1_2_Provider()},
                new ICodeGenerator[] {new DataFile1CodeGenerator()},
                new IPostProcessor[] {new Processed1PostProcessor()}
            );

            void OnProgress(string title, string info, float progress) => generator.Cancel();

            generator.OnProgress += OnProgress;

            generator.Generate();

            generator.OnProgress -= OnProgress;

            var files = generator.Generate();

            files.Should().HaveCount(2);
        }

        [Fact]
        public void CanDoDryRunAgainAfterCancel()
        {
            var generator = new CodeGenerator(
                new IPreProcessor[] {new Pre1PreProcessor(new List<string>())},
                new IDataProvider[] {new Data_1_2_Provider()},
                new ICodeGenerator[] {new DataFile1CodeGenerator()},
                new IPostProcessor[] {new Processed1PostProcessor()}
            );

            void OnProgress(string title, string info, float progress) => generator.Cancel();

            generator.OnProgress += OnProgress;

            generator.Generate();

            generator.OnProgress -= OnProgress;

            var files = generator.DryRun();

            files.Should().HaveCount(2);
        }

        [Fact]
        public void RegistersObjectInSharedCache()
        {
            var generator = new CodeGenerator(
                new IPreProcessor[] {new Pre1PreProcessor(new List<string>())},
                new IDataProvider[] {new CachableProvider(), new CachableProvider()},
                new ICodeGenerator[] {new DataFile1CodeGenerator()},
                new IPostProcessor[] {new Processed1PostProcessor()}
            );

            var files = generator.Generate();
            files.Should().HaveCount(2);
            files[0].FileContent.Should().Be(files[1].FileContent);
        }

        [Fact]
        public void ResetsCacheBeforeEachNewRun()
        {
            var generator = new CodeGenerator(
                new IPreProcessor[] {new Pre1PreProcessor(new List<string>())},
                new IDataProvider[] {new CachableProvider(), new CachableProvider()},
                new ICodeGenerator[] {new DataFile1CodeGenerator()},
                new IPostProcessor[] {new Processed1PostProcessor()}
            );

            var result1 = generator.Generate()[0].FileContent;
            var result2 = generator.Generate()[0].FileContent;
            result1.Should().NotBe(result2);
        }
    }
}

public class Data_1_2_Provider : IDataProvider
{
    public string Name => "";
    public int Order => 0;
    public bool RunInDryMode => true;

    public CodeGeneratorData[] GetData()
    {
        var data1 = new CodeGeneratorData();
        data1.Add("testKey", "data1");

        var data2 = new CodeGeneratorData();
        data2.Add("testKey", "data2");

        return new[]
        {
            data1,
            data2
        };
    }
}

public class Data_3_4_Provider : IDataProvider
{
    public string Name => "";
    public int Order => 5;
    public bool RunInDryMode => true;

    public CodeGeneratorData[] GetData()
    {
        var data1 = new CodeGeneratorData();
        data1.Add("testKey", "data3");

        var data2 = new CodeGeneratorData();
        data2.Add("testKey", "data4");

        return new[]
        {
            data1,
            data2
        };
    }
}

public class DisabledDataProvider : IDataProvider
{
    public string Name => "";
    public int Order => 5;
    public bool RunInDryMode => false;

    public CodeGeneratorData[] GetData()
    {
        var data1 = new CodeGeneratorData();
        data1.Add("testKey", "data5");

        var data2 = new CodeGeneratorData();
        data2.Add("testKey", "data6");

        return new[]
        {
            data1,
            data2
        };
    }
}

public class DataFile1CodeGenerator : ICodeGenerator
{
    public string Name => "";
    public int Order => 0;
    public bool RunInDryMode => true;

    public CodeGenFile[] Generate(CodeGeneratorData[] data)
    {
        return data
            .Select((d, i) => new CodeGenFile(
                $"Test1File{i}",
                d["testKey"].ToString(),
                "Test1CodeGenerator"
            )).ToArray();
    }
}

public class DataFile2CodeGenerator : ICodeGenerator
{
    public string Name => "";
    public int Order => 5;
    public bool RunInDryMode => true;

    public CodeGenFile[] Generate(CodeGeneratorData[] data)
    {
        return data
            .Select((d, i) => new CodeGenFile(
                $"Test2File{i}",
                d["testKey"].ToString(),
                "Test2CodeGenerator"
            )).ToArray();
    }
}

public class DisabledCodeGenerator : ICodeGenerator
{
    public string Name => "";
    public int Order => -5;
    public bool RunInDryMode => false;

    public CodeGenFile[] Generate(CodeGeneratorData[] data)
    {
        return data
            .Select((d, i) => new CodeGenFile(
                $"Test3File{i}",
                d["testKey"].ToString(),
                "DisabledCodeGenerator"
            )).ToArray();
    }
}

public class Processed1PostProcessor : IPostProcessor
{
    public string Name => "";
    public int Order => 0;
    public bool RunInDryMode => true;

    public CodeGenFile[] PostProcess(CodeGenFile[] files)
    {
        foreach (var file in files)
        {
            file.FileName += "-Processed1";
        }

        return files;
    }
}

public class Processed2PostProcessor : IPostProcessor
{
    public string Name => "";
    public int Order => 5;
    public bool RunInDryMode => true;

    public CodeGenFile[] PostProcess(CodeGenFile[] files)
    {
        foreach (var file in files)
        {
            file.FileName += "-Processed2";
        }

        return files;
    }
}

public class DisabledPostProcessor : IPostProcessor
{
    public string Name => "";
    public int Order => 5;
    public bool RunInDryMode => false;

    public CodeGenFile[] PostProcess(CodeGenFile[] files)
    {
        foreach (var file in files)
        {
            file.FileName += "-Disabled";
        }

        return files;
    }
}

public class NoFilesPostProcessor : IPostProcessor
{
    public string Name => "";
    public int Order => -5;
    public bool RunInDryMode => true;

    public CodeGenFile[] PostProcess(CodeGenFile[] files)
    {
        return new[] {files[0]};
    }
}

public class CachableProvider : IDataProvider, ICachable
{
    public string Name => "";
    public int Order => 0;
    public bool RunInDryMode => true;

    public Dictionary<string, object> ObjectCache { get; set; }

    public CodeGeneratorData[] GetData()
    {
        object o;
        if (!ObjectCache.TryGetValue("myObject", out o))
        {
            o = new object();
            ObjectCache.Add("myObject", o);
        }

        var data = new CodeGeneratorData();
        data.Add("testKey", o.GetHashCode());
        return new[] {data};
    }
}

public class Pre1PreProcessor : IPreProcessor
{
    public string Name => "";
    public int Order => 0;
    public bool RunInDryMode => true;

    List<string> _strings;

    public Pre1PreProcessor(List<string> strings)
    {
        _strings = strings;
    }

    public void PreProcess()
    {
        _strings.Add("Pre1");
    }
}

public class Pre2PreProcessor : IPreProcessor
{
    public string Name => "";
    public int Order => 5;
    public bool RunInDryMode => true;

    List<string> _strings;

    public Pre2PreProcessor(List<string> strings)
    {
        _strings = strings;
    }

    public void PreProcess()
    {
        _strings.Add("Pre2");
    }
}

public class DisabledPreProcessor : IPreProcessor
{
    public string Name => "";
    public int Order => 0;
    public bool RunInDryMode => false;

    List<string> _strings;

    public DisabledPreProcessor(List<string> strings)
    {
        _strings = strings;
    }

    public void PreProcess()
    {
        _strings.Add("DisabledPre");
    }
}
