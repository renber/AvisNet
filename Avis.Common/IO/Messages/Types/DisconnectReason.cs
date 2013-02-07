using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Avis.IO.Messages.Types
{
    /// <summary>
    /// Reason for a disconnect reported by a Disconn message
    /// </summary>
    public enum DisconnectReason
    {
        Unknown = -1,
        Shutdown = 1,
        ShutdownRedirect = 2,
        ProtocolViolation = 4        
    }
}
