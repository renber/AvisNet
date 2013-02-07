using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Avis.Exceptions
{
    public class ProtocolCodecException : Exception
    {
        public ProtocolCodecException(String msg)
            : base(msg)
        {
        }

        public ProtocolCodecException(String msg, Exception innerException)
            : base(msg, innerException)
        {
        }

    }
}
