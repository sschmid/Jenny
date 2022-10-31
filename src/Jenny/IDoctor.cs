namespace Jenny
{
    public interface IDoctor : ICodeGenerationPlugin
    {
        Diagnosis Diagnose();
        bool ApplyFix();
    }
}
