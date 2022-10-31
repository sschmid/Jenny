using DesperateDevs.Serialization;
using DesperateDevs.Unity.Editor;
using UnityEditor;
using UnityEngine;

namespace Jenny.Generator.Unity.Editor
{
    public class CodeGeneratorPreferencesWindow : PreferencesWindow
    {
        [MenuItem(CodeGeneratorMenuItems.Preferences, false, CodeGeneratorMenuItemPriorities.Preferences)]
        public static void OpenPreferences()
        {
            var window = GetWindow<CodeGeneratorPreferencesWindow>(true, "Jenny");
            window.minSize = new Vector2(415f, 366f);
            window.Initialize(
                EditorPrefs.GetString(CodeGeneratorPreferencesDrawer.PropertiesPathKey, CodeGenerator.DefaultPropertiesPath),
                Preferences.DefaultUserPropertiesPath,
                false,
                false,
                typeof(CodeGeneratorPreferencesDrawer).FullName
            );

            window.Show();
        }
    }
}
