using System;
using System.Collections.Generic;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;

namespace Shared
{
    public static class ExtensionMethods
    {
        public static async Task SendAsync(this NetworkStream stream, byte[] data)
        {
            byte[] lengthBuffer = BitConverter.GetBytes(data.Length);
            byte[] totalPacket = new byte[lengthBuffer.Length + data.Length];
            lengthBuffer.CopyTo(totalPacket, 0);
            data.CopyTo(totalPacket, lengthBuffer.Length);
            await stream.WriteAsync(totalPacket, 0, totalPacket.Length);
        }
    
    }
}
