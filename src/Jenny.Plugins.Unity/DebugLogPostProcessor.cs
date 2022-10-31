using System.Linq;
using UnityEngine;

namespace Jenny.Plugins.Unity
{
    public class DebugLogPostProcessor : IPostProcessor
    {
        public string Name => "Debug.Log generated files";
        public int Order => 200;
        public bool RunInDryMode => true;

        public CodeGenFile[] PostProcess(CodeGenFile[] files)
        {
            Debug.Log(files.Aggregate(
                string.Empty,
                (acc, file) => $"{acc}{file.FileName} - {file.GeneratorName}\n")
            );

            return files;
        }
    }
}
