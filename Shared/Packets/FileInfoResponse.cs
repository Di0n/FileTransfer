using System;
using System.Collections.Generic;
using System.Text;

namespace Shared.Packets
{
    /*
     * {
     *  "packetType":"FileInfoResponse",
     *  "exists" : true
     *  "data" : {
     *      "id" : "zYsW2a",
     *      "name" : "Example.docx",
     *      "fileFormat" : "DOCX",
     *      "creationDate" : "37829498234",
     *      "fileSize" : "213124",
     *      "description" : "Blablabla" 
     *  }
     * }
     */
    class FileInfoResponse : IPacket
    {
        public NetworkFile File { get; private set; }
        public bool Exists { get; private set; }
        public FileInfoResponse() { }
        public FileInfoResponse(NetworkFile file)
        {
            Exists = true;
            File = file;
        }


        public static IPacket ToClass(dynamic json)
        {
            if (!((bool)json.exists))
                return new FileInfoResponse();

            string id = (string)json.data.id;
            string name = (string)json.data.name;
            string fileFormat = (string)json.data.fileFormat;
            DateTime creationDate = /*DateTime.FromFileTimeUtc*/DateTime.FromBinary((long)json.data.creationDate);
            long fileSize = (long)json.data.fileSize;
            string description = (string)json.data.description;

            return new FileInfoResponse(new NetworkFile(id, name, fileFormat,
                creationDate, fileSize, description));
        }

        IPacket IPacket.ToClass(dynamic json)
        {
            return FileInfoResponse.ToClass(json);
        }

        public dynamic ToJson()
        {
            if (Exists)
                return new
                    {
                        packetType = nameof(FileInfoResponse),
                        exists = Exists,
                        data = new
                        {
                            id = File.ID,
                            name = File.Name,
                            fileFormat = File.FileFormat,
                            creationDate = File.CreationDate.ToBinary(),
                            fileSize = File.FileSize,
                            description = File.Description
                        }
                    };
            else
                return new
                {
                    packetType = nameof(FileInfoResponse),
                    exists = Exists
                };
        }
    }
}
