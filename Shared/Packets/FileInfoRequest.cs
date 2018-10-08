using System;
using System.Collections.Generic;
using System.Text;

namespace Shared.Packets
{
    /*
     * {
     *  "packetType":"FileInfoRequest",
     *  "data" : {
     *      "id" : "zYsW2a"
     *  }
     * }
     */
    class FileInfoRequest : IPacket
    {
        public FileInfoRequest(string id)
        {
            ID = id;
        }

        public string ID { get; private set; }

        public static IPacket ToClass(dynamic json)
        {
            string id = (string)json.data.id;
            return new FileInfoRequest(id);
        }

        IPacket IPacket.ToClass(dynamic json)
        {
            return FileInfoRequest.ToClass(json);
        }

        public dynamic ToJson()
        {
            return new
            {
                packetType = nameof(FileInfoRequest),
                data = new
                {
                    id = ID
                }
            };
        }
    }
}
