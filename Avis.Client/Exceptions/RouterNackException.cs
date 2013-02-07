using Avis.IO.Messages;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Avis.Client.Exceptions
{
    /// <summary>
    /// An exception indicating the Elvin router rejected (NACK'd) one of
    /// the client's requests.
    /// </summary>
    public class RouterNackException : Exception
    {
        public RouterNackException(String message)
            : base(message)
        {

        }

        public RouterNackException(XidMessage request, Nack nack)
            : base("Router rejected " + request.Name + ": " + nack.ErrorCodeText + ": " + nack.FormattedMessage)
        {

        }
    }
}
