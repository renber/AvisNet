using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Avis.Client
{
    /// <summary>
    /// Specifies the secure delivery mode for notifications.
    /// </summary>
    public enum SecureMode
    {
        /// <summary>
        /// Require secure key match between notification and subscription
        /// before delivering to a client.
        /// </summary>
        RequireSecureDelivery,
  
        /// <summary>
        /// Allow clients without matching keys to receive the message. Those
        /// with matching keys will still receive securely.
        /// </summary>
        AllowInsecureDelivery
    }
}
