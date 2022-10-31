using System.IO;
using Jenny.Tests;
using FluentAssertions;
using Xunit;

namespace Jenny.Plugins.Tests
{
    public class TargetFrameworkProfilePreProcessorTests
    {
        static readonly string ProjectRoot = TestHelper.GetProjectRoot();
        static readonly string FixturesPath = Path.Combine(ProjectRoot, "tests", "Jenny.Plugins.Tests", "fixtures");
        static readonly string TempPath = Path.Combine(FixturesPath, "temp");

        [Fact]
        public void RemovesUnitySubsetv35()
        {
            PreProcess("UnitySubsetv35.csproj").Should().NotContain("Unity Subset v3.5");
        }

        [Fact]
        public void RemovesUnityFullv35()
        {
            PreProcess("UnityFullv35.csproj").Should().NotContain("Unity Full v3.5");
        }

        string PreProcess(string csproj)
        {
            if (!Directory.Exists(TempPath)) Directory.CreateDirectory(TempPath);
            var project = Path.Combine(FixturesPath, csproj);
            var tempProject = Path.Combine(TempPath, csproj);
            File.Copy(project, tempProject, true);

            var preProcessor = new TargetFrameworkProfilePreProcessor();
            var preferences = new TestPreferences($"Jenny.Plugins.ProjectPath = {tempProject}");
            preProcessor.Configure(preferences);
            preProcessor.PreProcess();
            return File.ReadAllText(tempProject);
        }
    }
}
