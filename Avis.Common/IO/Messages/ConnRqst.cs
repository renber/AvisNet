using Avis.Immigrated;
using Avis.Security;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Avis.IO.Messages
{
    public class ConnRqst : RequestMessage<ConnRply>
    {
        public static readonly Dictionary<String, Object> EMPTY_OPTIONS = new Dictionary<string,object>();

        public const int ID = 49;

        public int versionMajor;
        public int versionMinor;
        public Dictionary<String, Object> options;
        public Keys notificationKeys;
        public Keys subscriptionKeys;

        public override int TypeId
        {
            get { return ID; }
        }

        public ConnRqst()
        {
            // zip
        }

        public ConnRqst(int major, int minor)
            : this(major, minor, EMPTY_OPTIONS, Keys.EmptyKeys, Keys.EmptyKeys)
        {
            
        }

        public ConnRqst(int major, int minor, Dictionary<String, Object> options,
                         Keys notificationKeys, Keys subscriptionKeys)
            : base(nextXid())
        {            
            this.versionMajor = major;
            this.versionMinor = minor;
            this.options = options;
            this.notificationKeys = notificationKeys;
            this.subscriptionKeys = subscriptionKeys;
        }

        public override Type ReplyType
        {
            get { return typeof(ConnRply); }
        }

        public override void Encode(Stream outStream)
        {
            base.Encode (outStream);
    
            using (BinWriter w = new BinWriter(outStream))
            {
                w.Write (versionMajor);
                w.Write (versionMinor);
            }
    
            XdrCoding.putNameValues (outStream, options);

            notificationKeys.Encode (outStream);
            subscriptionKeys.Encode(outStream);
        }

        public override void Decode(Stream inStream)
        {
            base.Decode (inStream);
    
            using(BinReader r = new BinReader(inStream))
            {
                versionMajor = r.ReadInt32 ();
                versionMinor = r.ReadInt32 ();
            }
    
            options = XdrCoding.getNameValues (inStream);
    
            notificationKeys = Keys.Decode (inStream);
            subscriptionKeys = Keys.Decode(inStream);
        }

    }
}
