using Newtonsoft.Json;
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
            string data = JsonConvert.SerializeObject(packet.ToJson());
            await stream.SendAsync(Encoding.UTF8.GetBytes(data));
        }

        public async Task<IPacket> ReceivePacket()
        {
            NetworkStream stream = client.GetStream();
            byte[] buffer = await stream.ReceiveAsync();

            if (buffer.Length > 0)
            {
                string data = Encoding.UTF8.GetString(buffer);
                dynamic jsonData = JsonConvert.DeserializeObject(data);

                IPacket packet = HandlePacket(jsonData);

                return packet;
            }
            else
                return null;
        }

        public async Task ReceiveFileAsync()
        {
            NetworkStream stream = client.GetStream();
            
        }

        
        private static IPacket HandlePacket(dynamic jsonData)
        {
            string packetType = jsonData.packetType;
            switch (packetType)
            {
                case nameof(FileInfoResponse):
                    return FileInfoResponse.ToClass(jsonData);
                default:
                    return null;
            }
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
