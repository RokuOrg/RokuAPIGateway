using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Threading.Tasks;

namespace RokuAPIGateway
{
    [DataContract]
    public class Message
    {
        [DataMember(Name = "succes")]
        public bool Succes { get; set; }
        [DataMember(Name = "Object")]
        public dynamic Object { get; set; }
    }
}
