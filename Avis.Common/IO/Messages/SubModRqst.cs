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
    public class SubModRqst : RequestMessage<SubRply>
    {
        public const int ID = 59;

        public long subscriptionId;
        public String subscriptionExpr;
        public bool acceptInsecure;
        public Keys addKeys;
        public Keys delKeys;

        public override Type ReplyType
        {
            get { return typeof(SubRply); }
        }

        public override int TypeId
        {
            get { return ID; }
        }

        public SubModRqst()
        {
            // zip
        }

        public SubModRqst(long subscriptionId, String subscriptionExpr,
                           bool acceptInsecure)
            : this(subscriptionId, subscriptionExpr, Keys.EMPTY_KEYS, Keys.EMPTY_KEYS, acceptInsecure)
        {

        }

        public SubModRqst(long subscriptionId,
                           Keys addKeys, Keys delKeys,
                           bool acceptInsecure)
            : this(subscriptionId, "", addKeys, delKeys, acceptInsecure)
        {

        }

        public SubModRqst(long subscriptionId, String subscriptionExpr,
                           Keys addKeys, Keys delKeys,
                           bool acceptInsecure)
            : base(nextXid())
        {
            this.subscriptionExpr = subscriptionExpr;
            this.subscriptionId = subscriptionId;
            this.acceptInsecure = acceptInsecure;
            this.addKeys = addKeys;
            this.delKeys = delKeys;
        }


        public override void Encode(Stream outStream)
        {
            base.Encode(outStream);

            BinWriter.QuickWrite(outStream, (w) => w.Write(subscriptionId));
            XdrCoding.putString(outStream, subscriptionExpr);
            XdrCoding.putBool(outStream, acceptInsecure);
            addKeys.encode(outStream);
            delKeys.encode(outStream);
        }

        public override void Decode(Stream inStream)
        {
            base.Decode(inStream);

            subscriptionId = BinReader.ReadInt64(inStream);
            subscriptionExpr = XdrCoding.getString(inStream);
            acceptInsecure = XdrCoding.getBool(inStream);
            addKeys = Keys.decode(inStream);
            delKeys = Keys.decode(inStream);
        }
    }
}
