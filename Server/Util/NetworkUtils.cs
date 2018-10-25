using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Server.Util
{
    class NetworkUtils
    {
        public static byte[] CreatePacket(byte[] bytes)
        {
            byte[] sizePrefix = BitConverter.GetBytes(bytes.Length);
            byte[] buffer = new byte[sizePrefix.Length + bytes.Length];
            sizePrefix.CopyTo(buffer, 0);
            bytes.CopyTo(buffer, sizePrefix.Length);

            return buffer;
        }

    }
}
