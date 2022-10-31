namespace Jenny
{
    public interface IDataProvider : ICodeGenerationPlugin
    {
        CodeGeneratorData[] GetData();
    }
}
