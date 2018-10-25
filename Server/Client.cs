using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;

namespace Server
{
    class Client
    {
        public Socket Socket { get; private set; }
        public DateTime LastActivity { get; private set; }
        private Client(Socket socket)
        {
            this.Socket = socket;
        }

        public static implicit operator Client(Socket socket)
        {
            return new Client(socket);
        }

        /// <summary>
        /// Aanroepen wanneer er een interactie met de client gaande is.
        /// </summary>
        public void HeartBeat()
        {
            LastActivity = DateTime.Now;
        }
    }
}
