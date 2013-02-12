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
    public class UNotify : Notify
    {
        public const int ID = 32;

        public int clientMajorVersion;
        public int clientMinorVersion;

        public override int TypeId
        {
            get { return ID; }
        }

        public UNotify()
        {
            // zip
        }

        public UNotify(int clientMajorVersion,
                        int clientMinorVersion,
                        Dictionary<String, Object> attributes)
            : this(clientMajorVersion, clientMinorVersion, attributes, true, Keys.EmptyKeys)
        {

        }

        public UNotify(int clientMajorVersion,
                        int clientMinorVersion,
                        Dictionary<String, Object> attributes,
                        bool deliverInsecure,
                        Keys keys)
            : base(attributes, deliverInsecure, keys)
        {
            this.clientMajorVersion = clientMajorVersion;
            this.clientMinorVersion = clientMinorVersion;
        }


        public override void Decode(Stream inStream)
        {
            using (BinReader r = new BinReader(inStream))
            {
                clientMajorVersion = r.ReadInt32();
                clientMinorVersion = r.ReadInt32();
            }

            base.Decode(inStream);
        }

        public override void Encode(Stream outStream)
        {
            BinWriter.QuickWrite(outStream, (w) =>
            {
                w.Write(clientMajorVersion);
                w.Write(clientMinorVersion);
            });

            base.Encode(outStream);
        }
    }
}
