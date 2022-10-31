using System;
using System.IO;
using System.Linq;
using DesperateDevs.Cli.Utils;
using DesperateDevs.Serialization.Cli.Utils;
using DesperateDevs.Extensions;

namespace Jenny.Generator.Cli
{
    public class Step1PropertiesMenu : CliMenu
    {
        public string Properties;

        public Step1PropertiesMenu(CliProgram program, string title, string[] properties) : base(BuildTitle(title, properties))
        {
            foreach (var p in properties)
                AddMenuEntry(new SelectPropertiesMenuEntry(this, p.MakePathRelativeTo(Directory.GetCurrentDirectory())));

            if (!properties.Any(p => p.EndsWith(CodeGenerator.DefaultPropertiesPath)))
                AddMenuEntry(new CreateDefaultPropertiesMenuEntry(program, this));

            AddMenuEntry(new CreateCustomPropertiesMenuEntry(program, this));
            AddMenuEntry(new ExitMenuEntry("Quit", false));
        }

        static string BuildTitle(string title, string[] properties)
        {
            var header = title + "\n" +
                         "Step 1: Properties File\n" +
                         "=======================\n\n" +
                         "Jenny saves all its configuration in a *.properties file.\n";

            if (properties.Length == 0)
            {
                return header +
                       "No properties file was found in " + Directory.GetCurrentDirectory() + "\n" +
                       "Let's create this file now!\n\n" +
                       "Please choose how you want to proceed and press enter";
            }
            else
            {
                return header +
                       "These properties files were found in " + Directory.GetCurrentDirectory() + "\n" +
                       string.Join("\n", properties.Select(p => "- " + p.MakePathRelativeTo(Directory.GetCurrentDirectory()))) + "\n\n" +
                       "Please choose how you want to proceed and press enter";
            }
        }
    }

    public class SelectPropertiesMenuEntry : MenuEntry
    {
        public SelectPropertiesMenuEntry(Step1PropertiesMenu menu, string properties) :
            base($"Use {properties}", null, false, () =>
            {
                menu.Properties = properties;
                menu.Stop();
            }) { }
    }

    public class CreateDefaultPropertiesMenuEntry : MenuEntry
    {
        public CreateDefaultPropertiesMenuEntry(CliProgram progam, Step1PropertiesMenu menu) :
            base($"Create new {CodeGenerator.DefaultPropertiesPath}", null, false, () =>
            {
                var command = new NewPreferencesCommand();
                command.Run(progam, new[] {command.Trigger, "-s", CodeGenerator.DefaultPropertiesPath});
                menu.Properties = CodeGenerator.DefaultPropertiesPath;
                menu.Stop();
            }) { }
    }

    public class CreateCustomPropertiesMenuEntry : MenuEntry
    {
        public CreateCustomPropertiesMenuEntry(CliProgram progam, Step1PropertiesMenu menu) :
            base("Create a new properties file with a custom name", null, false, () =>
            {
                Console.WriteLine("Please enter a file name");
                var fileName = Console.ReadLine();
                var command = new NewPreferencesCommand();
                command.Run(progam, new[] {command.Trigger, "-s", fileName});
                menu.Properties = fileName;
                menu.Stop();
            }) { }
    }
}
