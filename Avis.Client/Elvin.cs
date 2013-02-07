using Avis.Client.Exceptions;
using Avis.Client.Types;
using Avis.Common;
using Avis.Exceptions;
using Avis.IO.Messages;
using Avis.IO.Messages.Types;
using Avis.IO.Net;
using Avis.Security;
using Avis.Security.Special;
using NLog;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Avis.Client
{
    /// <summary>
    /// The core class in the Avis client library which manages a client's
    /// connection to an Elvin router. Typically a client creates a
    /// connection and then subscribes to notifications and/or
    /// sends them.
    /// </summary>
    public class Elvin : IDisposable
    {
        private static Logger logger = LogManager.GetCurrentClassLogger();        
                
        protected Dictionary<long, Subscription> subscriptions;

        protected ElvinConnector connector;
        protected LivenessMonitor livenessMonitor;

        // A method which handles received messages with highe rpriority before
        // forwarding the messages to outside handlers
        // (may suppress the forwarding)
        private volatile Action<MessageEventArgs> MessagePriorityHandler;

        #region Properties

        /// <summary>
        /// The router's URI.
        /// </summary>
        public ElvinURI RouterUri { get; private set; }

        /// <summary>
        /// Return the current options for the connection. These options
        /// reflect any changes made after initialisation, e.g. by using
        /// {@link #setReceiveTimeout(long)}, {@link
        /// #setNotificationKeys(Keys)}, etc.
        /// </summary>
        public ElvinOptions Options { get; private set; }

        /// <summary>
        /// The connection options established with the router. These cannot
        /// be changed after connection.
        /// </summary>
        public ConnectionOptions ConnectionOptions
        {
            get
            {
                return Options.connectionOptions;
            }
        }

        public bool Connected
        {
            get
            {
                return connector != null && connector.Connected;
            }
        }

        int receiveTimeout = 10000; // ten seconds
        /// <summary>
        /// The amount of time that must pass before the router is
        /// assumed not to be responding to a request message (default is 10
        /// seconds). This method can be used on a live connection, unlike
        /// {@link ElvinOptions#receiveTimeout}.
        /// </summary>
        public int ReceiveTimeout
        {
            get
            {
                return receiveTimeout;
            }
            set
            {
                if (value < 0)
                {
                    throw new ArgumentOutOfRangeException("ReceiveTimeout", "ReceiveTimeout must be greater than or equal to zero");
                }
                receiveTimeout = value;
                // update monitor
                livenessMonitor.ReceiveTimeout = value;
            }
        }

        int livenessTimeout = 60000; // 60s
        /// <summary>
        /// The liveness timeout period (default is 60 seconds). If no
        /// messages are seen from the router in this period a connection
        /// test message is sent and if no reply is seen within the
        /// {@linkplain #receiveTimeout() receive timeout period} the
        /// connection is deemed to be closed. This method can be used on a
        /// live connection, unlike {@link ElvinOptions#livenessTimeout}.
        /// </summary>
        public int LivenessTimeout
        {
            get
            {
                return livenessTimeout;
            }
            set
            {
                if (value < 0)
                {
                    throw new ArgumentOutOfRangeException("LivenessTimeout", "LivenessTimeout must be greater than or equal to zero");
                }
                livenessTimeout = value;
                // update monitor
                livenessMonitor.LivenessTimeout = value;
            }
        }

        #endregion

        #region Constructors

        /// <summary>
        /// Create a new connection to an Elvin router.
        /// </summary>
        /// <param name="elvinUri">A URI for the Elvin router.</param>
        public Elvin(String elvinUri)
            : this(new ElvinURI(elvinUri))
        {

        }

        /// <summary>
        /// Create a new connection to an Elvin router.
        /// </summary>
        /// <param name="elvinUri"> A URI for the Elvin router.</param>
        /// <param name="options">The connection options.</param>
        public Elvin(String elvinUri, ConnectionOptions options)
            : this(new ElvinURI(elvinUri), options)
        {

        }

        /// <summary>
        /// Create a new connection to an Elvin router.
        /// </summary>
        /// <param name="routerUri">A URI for the Elvin router.</param>
        public Elvin(ElvinURI routerUri)
            : this(routerUri, ConnectionOptions.EMPTY_OPTIONS, Keys.EMPTY_KEYS, Keys.EMPTY_KEYS)
        {

        }

        /// <summary>
        /// Create a new connection to an Elvin router.
        /// </summary>
        /// <param name="routerUri">A URI for the Elvin router.</param>
        /// <param name="options">The connection options.</param>
        public Elvin(ElvinURI routerUri, ConnectionOptions options)
            : this(routerUri, options, Keys.EMPTY_KEYS, Keys.EMPTY_KEYS)
        {

        }

        /// <summary>
        /// Create a new connection to an Elvin router.
        /// </summary>
        /// <param name="routerUri">The URI of the router to connect to.</param>
        /// <param name="notificationKeys">These keys automatically apply to all
        /// notifications, exactly as if they were added to the keys
        /// in the
        /// {@linkplain #send(Notification, Keys, SecureMode) send}
        /// call.</param>
        /// <param name="subscriptionKeys">These keys automatically apply to all
        /// subscriptions, exactly as if they were added to the keys
        /// in the
        /// {@linkplain #subscribe(String, Keys, SecureMode) subscription}
        /// call.</param>
        public Elvin(ElvinURI routerUri, Keys notificationKeys, Keys subscriptionKeys)
            : this(routerUri, ConnectionOptions.EMPTY_OPTIONS, notificationKeys, subscriptionKeys)
        {

        }

        /// <summary>
        /// Create a new connection to an Elvin router.
        /// </summary>
        /// <param name="routerUri">The URI of the router to connect to.</param>
        /// <param name="options">The connection options.</param>
        /// <param name="notificationKeys">These keys automatically apply to all
        /// notifications, exactly as if they were added to the keys
        /// in the
        /// {@linkplain #send(Notification, Keys, SecureMode) send}
        /// call.</param>
        /// <param name="subscriptionKeys">These keys automatically apply to all
        /// subscriptions, exactly as if they were added to the keys
        /// in the
        /// {@linkplain #subscribe(String, Keys, SecureMode) subscription}
        /// call.</param>
        public Elvin(ElvinURI routerUri, ConnectionOptions options, Keys notificationKeys, Keys subscriptionKeys)
            : this(routerUri, new ElvinOptions(options, notificationKeys, subscriptionKeys))
        {

        }

        /// <summary>
        /// Create a new connection to an Elvin router.
        /// </summary>
        /// <param name="routerUri">The URI of the router to connect to.</param>
        /// <param name="options">The Elvin client options. Modifying these after
        /// they have been passed into this constructor has no
        /// effect.</param>
        public Elvin(String routerUri, ElvinOptions options)
            : this(new ElvinURI(routerUri), options)
        {

        }

        /// <summary>
        /// Create a new connection to an Elvin router.
        /// </summary>
        /// <param name="routerUri">The URI of the router to connect to.</param>
        /// <param name="options">The Elvin client options. Modifying these after
        /// they have been passed into this constructor has no
        /// effect.</param>
        /// <exception cref="ArgumentException">Unsupported protocol</exception>
        /// <exception cref="TimeoutException"></exception>
        /// <exception cref="SocketException">Target refuses connection or is unreachable</exception>
        /// <exception cref="ConnectionOptionsException">Could negotiate connection options.</exception>
        public Elvin(ElvinURI routerUri, ElvinOptions options)
        {
            RouterUri = routerUri;
            Options = options;            
            this.subscriptions = new Dictionary<long, Subscription>();            

            if (!routerUri.Protocol.SequenceEqual(Protocols.DefaultProtocol) &&
                !routerUri.Protocol.SequenceEqual(Protocols.SecureProtocol))
            {
                throw new ArgumentException("Avis only supports protocols: " + Protocols.DefaultProtocol + " and " + Protocols.SecureProtocol + " : " + routerUri);
            }


            bool successfullyConnected = false;

            try
            {
                OpenConnection();

                // send connection request to router
                ConnRply connRply = SendAndReceive(new ConnRqst(routerUri.VersionMajor,
                                   routerUri.VersionMinor,
                                   options.connectionOptions.asMapWithLegacy(),
                                   options.notificationKeys,
                                   options.subscriptionKeys));

                if (connRply == null)
                {
                    throw new TimeoutException("Connection attempt timed out.");
                }

                Dictionary<String, Object> acceptedOptions = ConnectionOptions.convertLegacyToNew(connRply.options);

                Dictionary<String, Object> rejectedOptions = options.connectionOptions.differenceFrom(acceptedOptions);

                if (rejectedOptions.Count > 0)
                {
                    throw new ConnectionOptionsException(options.connectionOptions, rejectedOptions);
                }

                // include any options the router has added that we didn't specify        
                options.updateConnectionOptions(acceptedOptions);                

                // create the liveness monitor
                livenessMonitor = new LivenessMonitor(connector, LivenessTimeout, ReceiveTimeout);
                livenessMonitor.ConnectionDied += livenessMonitor_ConnectionDied;
                livenessMonitor.Enable();

                successfullyConnected = true;
            }
            finally
            {
                if (!successfullyConnected)
                    Close();
            }
        }

        #endregion

        #region Connection

        /// <summary>
        /// Open a network connection to the router.
        /// </summary>
        private void OpenConnection()
        {
            try
            {
                connector = new ElvinConnector();
                connector.Connect(RouterUri.Host, RouterUri.Port, RouterUri.IsSecure, Options.serverCertificate);

                // handle the low-level message receive
                connector.MessageReceived += connector_MessageReceived;

            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        void livenessMonitor_ConnectionDied(object sender, EventArgs e)
        {
            // the connection liveness check failed -> connection closed
            Close(CloseReason.RouterStoppedResponding, "Router stopped responding.");
        }

        /// <summary>
        /// Close the connection to the router. May be executed more than
        /// once with no effect.
        /// </summary>
        public void Close()
        {
            if (Connected)
            {
                Close(CloseReason.ClientShutdown, "Client shutting down normally");
            }
        }

        protected void Close(CloseReason reason, String message)
        {
            Close(reason, message, null);
        }

        /// <summary>
        /// Close this connection. May be executed more than once.
        /// </summary>
        /// <param name="reason"></param>
        /// <param name="message"></param>
        /// <param name="error"></param>
        protected void Close(CloseReason reason, String message, Exception error)
        {
            /* Use of atomic flag allows close () to be lock-free when already
             * closed, which avoids contention when IoHandler gets a
             * sessionClosed () event triggered by this method. */
            if (Connected)
            {
                lock (this)
                {
                    if (connector != null)
                    {
                        // force this here, so filter cannot block callback executor shutdown
                        //LivenessFilter.dispose (connection);

                        if (connector.Connected)
                        {
                            if (reason == CloseReason.ClientShutdown)
                            {
                                try
                                {
                                    SendAndReceive(new DisconnRqst());
                                }
                                catch (Exception ex)
                                {
                                    logger.Warn("Failed to cleanly disconnect: " + ex.Message);
                                }
                            }

                            connector.Close();
                        }

                        connector = null;
                    }
                }
            }

            OnClosed(reason, message, error);
        }

        #endregion

        #region Send & Receive

        private T SendAndReceive<T>(RequestMessage<T> request)         
            where T: XidMessage
        {
            ManualResetEvent evt = new ManualResetEvent(false);

            XidMessage reply = null;
            MessagePriorityHandler = (e) =>
                {
                    if (e.Message is XidMessage)
                    {
                        if (((XidMessage)e.Message).xid == request.xid)
                        {
                            reply = (XidMessage)e.Message; // this is the reply we were waiting for
                            e.Handled = true; // consume the message
                            evt.Set(); // signal success
                        }
                    }
                };

            Send (request);
            evt.WaitOne(ReceiveTimeout);
            MessagePriorityHandler = null; // no more priorized handling

            return (T)reply;            
        }

        void connector_MessageReceived(object sender, MessageEvent e)
        {
            // a message was received

            MessageEventArgs evtArgs = new MessageEventArgs(e.Message);

            // is there a priority handler?
            if (MessagePriorityHandler != null)
            {                
                MessagePriorityHandler(evtArgs);
                if (evtArgs.Handled)
                {
                    return; // supress the message
                }
            }

            // handle system messages
            HandleMessage(e.Message);

            if (MessageReceived != null)
                MessageReceived(this, evtArgs);
        }

        private void HandleMessage(Message message)
        {
            switch (message.TypeId)
            {
                case NotifyDeliver.ID:
                    HandleNotifyDeliver((NotifyDeliver)message);
                    break;
                case Disconn.ID:
                    HandleDisconnect((Disconn)message);
                    break;
                case DropWarn.ID:
                    logger.Warn("Router sent a dropped packet warning: a message may have been discarded due to congestion");
                    break;
                case ErrorMessage.ID:
                    HandleErrorMessage ((ErrorMessage)message);
                    break;
                default:
                    Close(CloseReason.ProtocolViolation, "Received unexpected message type " + message.Name);
                    break;
            }
        }

        private void Send(Message message)
        {
            //checkConnected ();

            connector.Send(message);
        }

        #endregion

        #region Subscriptions

        /// <summary>
        /// Create a new subscription. See
        /// {@link #subscribe(String, Keys, SecureMode)} for details.
        /// </summary>
        /// <param name="subscriptionExpr">The subscription expression to match notifications.</param>
        /// <returns></returns>
        /// <exception cref="InvalidSubscriptionException"></exception>
        /// <exception cref="NotConnectedException"></exception>
        public Subscription Subscribe(String subscriptionExpr)
        {
            return Subscribe(subscriptionExpr, Keys.EMPTY_KEYS, SecureMode.AllowInsecureDelivery);
        }

        /// <summary>
        /// Create a new subscription with a given set of security keys to
        /// enable secure delivery, but also allowing insecure notifications.
        /// See {@link #subscribe(String, Keys, SecureMode)} for details.
        /// </summary>
        /// <param name="subscriptionExpr">The subscription expression.</param>
        /// <param name="keys">The keys to add to the subscription.</param>
        /// <returns></returns>
        /// <exception cref="InvalidSubscriptionException"></exception>
        /// <exception cref="NotConnectedException"></exception>
        public Subscription Subscribe(String subscriptionExpr, Keys keys)
        {
            return Subscribe(subscriptionExpr, keys, SecureMode.AllowInsecureDelivery);
        }

        /// <summary>
        /// Create a new subscription with a given security mode but with an
        /// empty key set. Be careful when using REQUIRE_SECURE_DELIVERY with
        /// this subscription option: if you don't specify keys for the
        /// subscription elsewhere, either via {@link #setKeys(Keys, Keys)}
        /// or {@link Subscription#setKeys(Keys)}, the subscription will
        /// never be able to receive notifications.
        /// </summary>
        /// <param name="subscriptionExpr">The subscription expression to match notifications.</param>
        /// <param name="secureMode">The security mode: specifying
        ///   REQUIRE_SECURE_DELIVERY means the subscription will only
        ///   receive notifications that are sent by clients with keys
        /// matching the set supplied here or the global
        /// subscription key set.</param>
        /// <returns></returns>
        /// <exception cref="InvalidSubscriptionException"></exception>
        /// <exception cref="NotConnectedException"></exception>
        public Subscription Subscribe(String subscriptionExpr, SecureMode secureMode)
        {
            return Subscribe(subscriptionExpr, Keys.EMPTY_KEYS, secureMode);
        }

        /// <summary>
        /// Create a new subscription. The returned subscription instance can
        /// be used to listen for notifications, modify subscription settings
        /// and unsubscribe.
        /// </summary>
        /// <see cref="http://avis.sourceforge.net/subscription_language.html"/>
        /// <param name="subscriptionExpr">The subscription expression to match notifications.</param>
        /// <param name="keys">The keys to add to the subscription.</param>
        /// <param name="secureMode">The security mode: specifying
        ///   REQUIRE_SECURE_DELIVERY means the subscription will only
        ///   receive notifications that are sent by clients with keys
        /// matching the set supplied here or the global
        /// subscription key set.</param>
        /// <returns></returns>
        /// <exception cref="InvalidSubscriptionException"></exception>
        /// <exception cref="NotConnectedException"></exception>
        public Subscription Subscribe(String subscriptionExpr, Keys keys, SecureMode secureMode)
        {
            if (Connected)
            {
                Subscription subscription = new Subscription(this, subscriptionExpr, secureMode, keys);
                Subscribe(subscription);
                return subscription;
            }
            else
            {
                throw new NotConnectedException("Not connected to event router.");
            }
        }

        protected void Subscribe(Subscription subscription)
        {
            subscription.Id = SendAndReceive(new SubAddRqst(subscription.SubscriptionExpr, subscription.Keys, subscription.AcceptInsecure)).SubscriptionId;

            // register real ID
            lock (subscriptions)
            {
                if (subscriptions.ContainsKey(subscription.Id))
                {
                    Close(CloseReason.ProtocolViolation, "Router issued duplicate subscription ID " + subscription.Id);
                }

                subscriptions.Add(subscription.Id, subscription);
            }
        }

        protected void Unsubscribe(Subscription subscription)
        {
            SendAndReceive(new SubDelRqst(subscription.Id));

            lock (subscriptions)
            {
                if (!subscriptions.Remove(subscription.Id))
                    throw new ArgumentException("Internal error: invalid subscription ID " + subscription.Id);
            }
        }

        protected void ModifyKeys(Subscription subscription, Keys newKeys)
        {
            Delta delta = subscription.Keys.deltaFrom(newKeys);

            if (!delta.IsEmpty)
            {
                SendAndReceive
                  (new SubModRqst(subscription.Id, delta.added, delta.removed,
                                   subscription.AcceptInsecure));
            }
        }

        protected void ModifySubscriptionExpr(Subscription subscription, String subscriptionExpr)
        {
            SendAndReceive(new SubModRqst(subscription.Id, subscriptionExpr, subscription.AcceptInsecure));
        }

        protected void ModifySecureMode(Subscription subscription, SecureMode mode)
        {
            SendAndReceive(new SubModRqst(subscription.Id, "", mode == SecureMode.AllowInsecureDelivery));
        }

        /// <summary>
        /// Check if a given subscription is part of this connection.
        /// </summary>
        /// <param name="subscription"></param>
        /// <returns></returns>
        public bool HasSubscription (Subscription subscription)
        {
            lock(subscriptions)
            {
                return subscriptions.ContainsValue(subscription);
            }            
        }

        /// <summary>
        /// Return the subscriptions with the given ids
        /// </summary>
        /// <param name="ids"></param>
        /// <returns></returns>
        private IEnumerable<Subscription> GetSubscriptions(params long[] ids)
        {
            // Todo: raise exception for unknown ids
            return subscriptions.Where(x => ids.Contains(x.Key)).Select(x => x.Value);
        }

        #endregion

        #region Notifications

        /// <summary>
        /// Send a notification
        /// </summary>
        /// <param name="notification"></param>
        public void Send(Notification notification)
        {
            Send(notification, Keys.EMPTY_KEYS, SecureMode.AllowInsecureDelivery);
        }

        /// <summary>
        /// Send a notification with a set of keys but <b>with no requirement
        /// for secure delivery</b>: use <code>send (notification,
        /// REQUIRE_SECURE_DELIVERY, keys)</code> if you want only
        /// subscriptions with matching keys to receive a notification.
        /// </summary>
        /// <param name="notification"></param>
        /// <param name="keys"></param>
        public void Send(Notification notification, Keys keys)
        {
            Send(notification, keys, SecureMode.AllowInsecureDelivery);
        }

        /// <summary>
        /// Send a notification with a specified security mode. Be careful
        /// when using REQUIRE_SECURE_DELIVERY with this method: if you
        /// haven't specified any global notification keys via
        /// {@link #setKeys(Keys, Keys)} or
        /// {@link #setNotificationKeys(Keys)}, the notification will never
        /// be able to to be delivered.
        /// </summary>
        /// <param name="notification"></param>
        /// <param name="secureMode"></param>
        public void Send (Notification notification, SecureMode secureMode)
        {
            Send (notification, Keys.EMPTY_KEYS, secureMode);
        }

        /// <summary>
        /// Send a notification.
        /// </summary>
        /// <param name="notification">The notification to send.</param>
        /// <param name="keys">The keys that must match for secure delivery.</param>
        /// <param name="secureMode">The security requirement.
        ///   REQUIRE_SECURE_DELIVERY means the notification can only
        ///   be received by subscriptions with keys matching the set
        ///   supplied here (or the connections' {@linkplain #setNotificationKeys(Keys) global notification keys}).</param>
        public void Send(Notification notification, Keys keys, SecureMode secureMode)
        {            
            if (keys == null) throw new ArgumentNullException("keys");

            Send(new NotifyEmit(notification.Attributes, secureMode == SecureMode.AllowInsecureDelivery, keys));
        }

        #endregion

        #region Message Handlers

        private void HandleNotifyDeliver(NotifyDeliver message)
        {
            var ntfy = new Notification(message.attributes);

            foreach (var sb in GetSubscriptions(message.secureMatches))
            {
                if (!sb.RaiseNotify(ntfy, true))
                    // raise the general handler, when notification was not marked as handled
                    OnNotificationReceived(sb, ntfy, true);
            }
            foreach (var sb in GetSubscriptions(message.insecureMatches))
            {
                if (!sb.RaiseNotify(ntfy, false))
                    // raise the general handler, when notification was not marked as handled
                    OnNotificationReceived(sb, ntfy, false);
            }
        }

        private void HandleDisconnect(Disconn disconn)
        {
            CloseReason reason;
            String message;

            switch (disconn.Reason)
            {
                case DisconnectReason.Shutdown:
                    reason = CloseReason.RouterShutdown;
                    message = disconn.HasArgs ? disconn.Args : "Router is shutting down";
                    break;
                case DisconnectReason.ShutdownRedirect:
                    // todo handle REASON_SHUTDOWN_REDIRECT properly
                    reason = CloseReason.RouterShutdown;
                    message = "Router suggested redirect to " + disconn.Args;
                    break;
                case DisconnectReason.ProtocolViolation:
                    reason = CloseReason.ProtocolViolation;
                    message = disconn.HasArgs ? disconn.Args : "Protocol violation";
                    break;
                default:
                    reason = CloseReason.ProtocolViolation;
                    message = "Protocol violation";
                    break;
            }

            Close(reason, message);
        }

        private void HandleErrorMessage(ErrorMessage message)
        {
            Close(CloseReason.ProtocolViolation, "Protocol error in communication with router: " +
                   message.FormattedMessage,
                   message.Error);
        }

        #endregion

        #region Events

        #region Message received

        public event EventHandler<MessageEventArgs> MessageReceived;

        #endregion

        #region Notification received

        /// <summary>
        /// Raises when a notification for a subscription has been received
        /// </summary>
        public event EventHandler<NotificationEventArgs> NotificationReceived;

        /// <summary>
        /// Fires the NotificationReceived event
        /// </summary>
        /// <param name="subscription"></param>
        /// <param name="notification"></param>
        /// <param name="secure"></param>
        protected void OnNotificationReceived(Subscription subscription, Notification notification, bool secure)
        {
            if (NotificationReceived != null)
                NotificationReceived(this, new NotificationEventArgs(subscription, notification, secure, null));            
        }

        #endregion

        #region Closed

        /// <summary>
        /// Fired when the connection was closed
        /// </summary>
        public event EventHandler<CloseEventArgs> Closed;

        /// <summary>
        /// Fires the Closed event
        /// </summary>
        /// <param name="reason"></param>
        /// <param name="message"></param>
        /// <param name="exception"></param>
        protected void OnClosed(CloseReason reason, String message, Exception exception)
        {
            if (Closed != null)
            {
                Closed(this, new CloseEventArgs(reason, message, exception));
            }
        }

        #endregion

        #endregion

        public void Dispose()
        {
            if (Connected)
                Close(CloseReason.ClientShutdown, "Client disposed");
        }
    }

    #region Event Args

    public class MessageEventArgs : EventArgs
    {
        /// <summary>
        /// The received message
        /// </summary>
        public readonly Message Message;

        /// <summary>
        /// If set to true, the Message will not be forwarded to the subsequent
        /// handlers
        /// </summary>
        public bool Handled { get; set; }

        public MessageEventArgs(Message message)
        {
            Message = message;
            Handled = false;
        }
    }

    public class CloseEventArgs : EventArgs
    {
        public readonly CloseReason Reason;
        public readonly string Message;
        /// <summary>
        /// Exception which may have occured prior to closing the connection
        /// (may be null, especially for Reason.ClientShutdown and Reason.RouterShutdown)
        /// </summary>
        public readonly Exception Exception;

        public CloseEventArgs(CloseReason reason, string message, Exception exception)
        {
            Reason = reason;
            Message = message;
            Exception = exception;
        }
    }    

    #endregion
}
