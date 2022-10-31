using DesperateDevs.Cli.Utils;
using DesperateDevs.Serialization.Cli.Utils;

namespace Jenny.Generator.Cli
{
    public class EditMenuEntry : MenuEntry
    {
        public EditMenuEntry(CliProgram progam, CliMenu menu, string propertiesPath) :
            base($"Edit {propertiesPath}", null, false, () =>
            {
                var command = new EditConfigCommand();
                command.Run(progam, new[] {command.Trigger, propertiesPath});
                menu.Stop();
            }) { }
    }
}
