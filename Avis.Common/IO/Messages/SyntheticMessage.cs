using Avis.Exceptions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Avis.IO.Messages
{
    public abstract class SyntheticMessage : Message
    {
        public override void Encode(System.IO.Stream outStream)
        {
            throw new ProtocolCodecException("Synthetic message");
        }

        public override void Decode(System.IO.Stream inStream)
        {
            throw new ProtocolCodecException("Synthetic message");
        }
    }
}
