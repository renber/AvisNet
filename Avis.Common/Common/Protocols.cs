using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Avis.Common
{
    /// <summary>
    /// Default protocols for the ElvinUri class
    /// </summary>
    public class Protocols
    {
        /// <summary>
        /// The default unsecure protocol
        /// </summary>
        public static readonly IList<String> DefaultProtocol = (new List<String>() { "tcp", "none", "xdr" }).AsReadOnly();

        /*
         * The standard secure protocl
         * Note: you might expect "tcp,ssl,xdr" as the logical secure stack,
         * but Mantara Elvin uses "ssl,none,xdr" and spec also indicates
         * this is correct, so we comply.
         */
        public static readonly IList<String> SecureProtocol = (new List<String>() { "ssl", "none", "xdr" }).AsReadOnly();
    }
}
