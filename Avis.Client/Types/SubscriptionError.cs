using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Avis.Client.Types
{
    /// <summary>
    /// Errors regarding subscriptions
    /// </summary>
    public enum SubscriptionError
    {
        /// <summary>
        /// Rejection code indicating there was a syntax error that prevented
        /// parsing. e.g. missing ")".
        /// </summary>
        SyntaxError,

        /// <summary>
        /// Rejection code indicating the expression was constant. i.e it
        /// matches everything or nothing. e.g. <tt>1 != 1</tt> or
        /// <tt>string ('hello')</tt>.
        /// </summary>
        TrivialExpression
    }
}
