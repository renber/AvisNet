using Avis.Security;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Threading.Tasks;

namespace Avis.Client
{
    /**
    * Options for configuring an Elvin client connection. Includes router
    * connection parameters, security keys, SSL parameters and timeouts.
    * 
    * The options object used to initialise the Elvin connection cannot
    * be directly changed after the connection is created, but some
    * settings can be changed on a live connection object using supported
    * methods such as {@link Elvin#setNotificationKeys(Keys)}, etc.
    */
    public sealed class ElvinOptions
    {
        /**
       * The options sent to the router to negotiate connection
       * parameters. After connection, these will be updated to include
       * any extra values sent by the router, including the
       * Vendor-Identification option.
       */
        public ConnectionOptions connectionOptions;

        /** 
         * The global notification keys that apply to all notifications. 
         */
        public Keys notificationKeys;

        /** 
         * The global subscription keys that apply to all subscriptions.
         */
        public Keys subscriptionKeys;

        /// <summary>
        /// The (public) certificate used for TLS/SSL secure connections (i.e. connections via "elvin:/secure/..." URI's)
        /// </summary>
        public X509Certificate serverCertificate;

        /**
         * Used to ensure that the router the client is connected to is
         * authentic. When true, only servers with a certificate
         * authenticated against the trusted certificates in the supplied
         * keystore or the JVM's CA certificates will be acceptable for
         * secure connections. See the <a
         * href="http://avis.sourceforge.net/tls.html">documentation at the
         * Avis web site</a> and <a
         * href="http://java.sun.com/j2se/1.5.0/docs/guide/security/jsse/JSSERefGuide.html#X509TrustManager">
         * the description of JSSE's X509TrustManager</a> for more
         * information.
         * 
         * @see #keystore
         */
        public bool requireAuthenticatedServer;

        /**
         * The amount of time (in milliseconds) that must pass before the
         * router is assumed to not be responding to a request. Default is
         * 10 seconds.
         *
         * @see Elvin#setReceiveTimeout(long)
         */
        public long receiveTimeout;

        /**
         * The liveness timeout period (in milliseconds). If no messages are
         * seen from the router in this period a connection test message is
         * sent and if no reply is seen the connection is deemed to be
         * defunct and automatically closed. Default is 60 seconds.
         * 
         * @see Elvin#setLivenessTimeout(long)
         */
        public long livenessTimeout;

        public ElvinOptions()
            : this(new ConnectionOptions(), new Keys(), new Keys())
        {

        }

        public ElvinOptions(ConnectionOptions connectionOptions,
                             Keys notificationKeys,
                             Keys subscriptionKeys)
        {
            if (notificationKeys == null) throw new ArgumentNullException("notificationKeys");
            if (subscriptionKeys == null) throw new ArgumentNullException("subscriptionKeys");
            if (connectionOptions == null) throw new ArgumentNullException("connectionOptions");

            this.connectionOptions = connectionOptions;
            this.notificationKeys = notificationKeys;
            this.subscriptionKeys = subscriptionKeys;
            this.requireAuthenticatedServer = false;
            this.receiveTimeout = 10000;
            this.livenessTimeout = 60000;
        }

        /**
         * Update connection options to include any new values from a given map.
         */
        public void updateConnectionOptions(Dictionary<String, Object> options)
        {
            if (options.Count > 0)
            {
                connectionOptions.setAll(options);
            }
        }

        /**
         * Shortcut to load a keystore from a <a
         * href="http://java.sun.com/j2se/1.5.0/docs/tooldocs/windows/keytool.html">Java
         * keystore file</a>.
         * 
         * @param keystorePath The file path for the keystore.
         * @param passphrase The passphrase for the keystore.
         * 
         * @throws IOException if an error occurred while loading the
         *                 keystore.
         * 
         * @see #setKeystore(URL, String)
         */
        public void setKeystore(String keystorePath, String passphrase)
        {
            // setKeystore (new File (keystorePath).toURL (), passphrase);
            throw new NotImplementedException();
        }

        /**
         * Shortcut to load a keystore from a <a
         * href="http://java.sun.com/j2se/1.5.0/docs/tooldocs/windows/keytool.html">Java
         * keystore file</a>.
         * 
         * @param keystoreUrl The URL for the keystore file.
         * @param passphrase The passphrase for the keystore.
         * 
         * @throws IOException if an error occurred while loading the
         *                 keystore.
         */
        public void setKeystore(Uri keystoreUrl, String passphrase)
        {
            /*
          InputStream keystoreStream = keystoreUrl.openStream ();

          try
          {
            KeyStore newKeystore = KeyStore.getInstance ("JKS");
      
            newKeystore.load (keystoreStream, passphrase.toCharArray ());
      
            keystore = newKeystore;
            keystorePassphrase = passphrase;
          } catch (GeneralSecurityException ex)
          {
            throw new IOException ("Error opening keystore: " + ex);
          } finally
          {
            close (keystoreStream);
          }*/
            throw new NotImplementedException();
        }
    }
}
