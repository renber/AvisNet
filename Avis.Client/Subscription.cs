using Avis.Client.Types;
using Avis.Security;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Avis.Client
{
    /// <summary>
    /// A subscription to notifications from an Elvin connection.
    /// </summary>
    public sealed class Subscription
    {
        public Elvin Elvin { get; private set; }

        public long Id { get; internal set; }
        public readonly String SubscriptionExpr;
        public readonly SecureMode SecureMode;
        public readonly Keys Keys;

        /// <summary>
        /// Test if this subscription is still able to receive notifications.
        /// A subscription is inactive after a {@link #remove()} or when its
        /// underlying connection is closed.
        /// </summary>
        /// <returns></returns>
        public bool IsActive
        {
            get
            {
                return Elvin != null && Elvin.Connected && Id != 0;
            }
        }

        public bool AcceptInsecure
        {
            get
            {
                return SecureMode == Client.SecureMode.AllowInsecureDelivery;
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="elvin"></param>
        /// <param name="subscriptionExpr"></param>
        /// <param name="secureMode"></param>
        /// <param name="keys"></param>
        /// <exception cref="ArgumentException">subscriptionExpr is null or empty</exception>
        public Subscription(Elvin elvin, String subscriptionExpr, SecureMode secureMode, Keys keys)
        {
            if (keys == null) throw new ArgumentNullException("keys");
            if (secureMode == null) throw new ArgumentNullException("secureMode");

            this.Elvin = elvin;
            this.SubscriptionExpr = checkSubscription(subscriptionExpr);
            this.SecureMode = secureMode;
            this.Keys = keys;
        }

        /// <summary>
        /// Remove this subscription (unsubscribe). May be called more than
        /// once. Unlike the other methods on this class, this may be called
        /// on a subscription for a connection that has been closed without
        /// generating an error, since such a subscription is effectively
        /// removed anyway.
        /// </summary>
        public void Remove()
        {
            /*  lock (elvin)
              {
                  if (id == 0)
                      return;

                  if (elvin.IsOpen())
                      elvin.Unsubscribe(this);

                  id = 0;
              */
            /*if (elvin.IsOpen())
                elvin.callbacks.flush();*/
        }

        private static String checkSubscription(String subscriptionExpr)
        {
            if (subscriptionExpr == null)
                throw new ArgumentException("Subscription cannot be null");

            subscriptionExpr = subscriptionExpr.Trim();

            if (subscriptionExpr.Length == 0)
                throw new ArgumentException("Subscription expression cannot be empty");

            return subscriptionExpr;
        }

        #region Events

        public event EventHandler<NotificationEventArgs> Notify;

        /// <summary>
        /// Raises the Notify event
        /// (called by the Elvin class)
        /// </summary>
        /// <param name="notification"></param>
        /// <param name="secure"></param>
        /// <returns>Whether the notification was marked as handled</returns>
        internal bool RaiseNotify(Notification notification, bool secure)
        {
            var eargs = new NotificationEventArgs(this, notification, secure, null);

            if (Notify != null)
                Notify(this, eargs);

            return eargs.Handled;
        }


        #endregion

        #region Utils

        /// <summary>
        /// Escape illegal characters in a field name for use in a
        /// subscription expression. This can be useful when constructing
        /// subscription expressions dynamically.
        /// </summary>
        /// <param name="field">The string to use as a field name.</param>
        /// <returns>The escaped version of field, guaranteed to be a valid field name.</returns>
        public static String escapeField(String field)
        {
            StringBuilder escapedStr = new StringBuilder(field.Length * 2);

            for (int i = 0; i < field.Length; i++)
            {
                char c = field[i];
                bool escape = false;

                if (i == 0)
                    escape = !(char.IsLetter(c) || c == '_');
                else
                    escape = !(char.IsLetter(c) || char.IsDigit(c) || c == '_');

                if (escape)
                    escapedStr.Append('\\');

                escapedStr.Append(c);
            }

            return escapedStr.ToString();
        }

        /// <summary>
        /// Escape illegal characters in a string value for use in a
        /// subscription expression. This can be useful when constructing
        /// subscription expressions dynamically.
        /// </summary>
        /// <param name="s">A string that will occur within single or double  quotes in a subscription expression.</param>
        /// <returns>The escaped version of string, guaranteed to be a valid to occur inside a string expression.</returns>
        public static String escapeString(String s)
        {
            StringBuilder escapedStr = new StringBuilder(s.Length * 2);

            for (int i = 0; i < s.Length; i++)
            {
                char c = s[i];

                if (c == '\"' || c == '\'' || c == '\\')
                    escapedStr.Append('\\');

                escapedStr.Append(c);
            }

            return escapedStr.ToString();
        }

        #endregion
    }

    public class NotificationEventArgs : AvisEventArgs
    {
        /// <summary>
        /// The subscription that matched the notification.
        /// </summary>
        public readonly Subscription Subscription;
        /// <summary>
        /// True if the notification was received securely from a client with
        /// compatible security keys.
        /// </summary>
        public readonly bool Secure;
        /// <summary>
        /// The notification received from the router.
        /// </summary>
        public readonly Notification Notification;
        /// <summary>
        /// If the handling event is the subscriptions Notify event
        /// then setting this to true will prevent that the NotificationReceived
        /// of the Elvin class is raised for this notification
        /// </summary>
        public bool Handled { get; set; }

        public NotificationEventArgs(Subscription subscription, Notification notification, bool secure, Dictionary<String, Object> data)
            : base(data)
        {
            Subscription = subscription;
            Notification = notification;
            Secure = secure;
            Handled = false;
        }
    }
}
