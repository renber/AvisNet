using Avis.Client.Types;
using Avis.IO.Messages;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Avis.Client.Exceptions
{
    /// <summary>
    /// Thrown when a subscription parse error is detected by the router.
    /// </summary>
    public class InvalidSubscriptionException : RouterNackException
    {
        /// <summary>
        /// The subscription expression that was rejected.
        /// </summary>
        public readonly String Expression;

        /// <summary>
        /// The reason the expression was rejected
        /// </summary>
        public readonly SubscriptionError Reason;

        public InvalidSubscriptionException(Message request, Nack nack)
            : base(TextForErrorCode(nack.error) + ": " + nack.FormattedMessage)
        {

            switch (request.TypeId)
            {
                case SubAddRqst.ID:
                    Expression = ((SubAddRqst)request).subscriptionExpr;
                    break;
                case SubModRqst.ID:
                    Expression = ((SubModRqst)request).subscriptionExpr;
                    break;
                default:
                    Expression = "";
                    break;
            }

            Reason = nack.error == NackError.ExpisTrivial ? SubscriptionError.TrivialExpression : SubscriptionError.SyntaxError;
        }

        private static String TextForErrorCode(NackError error)
        {
            switch (error)
            {
                case NackError.ParseError:
                    return "Syntax error";
                case NackError.ExpisTrivial:
                    return "Trivial expression";
                default:
                    return "Syntax error (" + error + ")";
            }
        }
    }
}
