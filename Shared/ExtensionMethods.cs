using System;
using System.Collections.Generic;
using System.IO;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;

namespace Shared
{
    public static class ExtensionMethods
    {
        public static Task SendAsync(this NetworkStream stream, byte[] data)
        {
            byte[] lengthBuffer = BitConverter.GetBytes(data.Length);
            byte[] totalPacket = new byte[lengthBuffer.Length + data.Length];
            lengthBuffer.CopyTo(totalPacket, 0);
            data.CopyTo(totalPacket, lengthBuffer.Length);
            return stream.WriteAsync(totalPacket, 0, totalPacket.Length);
        }

        public static async Task<byte[]> ReceiveAsync(this NetworkStream stream)
        {
            byte[] sizeInfo = new byte[sizeof(Int32)];

            int totalRead = 0, read = 0;

            do
            {
                read = await stream.ReadAsync(sizeInfo, totalRead, sizeInfo.Length - totalRead);
                totalRead += read;
            } while (totalRead < sizeInfo.Length && read > 0);

            int messageSize = 0;
            totalRead = 0;

            messageSize |= sizeInfo[0];
            messageSize |= (((int)sizeInfo[1]) << 8);
            messageSize |= (((int)sizeInfo[2]) << 16);
            messageSize |= (((int)sizeInfo[3]) << 24);

            if (messageSize > ushort.MaxValue) throw new InternalBufferOverflowException("Message exceeds ushort.Max");

            byte[] data = new byte[messageSize];

            do
            {
                totalRead += read = await stream.ReadAsync(data, totalRead, data.Length - totalRead);

            } while (totalRead < messageSize && read > 0);

            byte[] b = new byte[totalRead];
            Array.Copy(data, 0, b, 0, totalRead);
            return b;
            //return Encoding.UTF8.GetString(data, 0, totalRead);
        }

        public static async Task ReceiveFileAsync(this NetworkStream stream, string path, int bufferSize = 4096)
        {
            byte[] sizeInfo = new byte[sizeof(Int64)];

            int sizeRead = 0;
            int currentRead = 0;

            do
            {
                currentRead = await stream.ReadAsync(sizeInfo, sizeRead, sizeInfo.Length - sizeRead);
                sizeRead += currentRead;
            } while (sizeRead < sizeInfo.Length && currentRead > 0);


            /* fileSize |= sizeInfo[0];
             fileSize |= (((int)sizeInfo[1]) << 8);
             fileSize |= (((int)sizeInfo[2]) << 16);
             fileSize |= (((int)sizeInfo[3]) << 24);*/

            using (FileStream fileStream = new FileStream(path, FileMode.Create, FileAccess.Write, FileShare.None, bufferSize, true))
            {

                long fileSize = BitConverter.ToInt64(sizeInfo, 0);

                byte[] data = new byte[bufferSize];

                long fileRead = 0;
                do
                {
                    long remainingBytes = fileSize - fileRead;
                    fileRead += currentRead = await stream.ReadAsync(data, 0, remainingBytes < bufferSize ? (int)remainingBytes : bufferSize);

                    await fileStream.WriteAsync(data, 0, currentRead);
                } while (fileRead < fileSize && currentRead > 0);
            }
        }

        public static byte[] Combine(this byte[] bytes, byte[] b, int count)
        {
            byte[] data = new byte[bytes.Length + count];
            Buffer.BlockCopy(bytes, 0, data, 0, bytes.Length);
            Buffer.BlockCopy(b, 0, data, bytes.Length, count);
            return data;
}
    }
}
