using System;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using TCPeasy;
using DesperateDevs.Serialization;
using DesperateDevs.Serialization.Cli.Utils;

namespace Jenny.Generator.Cli
{
    public class ClientCommand : AbstractPreferencesCommand
    {
        public override string Trigger => "client";
        public override string Description => "Start client mode";
        public override string Group => CommandGroups.CodeGeneration;
        public override string Example => "client [command]";

        string _command;

        public ClientCommand() : base(typeof(ClientCommand).FullName) { }

        protected override void Run()
        {
            _command = string.Join(" ", _rawArgs.Skip(1).ToArray());

            var config = _preferences.CreateAndConfigure<CodeGeneratorConfig>();
            var client = new TcpClientSocket();
            client.OnConnected += OnConnected;
            client.OnReceived += OnReceived;
            client.OnDisconnected += OnDisconnected;
            client.Connect(config.Host, config.Port);

            while (true) { }
        }

        void OnConnected(TcpClientSocket client)
        {
            client.Send(Encoding.UTF8.GetBytes(_command));
        }

        void OnReceived(AbstractTcpSocket socket, Socket client, byte[] bytes)
        {
            _logger.Info(Encoding.UTF8.GetString(bytes));
            socket.Disconnect();
        }

        void OnDisconnected(AbstractTcpSocket socket)
        {
            Environment.Exit(0);
        }
    }
}
