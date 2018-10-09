using System;
using System.Collections.Generic;
using System.Text;

namespace Shared
{
    public class NetworkFile
    {
        public NetworkFile() { }
        public NetworkFile(string id, string name, string fileFormat,
            DateTime creationDate, long fileSize, string description)
        {
            ID = id;
            Name = name;
            FileFormat = fileFormat;
            CreationDate = creationDate;
            FileSize = fileSize;
            Description = description;
        }
        public string ID { get; private set; }
        public string Name { get; private set; }
        public string FileFormat { get; private set; }
        public DateTime CreationDate { get; private set; }
        public long FileSize { get; private set; }
        public string Description { get; private set; }
    }
}
