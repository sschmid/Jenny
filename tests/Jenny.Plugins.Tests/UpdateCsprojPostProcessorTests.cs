using System.IO;
using DesperateDevs.Extensions;
using Jenny.Tests;
using FluentAssertions;
using Xunit;

namespace Jenny.Plugins.Tests
{
    public class UpdateCsprojPostProcessorTests
    {
        static readonly string ProjectRoot = TestHelper.GetProjectRoot();
        static readonly string FixturesPath = Path.Combine(ProjectRoot, "tests", "Jenny.Plugins.Tests", "fixtures");
        static readonly string TempPath = Path.Combine(FixturesPath, "temp");

        [Fact]
        public void AddsGeneratedFilesToUnity2020_3()
        {
            var project = WriteAndUpdateTestCsproj("Unity-2020.3.csproj");
            project.Should().NotContain("Old");
            project.Should().Contain(@"  <ItemGroup>
    <Compile Include=""Assets/Sources/Generated/Test1/Test2/File1.cs"" />
    <Compile Include=""Assets/Sources/Generated/Test1/Test2/File2.cs"" />
  </ItemGroup>".ToUnixLineEndings());
        }

        [Fact]
        public void AddsGeneratedFilesToUnity2021_3()
        {
            var project = WriteAndUpdateTestCsproj("Unity-2021.3.csproj");
            project.Should().NotContain("Old");
            project.Should().Contain(@"  <ItemGroup>
    <Compile Include=""Assets/Sources/Generated/Test1/Test2/File1.cs"" />
    <Compile Include=""Assets/Sources/Generated/Test1/Test2/File2.cs"" />
  </ItemGroup>".ToUnixLineEndings());
        }

        string WriteAndUpdateTestCsproj(string csproj)
        {
            if (!Directory.Exists(TempPath)) Directory.CreateDirectory(TempPath);
            var project = Path.Combine(FixturesPath, csproj);
            var tempProject = Path.Combine(TempPath, csproj);
            File.Copy(project, tempProject, true);

            var postProcessor = new UpdateCsprojPostProcessor();
            var preferences = new TestPreferences($@"
Jenny.Plugins.ProjectPath = {tempProject}
Jenny.Plugins.TargetDirectory = Assets/Sources");
            postProcessor.Configure(preferences);

            var files = new[]
            {
                new CodeGenFile("Test1/Test2/File1.cs", "test file content 1", "TestGenerator1"),
                new CodeGenFile("Test1/Test2/File2.cs", "test file content 2", "TestGenerator2")
            };

            postProcessor.PostProcess(files);
            return File.ReadAllText(tempProject).ToUnixLineEndings();
        }
    }
}
