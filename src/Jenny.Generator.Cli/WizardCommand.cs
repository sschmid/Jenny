using System;
using System.IO;
using DesperateDevs.Cli.Utils;
using DesperateDevs.Serialization;
using Sherlog;

namespace Jenny.Generator.Cli
{
    public class WizardCommand : AbstractCommand
    {
        public override string Trigger => "wiz";
        public override string Description => "Setup Jenny, guided by a wizard";
        public override string Group => null;
        public override string Example => "wiz";

        readonly Logger _logger = Logger.GetLogger(typeof(WizardCommand));

        protected override void Run()
        {
            const string title = "";
//            const string title = @"
//     gg
//    dP8,
//   dP Yb
//  ,8  `8,
//  I8   Yb
//  `8b, `8,    ,ggg,    ,ggg,,ggg,    ,ggg,,ggg,   gg     gg
//   `'Y88888  i8' '8i  ,8' '8P' '8,  ,8' '8P' '8,  I8     8I
//       'Y8   I8, ,8I  I8   8I   8I  I8   8I   8I  I8,   ,8I
//        ,88, `YbadP' ,dP   8I   Yb,,dP   8I   Yb,,d8b, ,d8I
//    ,ad88888888P'Y8888P'   8I   `Y88P'   8I   `Y8P''Y88P'888
//  ,dP''   Yb                                           ,d8I'
// ,8'      I8                                         ,dP'8I
//,8'       I8                                        ,8'  8I
//I8,      ,8'    A lovely .NET Code Generator        I8   8I
//`Y8,___,d8'                                         `8, ,8I
//  'Y888P'                                            `Y8P'
//";

            const string indent = "→ ";

            // Step 1: Properties
            var allPreferences = Directory.GetFiles(Directory.GetCurrentDirectory(), "*.properties", SearchOption.TopDirectoryOnly);
            var propertiesMenu = new Step1PropertiesMenu(_program, title, allPreferences)
            {
                Indent = indent
            };
            propertiesMenu.Start();

            var preferences = new Preferences(propertiesMenu.Properties, Preferences.DefaultUserPropertiesPath);

            // Step 2: Plugins
            var pluginsMenu = new Step2PluginsMenu(_program, title, preferences, _rawArgs.IsVerbose())
            {
                Indent = indent
            };
            pluginsMenu.Start();

            var fixArgs = pluginsMenu.ShouldAutoImport
                ? "-s"
                : string.Empty;

            var fixCommand = new FixCommand();
            fixCommand.Run(_program, new[] {fixCommand.Trigger, preferences.PropertiesPath, fixArgs});

            Console.Clear();

            var doctorCommand = new DoctorCommand();
            doctorCommand.Run(_program, new[] {doctorCommand.Trigger, preferences.PropertiesPath});
        }
    }
}
