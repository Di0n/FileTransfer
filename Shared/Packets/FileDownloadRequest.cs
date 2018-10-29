using System;
using System.Collections.Generic;
using System.Text;

namespace Shared.Packets
{
    /*
     * {
     *  "packetType":"FileDownloadRequest",
     *  "data" : {
     *      "id" : "zYsW2a"
     *  }
     * }
     */
    class FileDownloadRequest : IPacket
    {
        public string ID { get; private set; }
        public FileDownloadRequest(string id) => ID = id;
        public static IPacket ToClass(dynamic json)
        {
            string id = (string)json.data.id;

            return new FileDownloadRequest(id);
        }
        IPacket IPacket.ToClass(dynamic json)
        {
            return FileDownloadRequest.ToClass(json);
        }
        public dynamic ToJson()
        {
            return new
            {
                packetType = nameof(FileDownloadRequest),
                data = new
                {
                    id = ID
                }
            };
        }
    }
}
