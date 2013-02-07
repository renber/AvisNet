using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Avis.Client.Types
{
    /// <summary>
    /// Reason why a connection was closed
    /// </summary>
    public enum CloseReason
    {
        /// <summary>
        /// The client was shut down normally with a call to Elvin.Close()
        /// </summary>
        ClientShutdown,
        /// <summary>
        /// The router was shut down normally
        /// </summary>
        RouterShutdown,
        /// <summary>
        /// The router failed to respond to a liveness check. Either the  
        /// router has crashed, or network problems have stopped messages 
        /// getting through.                                                      
        /// </summary>
        RouterStoppedResponding,
        /// <summary>
        /// The network connection to the router was terminated abnormally
        /// without the standard shutdown protocol. Most likely the network
        /// connection between client and router has been disconnected.
        /// </summary>
        RouterShutdownUnexpectedly,
        /// <summary>
        /// Either the client or the router decided that the protocol rules
        /// have been violated. This would only happen in the case of a
        /// serious bug in the client or router.
        /// </summary>
        ProtocolViolation,
        /// <summary>
        /// An I/O exception was thrown while communicating with the router.
        /// The exception will be in the error field.
        /// </summary>
        IoError
    }
}
