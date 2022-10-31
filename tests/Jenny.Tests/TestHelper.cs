using System.IO;

namespace Jenny.Tests
{
    public static class TestHelper
    {
        public static string GetProjectRoot()
        {
            var current = new DirectoryInfo(Directory.GetCurrentDirectory());
            while (!File.Exists(Path.Combine(current.FullName, "Jenny.sln"))) current = current.Parent;
            return current.FullName;
        }
    }
}
