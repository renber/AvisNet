using Avis.Security;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Avis.IO.Messages
{
    public class SubAddRqst : RequestMessage<SubRply>
    {
        public const int ID = 58;

        public String subscriptionExpr;
        public bool acceptInsecure;
        public Keys keys;

        public override Type ReplyType
        {
            get { return typeof(SubRply); }
        }

        public override int TypeId
        {
            get { return ID; }
        }

        public SubAddRqst()
        {
            // zip
        }

        public SubAddRqst(String subExpr)
            : this(subExpr, Keys.EmptyKeys, true)
        {

        }

        public SubAddRqst(String subExpr, Keys keys, bool acceptInsecure)
            : base(nextXid())
        {
            this.subscriptionExpr = subExpr;
            this.acceptInsecure = acceptInsecure;
            this.keys = keys;
        }

        public override void Encode(Stream outStream)
        {
            base.Encode(outStream);

            XdrCoding.putString(outStream, subscriptionExpr);
            XdrCoding.putBool(outStream, acceptInsecure);
            keys.Encode(outStream);
        }

        public override void Decode(Stream inStream)
        {
            base.Decode(inStream);

            subscriptionExpr = XdrCoding.getString(inStream);
            acceptInsecure = XdrCoding.getBool(inStream);
            keys = Keys.Decode(inStream);
        }
    }
}
