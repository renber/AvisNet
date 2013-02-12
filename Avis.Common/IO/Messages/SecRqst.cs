using Avis.Security;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Avis.IO.Messages
{
    public class SecRqst : RequestMessage<SecRply>
    {
        public const int ID = 54;

        public Keys addNtfnKeys;
        public Keys delNtfnKeys;
        public Keys addSubKeys;
        public Keys delSubKeys;

        public override Type ReplyType
        {
            get { return typeof(SecRply); }
        }

        public override int TypeId
        {
            get { return ID; }
        }

        public SecRqst()
            // make it easier for client to create and assign keys later
            : this(Keys.EmptyKeys, Keys.EmptyKeys, Keys.EmptyKeys, Keys.EmptyKeys)
        {

        }

        public SecRqst(Keys addNtfnKeys, Keys delNtfnKeys,
                        Keys addSubKeys, Keys delSubKeys)
            : base(nextXid())
        {
            this.addNtfnKeys = addNtfnKeys;
            this.delNtfnKeys = delNtfnKeys;
            this.addSubKeys = addSubKeys;
            this.delSubKeys = delSubKeys;
        }


        public override void Encode(Stream outStream)
        {
            base.Encode(outStream);

            addNtfnKeys.Encode(outStream);
            delNtfnKeys.Encode(outStream);
            addSubKeys.Encode(outStream);
            delSubKeys.Encode(outStream);
        }

        public override void Decode(Stream inStream)
        {
            base.Decode(inStream);

            addNtfnKeys = Keys.Decode(inStream);
            delNtfnKeys = Keys.Decode(inStream);
            addSubKeys = Keys.Decode(inStream);
            delSubKeys = Keys.Decode(inStream);
        }
    }
}
