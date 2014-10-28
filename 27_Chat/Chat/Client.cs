using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Threading;
using System.Net;
using System.Net.Sockets;
using System.IO;
using Timers = System.Timers;

namespace Chat {
    class Client : ClientAbstract {

        public virtual string[] SupportedVersions { get { return new string[] { "1.0" }; } }

        TcpClient connection;

        Action closeCallback;
        Action<string, string> rcvMessageCallback;

        public Client(Action closeCallback, Action<string, string> rcvMessageCallback)
        {
            this.rcvMessageCallback = rcvMessageCallback;
            this.closeCallback = closeCallback;
        }

        public virtual async Task<bool> Connect(String ip)
        {
            try
            {
                IPEndPoint ipAddress = new IPEndPoint(IPAddress.Parse(ip), Server.Port);

                connection = new TcpClient(AddressFamily.InterNetworkV6);
                connection.Client.DualMode = true;
                connection.Connect(ipAddress);

                stream = connection.GetStream();
                createStreams(stream);

                return await ClientHandshake();
            }
            catch (SocketException)
            {
                return false;
            }
        }

        public override ClientAbstract CloseConnection()
        {
            closeCallback();

            return base.CloseConnection();
        }

        public async Task<bool> ClientHandshake()
        {
            await Send(String.Format("HELLO {0} {1}", Server.ProtocolName, String.Join(" ", SupportedVersions)));

            var response = await Reader.ReadLineAsync();

            var responseSplitted = response.Split(new char[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);

            if (responseSplitted[0] != "OLLEH")
            {
                SendError("Bad hand-shake format [served didnt response OLLEH]").CloseConnection();
                return false;
            }

            if (responseSplitted[1] != Server.ProtocolName)
            {
                SendError("Bad hand-shake format [unrecognized protocol]").CloseConnection();
                return false;
            }

            if (responseSplitted.Length != 3)
            {
                SendError("Bad hand-shake format [wrong number of parameters]").CloseConnection();
                return false;
            }

            AcceptedVersion = responseSplitted[2];

            await Send("ACK");

            return true;
        }

        public override ClientAbstract SendError(string msg)
        {
            rcvMessageCallback("Client", msg);
            return base.SendError(msg);
        }

        public async Task ListenForMessages(Action errorCallback)
        {
            await Task.Yield();

            try
            {
                while (!IsClosing)
                    if (!handleMessage(await Reader.ReadLineAsync()))
                        break;
            }
            catch (IOException)
            {
                rcvMessageCallback("Server", "closed connection");
                errorCallback();
            }
        }

        protected virtual bool handleMessage(string message)
        {
            var msgSplitted = message.Split(new char[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);

            if (msgSplitted[0] == "ERROR")
            {
                rcvMessageCallback("Server", String.Join(" ", msgSplitted.Skip(1).ToArray()));
                CloseConnection();
                return false;
            }

            if (msgSplitted[0] != "MSG")
            {
                SendError("Expecting MSG with 2 parameters [no MSG received]").CloseConnection();
                return false;
            }

            if (msgSplitted.Length < 2)
            {
                SendError("Expecting MSG with 2 parameters").CloseConnection();
                return false;
            }

            rcvMessageCallback(msgSplitted[1], String.Join(" ", msgSplitted.Skip(2).ToArray()));

            return true;
        }
    }
}
