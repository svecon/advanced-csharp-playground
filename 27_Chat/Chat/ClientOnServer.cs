using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Net.Sockets;

namespace Chat {
    class ClientOnServer : ClientAbstract {

        public ClientOnServer(NetworkStream stream) : base(stream) { }

        public async Task<bool> ServerHandshake(string protocol, string[] supportedVersions)
        {

            var hello = await Reader.ReadLineAsync();
            var helloSplitted = hello.Split(new char[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);

            if (helloSplitted[0] != "HELLO")
                SendError("Bad hand-shake format [expecting HELLO]").CloseConnection();

            if (helloSplitted[1] != protocol)
                SendError("Bad hand-shake format [unknown protocol = " + helloSplitted[1] + "]").CloseConnection();

            if (helloSplitted.Length < 3)
                SendError("Bad hand-shake format [specify at least 3 parameters]").CloseConnection();

            try
            {
                AcceptedVersion = getBestVersion(helloSplitted.Skip(2).ToArray(), supportedVersions);
            }
            catch (ArgumentException)
            {
                SendError("Bad hand-shake format [unsupported version]").CloseConnection();
            }

            await Send(String.Format("OLLEH {0} {1}", helloSplitted[1], AcceptedVersion));

            var ack = await Reader.ReadLineAsync();

            if (ack != "ACK")
                SendError("Bad hand-shake format [client didnt ACKnowledge]").CloseConnection();

            return true;
        }

        string getBestVersion(string[] versions, string[] supported)
        {

            foreach (var v in versions)
                if (supported.Contains(v))
                    return v;

            throw new ArgumentException("Unsupported version");
        }

        public async Task ListenForMessages(Action<string> callbackMessage)
        {

            while (!IsClosing)
            {
                try
                {
                    var message = await Reader.ReadLineAsync();
                    LastCommunication = DateTime.Now;

                    if (!handleMessage(message))
                        continue;

                    callbackMessage(message);
                }
                catch (System.IO.IOException)
                {
                    CloseConnection();
                }
            }

        }

        protected virtual bool handleMessage(string message)
        {
            var msgSplitted = message.Split(new char[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);

            if (msgSplitted[0] == "PING")
            {
                Send("PONG");
                return false;
            }

            if (msgSplitted[0] != "MSG")
            {
                SendError("Expecting MSG with 2 parameters [no MSG]").CloseConnection();
                return false;
            }

            if (msgSplitted.Length < 2)
            {
                SendError("Expecting MSG with 2 parameters").CloseConnection();
                return false;
            }

            return true;
        }

    }
}
