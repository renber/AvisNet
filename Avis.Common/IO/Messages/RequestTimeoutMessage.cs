using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Avis.IO.Messages
{
    /// <summary>
    /// Synthetic message generated when a request timeout has elapsed.
    /// </summary>
    public class RequestTimeoutMessage : SyntheticMessage
    {
        public const int ID = -2;

        public override int TypeId
        {
            get { return ID; }
        }

        /// <summary>
        /// The request that timed out.
        /// </summary>
        public readonly IRequestMessage request;

        public RequestTimeoutMessage(IRequestMessage request)
        {
            this.request = request;
        }

    }
}
