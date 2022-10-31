using System;
using DesperateDevs.Cli.Utils;

namespace Jenny.Generator.Cli
{
    public class HelpCommand : AbstractCommand
    {
        public override string Trigger => "help";
        public override string Description => "Show help";
        public override string Group => null;
        public override string Example => "help";

        protected override void Run()
        {
            var pad = _program.GetCommandListPad();
            Console.WriteLine($@"usage:
{_program.GetFormattedCommandList()}
{"[-v]".PadRight(pad)}   - verbose output
{"[-s]".PadRight(pad)}   - silent (minimal output)

Menus

  Down, Right, j, l          - Select next
  Up, Left, k, h             - Select previous
  Home, a                    - Select first
  End, e                     - Select last
  Enter, Space               - Run selected menu entry

Jenny automatically uses {CodeGenerator.DefaultPropertiesPath} and <userName>.userproperties
when no properties files are specified along with the command.

EXAMPLE
  jenny new Jenny.properties
  jenny auto-import -s
  jenny doctor
  jenny edit
  jenny fix
  jenny gen
  jenny gen Other.properties
  jenny gen Other.properties Other.userproperties
");
        }
    }
}
