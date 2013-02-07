using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Avis.IO.Messages
{
    public class ErrorMessage : SyntheticMessage
    {
        public const int ID = -1;

        public Exception Error { get; private set; }
        public Message Cause { get; private set; }

        public override int TypeId
        {
            get { return ID; }
        }

        public ErrorMessage(Exception error, Message cause)
        {
            Error = error;
            Cause = cause;
        }

        /// <summary>
        /// Generate an error message suitable for presentation as a debugging aid.
        /// </summary>
        /// <returns></returns>
        public String FormattedMessage
        {
            get
            {
                StringBuilder message = new StringBuilder();

                if (Cause == null)
                    message.Append("Error decoding XDR frame");
                else
                    message.Append("Error decoding ").Append(Cause.Name);

                if (Error != null)
                    message.Append(": ").Append(Error.Message);

                return message.ToString();
            }
        }

    }
}
