using Avis.Immigrated;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Avis.IO.Messages
{
    public class SubRply : XidMessage
    {
        public const int ID = 61;

        public long SubscriptionId { get; private set; }

        public override int TypeId
        {
            get { return ID; }
        }

        public SubRply()
        {
            // zip
        }

        public SubRply(XidMessage inReplyTo, long subscriptionId)
            : base(inReplyTo)
        {
            this.SubscriptionId = subscriptionId;
        }


        public override void Encode(Stream outStream)
        {
            base.Encode(outStream);
            BinWriter.QuickWrite(outStream, (w) => w.Write(SubscriptionId));
        }

        public override void Decode(Stream inStream)
        {
            base.Decode(inStream);

            SubscriptionId = BinReader.ReadInt64(inStream);
        }
    }
}
