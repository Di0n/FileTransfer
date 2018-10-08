using Shared;
using Shared.Packets;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;

namespace Client
{
    class Connection : IDisposable
    {
        private TcpClient client;

        public Connection()
        {
            
        }

        public async Task SendPacket(IPacket packet)
        {
            NetworkStream stream = client.GetStream();
            //stream.WriteAsync();
        }

        public void Receive()
        {

        }

        public async Task Connect(string ip, ushort port)
        {
            await client.ConnectAsync(ip, port);
        }

        public void Close()
        {
            if (client != null)
                client.Close();
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        private void Dispose(bool disposing)
        {
            if (disposing)
            {
                if (client != null)
                    client.Close();
            }
        }
    }
}
