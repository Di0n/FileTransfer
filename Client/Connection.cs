using Client.Properties;
using Newtonsoft.Json;
using Shared;
using Shared.Packets;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;

namespace Client
{
    internal class Connection : IDisposable
    {
        private readonly TcpClient client;
        public delegate void FileTransferProgressHandler(object sender, ProgressEventArgs args);
        public event FileTransferProgressHandler FileTransferProgressChanged;
        public Connection()
        {
            client = new TcpClient();
        }

        public Task SendPacket(IPacket packet)
        {
            NetworkStream stream = client.GetStream();
            string data = JsonConvert.SerializeObject(packet.ToJson());
            return stream.SendAsync(Encoding.UTF8.GetBytes(data));
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

        public Task SendFileAsync(string path)
        {
            return Task.Factory.FromAsync(client.Client.BeginSendFile(path, SendFileCallback, null), SendFileCallback);
        }

        private void SendFileCallback(IAsyncResult ar)
        {
            client.Client.EndSendFile(ar);
        }

        public async Task ReceiveFileAsync(string path, long fileSize)
        {
            NetworkStream stream = client.GetStream();
            int bufferSize = Settings.Default.FileTransferBufferSize;
            using (FileStream fileStream = new FileStream(path, FileMode.Create, FileAccess.Write, FileShare.None, Settings.Default.FileWriteBufferSize, true))
            {

                //long fileSize = BitConverter.ToInt64(sizeInfo, 0);
                int currentRead = 0;
                byte[] data = new byte[bufferSize];

                long fileRead = 0;
                do
                {
                    long remainingBytes = fileSize - fileRead;
                    fileRead += currentRead = await stream.ReadAsync(data, 0, remainingBytes < bufferSize ? (int)remainingBytes : bufferSize);

                    Task writeTask = fileStream.WriteAsync(data, 0, currentRead);
                    await InvokeFileProgressChanged(new ProgressEventArgs(fileRead, fileSize, 0));
                    await writeTask;
                } while (fileRead < fileSize && currentRead > 0);
            }
            //byte[] sizeInfo = new byte[sizeof(Int64)];

            //int sizeRead = 0;
            //int currentRead = 0;

            //do
            //{
            //    currentRead = await stream.ReadAsync(sizeInfo, sizeRead, sizeInfo.Length - sizeRead);
            //    sizeRead += currentRead;
            //} while (sizeRead < sizeInfo.Length && currentRead > 0);


            ///* fileSize |= sizeInfo[0];
            // fileSize |= (((int)sizeInfo[1]) << 8);
            // fileSize |= (((int)sizeInfo[2]) << 16);
            // fileSize |= (((int)sizeInfo[3]) << 24);*/
            //int bufferSize = Settings.Default.FileBufferSize;
            //using (FileStream fileStream = new FileStream(fileName, FileMode.Create, FileAccess.Write, FileShare.None, bufferSize, true))
            //{

            //    long fileSize = BitConverter.ToInt64(sizeInfo, 0);

            //    byte[] data = new byte[bufferSize];

            //    long fileRead = 0;
            //    do
            //    {

            //        long remainingBytes = fileSize - fileRead;
            //        fileRead += currentRead = await stream.ReadAsync(data, 0, remainingBytes < bufferSize ? (int)remainingBytes : bufferSize);
            //        Task writeTask = fileStream.WriteAsync(data, 0, currentRead);
            //        await InvokeFileProgressChanged(new ProgressEventArgs(fileRead, fileSize, 0));
            //        await writeTask;
            //    } while (fileRead < fileSize && currentRead > 0);
            //}
        }

        private Task InvokeFileProgressChanged(ProgressEventArgs args) 
            => Task.Factory.FromAsync(FileTransferProgressChanged?.BeginInvoke(this, args, null, null), FileTransferProgressChanged.EndInvoke);

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

        public Task Connect(string ip, ushort port) => client.ConnectAsync(ip, port);

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
