using FluentAssertions;
using Xunit;

namespace Jenny.Plugins.Tests
{
    public class TargetDirectoryExtensionTests
    {
        [Theory]
        [InlineData("Assets", "Assets/Generated")]
        [InlineData("Assets/", "Assets/Generated")]
        [InlineData("Assets/Generated", "Assets/Generated")]
        [InlineData("Assets/Generated/", "Assets/Generated")]
        [InlineData("/", "/Generated")]
        [InlineData("", "Generated")]
        [InlineData(".", "Generated")]
        public void ConvertsPathToSafePath(string path, string generatedFolder)
        {
            path.ToSafeDirectory().Should().Be(generatedFolder);
        }
    }
}
