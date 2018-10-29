using System;
using System.Collections.Generic;
using System.Text;

namespace Shared.Packets
{
    class DownloadID : IPacket
    {
        public string ID { get; private set; }
        public DownloadID(string id) => ID = id;
        public static IPacket ToClass(dynamic json)
        {
            string id = (string)json.data.id;

            return new DownloadID(id);
        }
        IPacket IPacket.ToClass(dynamic json)
        {
            return DownloadID.ToClass(json);
        }
        public dynamic ToJson()
        {
            return new
            {
                packetType = nameof(DownloadID),
                data = new
                {
                    id = ID
                }
            };
        }
    }
}
