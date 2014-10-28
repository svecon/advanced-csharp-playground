using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Timers = System.Timers;

namespace Chat {
    class ServerPingable : Server {

        public override string[] SupportedVersions { get { return new string[] { "1.1", "1.0" }; } }

        public override void Start()
        {
            base.Start();

            var timeoutTimer = new Timers.Timer(3000);
            timeoutTimer.Elapsed += timeoutInactiveClients;
            timeoutTimer.Enabled = true;
        }

        private void timeoutInactiveClients(object x, Timers.ElapsedEventArgs arg)
        {
            foreach (var client in clientsPerVersion["1.1"])
            {
                //if (client.LastCommunication.AddSeconds(2) < DateTime.Now)
                if (client.LastCommunication.AddMinutes(3) < DateTime.Now)
                {
                    client.SendError("connection close [timeout].").CloseConnection();
                }
            }
        }
    }
}
