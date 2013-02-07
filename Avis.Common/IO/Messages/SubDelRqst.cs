using Avis.Immigrated;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Avis.IO.Messages
{
    public class SubDelRqst : RequestMessage<SubRply>
    {
        public const int ID = 60;

        public long subscriptionId;

        public override Type ReplyType
        {
            get { return typeof(SubRply); }
        }

        public override int TypeId
        {
            get { return ID; }
        }

        public SubDelRqst()
        {
            // zip
        }

        public SubDelRqst(long subscriptionId)
            : base(nextXid())
        {
            this.subscriptionId = subscriptionId;
        }

        public override void Encode(Stream outStream)
        {
            base.Encode(outStream);

            BinWriter.QuickWrite(outStream, (w) => w.Write(subscriptionId));
        }

        public override void Decode(Stream inStream)
        {
            base.Decode(inStream);

            subscriptionId = BinReader.ReadInt64(inStream);
        }
    }
}
