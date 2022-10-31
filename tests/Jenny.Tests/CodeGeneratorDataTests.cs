using FluentAssertions;
using Xunit;

namespace Jenny.Tests
{
    public class CodeGeneratorDataTests
    {
        readonly CodeGeneratorData _data;

        public CodeGeneratorDataTests()
        {
            _data = new CodeGeneratorData
            {
                ["TestKey1"] = "TestValue1",
                ["testKey2"] = "testValue2",
                ["testKey3"] = new[] {"testValue1", "testValue2", "testValue3"}
            };
        }

        [Fact]
        public void AddsKeyAndValue()
        {
            _data["TestKey1"].Should().Be("TestValue1");
            _data["testKey2"].Should().Be("testValue2");
        }

        [Fact]
        public void ReplacesPlaceholders()
        {
            _data.ReplacePlaceholders("Test, ${TestKey1}, test, ${testKey2}")
                .Should().Be("Test, TestValue1, test, testValue2");
        }

        [Fact]
        public void ReplacesWithLower()
        {
            _data.ReplacePlaceholders("Test, ${TestKey1:lower}, test, ${testKey2:lower}")
                .Should().Be("Test, testvalue1, test, testvalue2");
        }

        [Fact]
        public void ReplacesWithUpper()
        {
            _data.ReplacePlaceholders("Test, ${TestKey1:upper}, test, ${testKey2:upper}")
                .Should().Be("Test, TESTVALUE1, test, TESTVALUE2");
        }

        [Fact]
        public void ReplacesWithLowerFirst()
        {
            _data.ReplacePlaceholders("Test, ${TestKey1:lowerFirst}, test, ${testKey2:lowerFirst}")
                .Should().Be("Test, testValue1, test, testValue2");
        }

        [Fact]
        public void ReplacesWithUpperFirst()
        {
            _data.ReplacePlaceholders("Test, ${TestKey1:upperFirst}, test, ${testKey2:upperFirst}")
                .Should().Be("Test, TestValue1, test, TestValue2");
        }

        [Fact]
        public void ReplacesWithTemplate()
        {
            _data.ReplacePlaceholders("Test, ${testKey3:foreach:var $item = value;\\n}")
                .Should().Be(@"Test, var testValue1 = value;
var testValue2 = value;
var testValue3 = value;
");
        }

        [Fact]
        public void ClonesData()
        {
            var clone = new CodeGeneratorData(_data);
            clone.Should().HaveCount(_data.Count);
            clone["TestKey1"].Should().Be("TestValue1");
            clone["testKey2"].Should().Be("testValue2");
        }
    }
}
