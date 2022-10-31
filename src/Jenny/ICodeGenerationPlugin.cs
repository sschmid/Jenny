namespace Jenny
{
    public interface ICodeGenerationPlugin
    {
        string Name { get; }
        int Order { get; }
        bool RunInDryMode { get; }
    }
}
