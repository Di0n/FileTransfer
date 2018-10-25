using Newtonsoft.Json;
using Shared;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Server
{
    class NetworkFileInfo
    {
        
        public static NetworkFile GetNetworkFile(string json)
        {
            dynamic data = JsonConvert.DeserializeObject(json);
            
        }
    }
}
