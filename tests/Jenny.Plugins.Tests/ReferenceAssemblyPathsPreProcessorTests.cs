using System.IO;
using Jenny.Tests;
using FluentAssertions;
using Xunit;

namespace Jenny.Plugins.Tests
{
    public class ReferenceAssemblyPathsPreProcessorTests
    {
        static readonly string ProjectRoot = TestHelper.GetProjectRoot();
        static readonly string FixturesPath = Path.Combine(ProjectRoot, "tests", "Jenny.Plugins.Tests", "fixtures");
        static readonly string TempPath = Path.Combine(FixturesPath, "temp");

        [Fact]
        public void AddsMissingProperties()
        {
            var result = PreProcess("NoReferenceAssemblyPaths.csproj");
            result.Should().Contain($"<_TargetFrameworkDirectories>{typeof(ReferenceAssemblyPathsPreProcessor).FullName}</_TargetFrameworkDirectories>");
            result.Should().Contain($"<_FullFrameworkReferenceAssemblyPaths>{typeof(ReferenceAssemblyPathsPreProcessor).FullName}</_FullFrameworkReferenceAssemblyPaths>");
            result.Should().Contain("<DisableHandlePackageFileConflicts>true</DisableHandlePackageFileConflicts>");
        }

        [Fact]
        public void UpdatesExistingProperties()
        {
            var result = PreProcess("HasReferenceAssemblyPaths.csproj");
            result.Should().Contain($"<_TargetFrameworkDirectories>{typeof(ReferenceAssemblyPathsPreProcessor).FullName}</_TargetFrameworkDirectories>");
            result.Should().Contain($"<_FullFrameworkReferenceAssemblyPaths>{typeof(ReferenceAssemblyPathsPreProcessor).FullName}</_FullFrameworkReferenceAssemblyPaths>");
            result.Should().Contain("<DisableHandlePackageFileConflicts>true</DisableHandlePackageFileConflicts>");
        }

        string PreProcess(string csproj)
        {
            if (!Directory.Exists(TempPath)) Directory.CreateDirectory(TempPath);
            var project = Path.Combine(FixturesPath, csproj);
            var tempProject = Path.Combine(TempPath, csproj);
            File.Copy(project, tempProject, true);

            var preProcessor = new ReferenceAssemblyPathsPreProcessor();
            var preferences = new TestPreferences($"Jenny.Plugins.ProjectPath = {tempProject}");
            preProcessor.Configure(preferences);
            preProcessor.PreProcess();
            return File.ReadAllText(tempProject);
        }
    }
}
