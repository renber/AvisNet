using Avis.Utils;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Avis.Client.Exceptions
{
    /// <summary>
    /// Thrown when the Elvin client receives a rejection of one or more
    /// requested connection options.
    /// </summary>
    public class ConnectionOptionsException : Exception
    {
        /** The requested options */
        public readonly ConnectionOptions options;
        /** The rejected options and the actual value that the server will use. */
        public readonly Dictionary<String, Object> rejectedOptions;

        public ConnectionOptionsException(ConnectionOptions options, Dictionary<String, Object> rejectedOptions)
            : base("Router rejected connection options: rejected options and actual values: " + TextUtils.MapToString(rejectedOptions))
        {
            this.options = options;
            this.rejectedOptions = rejectedOptions;
        }
    }
}
