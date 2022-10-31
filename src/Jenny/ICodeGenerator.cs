namespace Jenny
{
    public interface ICodeGenerator : ICodeGenerationPlugin
    {
        CodeGenFile[] Generate(CodeGeneratorData[] data);
    }
}
