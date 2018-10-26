using System;
using System.Collections.Generic;
using System.Text;

namespace Shared
{
    public class NetworkFile
    {
        public NetworkFile() { }

        public NetworkFile(string name, string fileFormat,
           DateTime creationDate, long fileSize, string description)
        {
            ID = "";
            Name = name;
            FileFormat = fileFormat;
            CreationDate = creationDate;
            FileSize = fileSize;
            Description = description;
        }
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
        public string ID { get; set; }
        public string Name { get; set; }
        public string FileFormat { get; set; }
        public DateTime CreationDate { get; set; }
        public long FileSize { get; set; }
        public string Description { get;set; }

    }
}
