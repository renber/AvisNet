using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Avis.IO.Messages
{
    /// <summary>
    /// A XID-based request message that has a defined reply message.
    /// </summary>
    public abstract class RequestMessage<R> : XidMessage, IRequestMessage
        where R: XidMessage
    {
        public RequestMessage()
            : base()
        {
        
        }

        public RequestMessage(int xid)
            : base(xid)
        {

        }

        /// <summary>
        /// The type of a successful reply.
        /// </summary>
        public abstract Type ReplyType { get; }
    }
}
