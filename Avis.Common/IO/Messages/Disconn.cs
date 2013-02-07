using Avis.Exceptions;
using Avis.Immigrated;
using Avis.IO.Messages.Types;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Avis.IO.Messages
{
    public class Disconn : Message
    {
        public const int ID = 53;

        public DisconnectReason Reason { get; set; }
        public String Args { get; set; }

        public Disconn()
            : this(DisconnectReason.Unknown, "")
        {

        }

        public Disconn(DisconnectReason reason)
            : this(reason, "")
        {

        }

        public Disconn(DisconnectReason reason, String args)
        {
            this.Reason = reason;
            this.Args = args;
        }

        public override int TypeId
        {
            get { return ID; }
        }

        public bool HasArgs
        {
            get
            {
                return Args.Length > 0;
            }
        }

        public override void Decode(Stream inStream)
        {
            Reason = (DisconnectReason)BinReader.ReadInt32(inStream);
            Args = XdrCoding.getString(inStream);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="outStream"></param>
        /// <exception cref="ProtocolCodecException"></exception>
        public override void Encode(Stream outStream)
        {
            if (Reason == DisconnectReason.Unknown)
                throw new ProtocolCodecException("Reason not set");

            BinWriter.QuickWrite(outStream, (w) => w.Write((int)Reason));

            XdrCoding.putString(outStream, Args);
        }
    }
}
