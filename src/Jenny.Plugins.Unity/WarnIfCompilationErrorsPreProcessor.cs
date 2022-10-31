using System;
using UnityEditor;

namespace Jenny.Plugins.Unity
{
    public class WarnIfCompilationErrorsPreProcessor : IPreProcessor
    {
        public string Name => "Warn If Compilation Errors";
        public int Order => -5;
        public bool RunInDryMode => true;

        public void PreProcess()
        {
            string errorMessage = null;
            if (EditorApplication.isCompiling)
            {
                errorMessage = "Cannot generate because Unity is still compiling. Please wait...";
            }

            var assembly = typeof(Editor).Assembly;

            var logEntries = assembly.GetType("UnityEditorInternal.LogEntries")
                             ?? assembly.GetType("UnityEditor.LogEntries");

            logEntries.GetMethod("Clear").Invoke(new object(), null);
            var canCompile = (int)logEntries.GetMethod("GetCount").Invoke(new object(), null) == 0;
            if (!canCompile)
                errorMessage = "There are compilation errors! Please fix all errors first.";

            if (errorMessage != null)
                throw new Exception($"{errorMessage}\n\nYou can disable this warning by removing '{Name}' from the Pre Processors.");
        }
    }
}
