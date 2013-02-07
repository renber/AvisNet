using Avis.Exceptions;
using Avis.Immigrated;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Avis.IO.Messages
{
    /// <summary>
    /// Base class for messages that use a transaction id to identify replies.
    /// </summary>
    public abstract class XidMessage : Message
    {
        private static readonly AtomicInteger xidCounter = 0;

        public int xid { get; set; }

        /**
        * The request message that triggered this reply. This is for the
        * convenience of message processing, not part of the serialized
        * format: you need to add a {@link RequestTrackingFilter} to the
        * filter chain if you want this automatically filled in.
        */
        [NonSerialized]
        public IRequestMessage request;

        public XidMessage()
        {
            xid = -1;
        }

        public XidMessage(XidMessage inReplyTo)
            : this(inReplyTo.xid)
        {
            
        }

        public XidMessage(int xid)
        {
            if (xid <= 0)
                throw new ArgumentException("Invalid XID: " + xid);

            this.xid = xid;
        }

        public bool hasValidXid()
        {
            return xid > 0;
        }

        public override void Encode(Stream outStream)
        {
            if (xid == -1) throw new ProtocolCodecException ("No XID");
    
            BinWriter.QuickWrite(outStream, (w) => w.Write(xid));            
        }

        public override void Decode(Stream inStream)
        {
            xid = BinReader.ReadInt32(inStream);

            if (xid <= 0)
                throw new ProtocolCodecException("XID must be >= 0: " + xid);
        }

        protected static int nextXid ()
        {
            // NOTE: XID must not be zero (sec 7.4)	  
            return xidCounter.IncrementAndGet ();
        }
    }
}
