using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Threading;
using System.Net;
using System.Net.Sockets;
using System.IO;
using System.Collections.Concurrent;

namespace Chat {

    class Server {

        public const int Port = 4586;
        public const string ProtocolName = "Nprg038Chat";
        public virtual string[] SupportedVersions { get { return new string[] { "1.0" }; } }

        protected TcpListener tcpListener;

        protected Dictionary<string, List<ClientOnServer>> clientsPerVersion = new Dictionary<string, List<ClientOnServer>>();

        public virtual void Start()
        {
            tcpListener = TcpListener.Create(Port);
            tcpListener.Start();

            foreach (var version in SupportedVersions)
                clientsPerVersion.Add(version, new List<ClientOnServer>());

            StartListener();
        }

        private async Task StartListener()
        {
            while (true)
            {
                var tcpClient = await tcpListener.AcceptTcpClientAsync();

                HandleConnectionAsync(tcpClient);
            }
        }

        private async Task HandleConnectionAsync(TcpClient tcpClient)
        {

            using (var networkStream = tcpClient.GetStream())
            {
                var client = new ClientOnServer(networkStream);

                await client.ServerHandshake(ProtocolName, SupportedVersions);

                lock (clientsPerVersion)
                    clientsPerVersion[client.AcceptedVersion].Add(client);

                try
                {
                    await client.ListenForMessages((msg) =>
                    {
                        lock (clientsPerVersion)
                            foreach (var lists in clientsPerVersion.Values)
                                foreach (var c in lists)
                                    c.Writer.WriteLineAsync(msg);
                    });
                }
                catch (IOException) { }

                lock (clientsPerVersion)
                    clientsPerVersion[client.AcceptedVersion].Remove(client);

            }

        }

    }
}
