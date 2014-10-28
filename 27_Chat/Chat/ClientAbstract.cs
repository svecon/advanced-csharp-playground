using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using System.Net.Sockets;

namespace Chat {
    abstract class ClientAbstract {

        public StreamReader Reader;
        public StreamWriter Writer;

        public bool IsClosing = false;

        protected NetworkStream stream;

        public string AcceptedVersion;
        public DateTime LastCommunication;

        public ClientAbstract() { }

        public ClientAbstract(NetworkStream stream)
        {
            this.stream = stream;

            createStreams(stream);
        }

        protected void createStreams(NetworkStream stream)
        {
            Reader = new StreamReader(stream);
            Writer = new StreamWriter(stream, Encoding.UTF8) { AutoFlush = true };
        }

        public virtual ClientAbstract SendError(string msg)
        {
            Send("ERROR " + msg);

            return this;
        }

        public virtual ClientAbstract CloseConnection()
        {
            IsClosing = true;

            if (stream != null)
                stream.Dispose();

            return this;
        }

        public ClientAbstract SendMessage(String msg, String username)
        {
            Send("MSG " + username + " " + msg);

            return this;
        }

        protected async Task Send(string message)
        {
            LastCommunication = DateTime.Now;

            await Writer.WriteLineAsync(message);
        }
    }
}
