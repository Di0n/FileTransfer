using Server.Properties;
using Shared;
using System;
using System.Collections.Generic;
using System.IO;
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

        public static NetworkFile FromJson(dynamic json)
        {
            return new NetworkFile((string)json.id, (string)json.fileName, (string)json.fileFormat,
                DateTime.FromBinary((long)json.creationDate), (long)json.fileSize, (string)json.description);
        }

        public static dynamic ToJson(NetworkFile file)
        {
            return new
            {
                id = file.ID,
                fileName = file.Name,
                fileFormat = file.FileFormat,
                creationDate = file.CreationDate,
                fileSize = file.FileSize,
                description = file.Description
            };
        }
        public static string GetAssociatedFile(string path, string id)
        {
            return Directory.EnumerateFiles(path, "*.*", SearchOption.TopDirectoryOnly)
                .Where(f => !f.EndsWith(".json")).ToList().Find(f => f.Substring(f.LastIndexOf("\\")+1).StartsWith(id, StringComparison.InvariantCulture));
        }
    }
}
