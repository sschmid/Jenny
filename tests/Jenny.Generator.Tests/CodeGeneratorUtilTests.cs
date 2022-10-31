using System.IO;
using DesperateDevs.Serialization;
using Jenny.Tests;
using FluentAssertions;
using Xunit;

namespace Jenny.Generator.Tests
{
    public class CodeGeneratorUtilTests
    {
        static readonly string ProjectRoot = TestHelper.GetProjectRoot();
        static readonly string FixturesPath = Path.Combine(ProjectRoot, "tests", "Jenny.Generator.Tests", "fixtures");
        static readonly string TempPath = Path.Combine(FixturesPath, "temp");
        static readonly string SearchPaths = Path.Combine(ProjectRoot, "tests", "Jenny.Generator.Tests.Fixture", "bin", "Release");

        readonly CodeGeneratorConfig _config;

        public CodeGeneratorUtilTests()
        {
            _config = new CodeGeneratorConfig();
            var properties = new Properties();
            properties.AddProperties(_config.DefaultProperties, true);
            var preferences = new TestPreferences(properties);
            _config.Configure(preferences);
        }

        [Fact(Skip = "Dispose AssemblyResolver")]
        public void UpdatesSearchPathsInCodeGeneratorConfig()
        {
            CodeGeneratorUtil.AutoImport(_config, SearchPaths);
            _config.SearchPaths.Should().HaveCount(1);
            _config.SearchPaths[0].Should().Be($"{SearchPaths}");
        }

        [Fact(Skip = "Dispose AssemblyResolver")]
        public void UpdatesSearchPathsWhenPathContainsSpaces()
        {
            var tempWithSpaces = Path.Combine(TempPath, "Test Plugins");
            if (!Directory.Exists(tempWithSpaces)) Directory.CreateDirectory(tempWithSpaces);
            foreach (var file in Directory.GetFiles(SearchPaths, "*"))
                File.Copy(file, Path.Combine(tempWithSpaces, Path.GetFileName(file)), true);

            CodeGeneratorUtil.AutoImport(_config, tempWithSpaces);
            _config.SearchPaths.Should().HaveCount(1);
            _config.SearchPaths[0].Should().Be($"{tempWithSpaces}");
        }
    }
}
