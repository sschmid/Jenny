using FluentAssertions;
using Xunit;

namespace Jenny.Tests
{
    public class CodeGenFileTests
    {
        [Fact]
        public void SetFields()
        {
            var file = new CodeGenFile("name.cs", "content", "MyGenerator");
            file.FileName.Should().Be("name.cs");
            file.FileContent.Should().Be("content");
            file.GeneratorName.Should().Be("MyGenerator");
        }

        [Fact]
        public void ConvertsToUnixNewLines()
        {
            var file = new CodeGenFile(null, "line1\r\nline2\r", null);
            file.FileContent.Should().Be("line1\nline2\n");
        }

        [Fact]
        public void ConvertsToUnixNewLinesWhenSettingFileContent()
        {
            var file = new CodeGenFile(null, string.Empty, null);
            file.FileContent = "line1\r\nline2\r";
            file.FileContent.Should().Be("line1\nline2\n");
        }
    }
}
