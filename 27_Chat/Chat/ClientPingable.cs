using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Timers = System.Timers;

namespace Chat {
    class ClientPingable : Client {

        public ClientPingable(Action closeCallback, Action<string, string> rcvMessageCallback)
            : base(closeCallback, rcvMessageCallback) { }

        public override string[] SupportedVersions { get { return new string[] { "1.1", "1.0" }; } }

        public override async Task<bool> Connect(string ip)
        {
            var connected = await base.Connect(ip);

            if (AcceptedVersion == "1.1")
            {
                var timeoutTimer = new Timers.Timer(1 * 60 * 1000);
                timeoutTimer.Elapsed += (object o, Timers.ElapsedEventArgs args) => Send("PING");
                timeoutTimer.Enabled = true;
            }

            return connected;
        }

        protected override bool handleMessage(string message)
        {
            if (message == "PONG")
                return true;

            return base.handleMessage(message);
        }

    }
}
