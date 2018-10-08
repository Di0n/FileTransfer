using System;
using System.Collections.Generic;
using System.Text;

namespace Shared.Packets
{
    /*
     * {
     *  "packetType":"Type",
     *  "data" : {
     *      "field" : "ajkhjkafhka"
     *  }
     * }
     */
    interface IPacket
    {
        dynamic ToJson();
        IPacket ToClass(dynamic json);
    }
}
