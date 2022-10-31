using System;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using DesperateDevs.Serialization;
using TCPeasy;
using UnityEditor;
using UnityEngine;

namespace Jenny.Generator.Unity.Editor
{
    public static class UnityCodeGenerator
    {
        public static readonly string DryRunKey = $"{typeof(UnityCodeGenerator).Namespace}.DryRun";

        public static Preferences GetPreferences() => new Preferences(
            EditorPrefs.GetString(CodeGeneratorPreferencesDrawer.PropertiesPathKey, CodeGenerator.DefaultPropertiesPath),
            Preferences.DefaultUserPropertiesPath
        );

        [MenuItem(CodeGeneratorMenuItems.Generate, false, CodeGeneratorMenuItemPriorities.Generate)]
        public static void Generate()
        {
            Debug.Log("Generating...");

            var codeGenerator = CodeGeneratorUtil.CodeGeneratorFromPreferences(GetPreferences());

            var progressOffset = 0f;

            codeGenerator.OnProgress += (title, info, progress) =>
            {
                var cancel = EditorUtility.DisplayCancelableProgressBar(title, info, progressOffset + progress / 2);
                if (cancel) codeGenerator.Cancel();
            };

            CodeGenFile[] dryFiles = null;
            CodeGenFile[] files = null;

            try
            {
                dryFiles = EditorPrefs.GetBool(DryRunKey, true) ? codeGenerator.DryRun() : Array.Empty<CodeGenFile>();
                progressOffset = 0.5f;
                files = codeGenerator.Generate();
            }
            catch (Exception exception)
            {
                dryFiles = Array.Empty<CodeGenFile>();
                files = Array.Empty<CodeGenFile>();

                EditorUtility.DisplayDialog("Error", exception.Message, "Ok");
            }

            EditorUtility.ClearProgressBar();

            var totalGeneratedFiles = files.Select(file => file.FileName).Distinct().Count();
            var sloc = dryFiles.Sum(file => file.FileContent.Split(new[] {'\n'}, StringSplitOptions.RemoveEmptyEntries).Length);
            var loc = files.Sum(file => file.FileContent.Split('\n').Length);
            Debug.Log($"Generated {totalGeneratedFiles} files ({sloc} sloc, {loc} loc)");

            AssetDatabase.Refresh();
        }

        static string _propertiesPath;

        [MenuItem(CodeGeneratorMenuItems.GenerateServer, false, CodeGeneratorMenuItemPriorities.GenerateServer)]
        public static void GenerateExternal()
        {
            Debug.Log("Connecting...");

            var preferences = GetPreferences();
            _propertiesPath = preferences.PropertiesPath;
            var config = preferences.CreateAndConfigure<CodeGeneratorConfig>();
            var client = new TcpClientSocket();
            client.OnConnected += OnConnected;
            client.OnReceived += OnReceive;
            client.OnDisconnected += OnDisconnect;
            client.Connect(config.Host, config.Port);
        }

        static void OnConnected(TcpClientSocket client)
        {
            Debug.Log("Connected");
            Debug.Log("Generating...");
            client.Send(Encoding.UTF8.GetBytes($"gen {_propertiesPath}"));
        }

        static void OnReceive(AbstractTcpSocket socket, Socket client, byte[] bytes)
        {
            Debug.Log("Generated");
            socket.Disconnect();
        }

        static void OnDisconnect(AbstractTcpSocket socket)
        {
            Debug.Log("Disconnected");
        }
    }
}
