using System;
using System.Collections.Generic;
using System.Text;

namespace Shared.Packets
{
    class FileUploadRequest : IPacket
    {
        public FileUploadRequest(string name, long size)
        {
            FileName = name;
            FileSize = size;
        }

        public string FileName { get; private set; }
        public long FileSize { get; private set; }

        public static IPacket ToClass(dynamic json)
        {
            string fileName = (string)json.data.fileName;
            long fileSize = (long)json.data.fileSize;
            return new FileUploadRequest(fileName, fileSize);
        }

        IPacket IPacket.ToClass(dynamic json)
        {
            return FileUploadRequest.ToClass(json);
        }

        public dynamic ToJson()
        {
            return new
            {
                packetType = nameof(FileUploadRequest),
                data = new
                {
                    fileName = FileName,
                    fileSize = FileSize
                }
            };
        }
    }
}
