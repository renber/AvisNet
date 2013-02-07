using Avis.Security;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Avis.IO.Messages
{

    /// <summary>
    /// Base class for notify messages.
    /// </summary>
    public abstract class Notify : Message
    {
        public Dictionary<String, Object> attributes;
        public bool deliverInsecure;
        public Keys keys;

        protected Notify()
        {
            this.attributes = new Dictionary<string, object>();
            this.deliverInsecure = true;
            this.keys = Keys.EMPTY_KEYS;
        }

        protected Notify(params Object[] attributes)
            : this(asAttributes(attributes), true, Keys.EMPTY_KEYS)
        {

        }

        protected Notify(Dictionary<String, Object> attributes,
                          bool deliverInsecure,
                          Keys keys)
        {
            this.attributes = attributes;
            this.deliverInsecure = deliverInsecure;
            this.keys = keys;
        }

        public override void Decode(Stream inStream)
        {
            attributes = XdrCoding.getNameValues(inStream);
            deliverInsecure = XdrCoding.getBool(inStream);
            keys = Keys.decode(inStream);
        }

        public override void Encode(Stream outStream)
        {
            XdrCoding.putNameValues(outStream, attributes);
            XdrCoding.putBool(outStream, deliverInsecure);
            keys.encode(outStream);
        }

        public static Dictionary<String, Object> asAttributes(params Object[] pairs)
        {
            if (pairs.Length % 2 != 0)
                throw new ArgumentException("Items must be a set of pairs");

            Dictionary<String, Object> map = new Dictionary<String, Object>();

            for (int i = 0; i < pairs.Length; i += 2)
                map.Add((String)pairs[i], pairs[i + 1]);

            return map;
        }
    }
}
