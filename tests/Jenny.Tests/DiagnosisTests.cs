using FluentAssertions;
using Xunit;

namespace Jenny.Tests
{
    public class DiagnosisTests
    {
        [Fact]
        public void CreatesDiagnosis()
        {
            var diagnosis = new Diagnosis("test symptoms", "test treatment", DiagnosisSeverity.Error);
            diagnosis.Symptoms.Should().Be("test symptoms");
            diagnosis.Treatment.Should().Be("test treatment");
            diagnosis.Severity.Should().Be(DiagnosisSeverity.Error);
        }
        [Fact]
        public void CreatesHealthyDiagnosis()
        {
            var diagnosis = Diagnosis.Healthy;
            diagnosis.Symptoms.Should().BeNull();
            diagnosis.Treatment.Should().BeNull();
            diagnosis.Severity.Should().Be(DiagnosisSeverity.Healthy);
        }
    }
}
