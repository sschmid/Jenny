using FluentAssertions;
using Xunit;

namespace Jenny.Plugins.Tests
{
    public class MergeFilesPostProcessorTests
    {
        [Fact]
        public void MergesFilesWithSameFilename()
        {
            var files = new[]
            {
                new CodeGenFile("file1", "content1", "gen1"),
                new CodeGenFile("file1", "content2", "gen2"),
                new CodeGenFile("file3", "content3", "gen3")
            };

            var postprocessor = new MergeFilesPostProcessor();
            files = postprocessor.PostProcess(files);

            files.Should().HaveCount(2);
            files[0].FileName.Should().Be("file1");
            files[1].FileName.Should().Be("file3");

            files[0].FileContent.Should().Be("content1\ncontent2");
            files[0].GeneratorName.Should().Be("gen1, gen2");
        }
    }
}
