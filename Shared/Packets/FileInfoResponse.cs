using System;
using System.Collections.Generic;
using System.Text;

namespace Shared.Packets
{
    /*
     * {
     *  "packetType":"FileInfoResponse",
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
        public string ID { get; private set; }
        public string Name { get; private set; }
        public string FileFormat { get; private set; }
        public DateTime CreationDate { get; private set; }
        public long FileSize { get; private set; }
        public string Description { get; private set; }

        public FileInfoResponse(string id, string name, string fileFormat, 
            DateTime creationDate, long fileSize, string description)
        {
            ID = id;
            Name = name;
            FileFormat = fileFormat;
            CreationDate = creationDate;
            FileSize = fileSize;
            Description = description;
        }

        public static IPacket ToClass(dynamic json)
        {
            string id = (string)json.data.id;
            string name = (string)json.data.name;
            string fileFormat = (string)json.data.fileFormat;
            DateTime creationDate = DateTime.FromFileTime((long)json.data.creationDate);
            long fileSize = (long)json.data.fileSize;
            string description = (string)json.data.description;

            return new FileInfoResponse(id, name, fileFormat,
                creationDate, fileSize, description);
        }

        IPacket IPacket.ToClass(dynamic json)
        {
            return FileInfoResponse.ToClass(json);
        }

        public dynamic ToJson()
        {
            return new
            {
                packetType = nameof(FileInfoResponse),
                data = new
                {
                    id = ID,
                    name = Name,
                    fileFormat = FileFormat,
                    creationDate = CreationDate.ToFileTime(),
                    fileSize = FileSize,
                    description = Description
                }
            };
        }
    }
}
