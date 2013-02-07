using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Avis.IO.Messages
{
    public class NotifyDeliver : Message
    {
        public const int ID = 57;

        public Dictionary<String, Object> attributes { get; private set; }
        public long[] secureMatches { get; set; }
        public long[] insecureMatches { get; set; }

        public NotifyDeliver()
        {
            attributes = new Dictionary<string, object>();
            secureMatches = new long[0];
            insecureMatches = new long[0];
        }

        public NotifyDeliver(Dictionary<String, Object> attributes,
                              long[] secureMatches, long[] insecureMatches)
        {
            this.attributes = attributes;
            this.secureMatches = secureMatches;
            this.insecureMatches = insecureMatches;
        }

        public override int TypeId
        {
            get { return ID; }
        }

        public override void Encode(System.IO.Stream outStream)
        {
            XdrCoding.putNameValues(outStream, attributes);
            XdrCoding.putLongArray(outStream, secureMatches);
            XdrCoding.putLongArray(outStream, insecureMatches);
        }

        public override void Decode(System.IO.Stream inStream)
        {
            attributes = XdrCoding.getNameValues(inStream);
            secureMatches = XdrCoding.getLongArray(inStream);
            insecureMatches = XdrCoding.getLongArray(inStream);
        }
    }
}
