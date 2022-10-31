namespace Jenny
{
    public enum DiagnosisSeverity
    {
        Healthy,
        Hint,
        Warning,
        Error
    }

    public class Diagnosis
    {
        public static Diagnosis Healthy => new Diagnosis(null, null, DiagnosisSeverity.Healthy);

        public readonly string Symptoms;
        public readonly string Treatment;
        public readonly DiagnosisSeverity Severity;

        public Diagnosis(string symptoms, string treatment, DiagnosisSeverity severity)
        {
            Symptoms = symptoms;
            Treatment = treatment;
            Severity = severity;
        }
    }
}
