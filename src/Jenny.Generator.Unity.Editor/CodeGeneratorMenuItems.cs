namespace Jenny.Generator.Unity.Editor
{
    public static class CodeGeneratorMenuItems
    {
        public const string Preferences = "Tools/Jenny/Preferences... #%j";
        public const string Generate = "Tools/Jenny/Generate #%g";
        public const string GenerateServer = "Tools/Jenny/Generate with Server %&g";
    }

    public static class CodeGeneratorMenuItemPriorities
    {
        public const int Preferences = 1;
        public const int Generate = 2;
        public const int GenerateServer = 3;
    }
}
