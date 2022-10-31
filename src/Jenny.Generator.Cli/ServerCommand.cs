using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using DesperateDevs.Cli.Utils;
using DesperateDevs.Serialization;
using DesperateDevs.Serialization.Cli.Utils;
using Sherlog;
using TCPeasy;

namespace Jenny.Generator.Cli
{
    public class ServerCommand : AbstractPreferencesCommand
    {
        public override string Trigger => "server";
        public override string Description => "Start server mode";
        public override string Group => CommandGroups.CodeGeneration;
        public override string Example => "server";

        AbstractTcpSocket _socket;
        readonly List<string> _logBuffer = new List<string>();

        public ServerCommand() : base(typeof(ServerCommand).FullName) { }

        protected override void Run()
        {
            var config = _preferences.CreateAndConfigure<CodeGeneratorConfig>();
            var server = new TcpServerSocket();
            _socket = server;
            server.OnReceived += OnReceived;
            server.Listen(config.Port);
            Console.CancelKeyPress += OnCancel;
            while (true)
                server.Send(Encoding.UTF8.GetBytes(Console.ReadLine() ?? string.Empty));
        }

        void OnReceived(AbstractTcpSocket socket, Socket client, byte[] bytes)
        {
            var message = Encoding.UTF8.GetString(bytes);
            _logger.Info(message);

            var args = GetArgsFromMessage(message);

            try
            {
                if (args[0] == Trigger)
                {
                    throw new Exception("Server is already running!");
                }

                var command = _program.GetCommand(args.WithoutDefaultParameter().First());
                Logger.AddAppender(OnLog);
                command.Run(_program, args);
                Logger.RemoveAppender(OnLog);
                var logBufferString = GetLogBufferString();
                var sendBytes = logBufferString.Length == 0
                    ? new byte[] {0}
                    : Encoding.UTF8.GetBytes(logBufferString);
                socket.Send(sendBytes);
            }
            catch (Exception exception)
            {
                _logger.Error(args.IsVerbose()
                    ? exception.ToString()
                    : exception.Message);

                socket.Send(Encoding.UTF8.GetBytes(GetLogBufferString() + exception.Message));
            }

            _logBuffer.Clear();
        }

        string[] GetArgsFromMessage(string command) => command
            .Split(new[] {' '}, StringSplitOptions.RemoveEmptyEntries)
            .Select(value => value.Trim())
            .ToArray();

        void OnCancel(object sender, ConsoleCancelEventArgs e) => _socket.Disconnect();

        string GetLogBufferString() => string.Join("\n", _logBuffer);

        void OnLog(Logger logger, LogLevel loglevel, string message) => _logBuffer.Add(message);
    }
}
