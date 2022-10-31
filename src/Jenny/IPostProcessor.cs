namespace Jenny
{
    public interface IPostProcessor : ICodeGenerationPlugin
    {
        CodeGenFile[] PostProcess(CodeGenFile[] files);
    }
}
