using Avis.Exceptions;
using Avis.IO.Messages;
using NLog;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Security;
using System.Net.Sockets;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Threading.Tasks;

namespace Avis.IO.Net
{
    public class ElvinConnector : IConnector
    {
        private static Logger logger = LogManager.GetCurrentClassLogger();

        private String hostname;
        private int port;

        private TcpClient client;
        // stream used for secured connections
        private SslStream ssl;

        /// <summary>
        /// The public certificate used to authenticate the target router
        /// </summary>
        public X509Certificate2 SslCertificate { get; private set; }        

        public bool Connected
        {
            get
            {
                return client != null && client.Connected;
            }
        }

        /// <summary>
        /// The underlying network stream
        /// </summary>
        private Stream Stream
        {
            get
            {
                if (client == null)
                    return null;

                return ssl ?? (Stream)client.GetStream();
            }
        }

        /// <summary>
        /// Establishes an unsecure connection
        /// </summary>
        /// <param name="hostname"></param>
        /// <param name="port"></param>
        public void Connect(String hostname, int port)
        {
            Connect(hostname, port, false, null);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="hostname"></param>
        /// <param name="port"></param>
        /// <param name="secured"></param>
        /// <param name="certificate">The server's public certificate</param>
        public void Connect(String hostname, int port, bool secured, X509Certificate certificate)
        {
            this.hostname = hostname;
            this.port = port;

            try
            {
                client = new TcpClient();
                client.ExclusiveAddressUse = false;
                client.Connect(hostname, port);

                if (secured)
                {
                    EstablishSSL(certificate);
                }

                // Create the state object.
                StateObject state = new StateObject();
                state.workClient = client;

                Stream.BeginRead(state.buffer, 0, StateObject.BufferSize, new AsyncCallback(ReadCallback), state);
            }
            catch (Exception exc)
            {
                client = null;
                throw exc;
            }
        }

        public void Close()
        {
            if (Stream != null)
            {
                Stream.Close();
            }
            if (client != null)
            {                
                client.Close();
                client = null;
            }            
        }

        private void ReadCallback(IAsyncResult ar)
        {
            try
            {
                // Retrieve the state object and the client socket 
                // from the asynchronous state object.
                StateObject state = (StateObject)ar.AsyncState;
                TcpClient client = state.workClient;

                // Read data from the remote device.
                int bytesRead = Stream.EndRead(ar);

                bool complete = true;

                if (bytesRead > 0)
                {
                    // There might be more data, so store the data received so far.
                    state.receivedBytes.Write(state.buffer, 0, bytesRead);

                    // more data available?
                    if (client.Available > 0)
                    {
                        // Get the rest of the data.
                        Stream.BeginRead(state.buffer, 0, StateObject.BufferSize, new AsyncCallback(ReadCallback), state);
                        complete = false;
                    }
                }

                if (complete)
                {
                    // All the data has arrived; try to read the message
                    if (state.receivedBytes.Length > 0)
                    {
                        state.receivedBytes.Position = 0;
                        Message msg = ClientFrameCodec.Instance.Decode(state.receivedBytes);
                        OnMessageReceived(msg);
                    }

                    // start new reading
                    state.receivedBytes.Close();
                    state.receivedBytes = new MemoryStream();

                    Stream.BeginRead(state.buffer, 0, StateObject.BufferSize, new AsyncCallback(ReadCallback), state);
                }
            }
            catch (ObjectDisposedException)
            {
                // connection was closed -> read callback finished
                // all is well
                logger.Debug("ElvinConnector async read stopped due to connector being disposed. (this is not an error)");                
            }
            catch (Exception e)
            {
                logger.Error("ElvinConnector async read failed: " + e.Message);                
            }
        }

        public async void Send(Message message)
        {
            MemoryStream str = new MemoryStream();

            ClientFrameCodec.Instance.Encode(message, str);
            byte[] buf = str.ToArray();

            await Stream.WriteAsync(buf, 0, buf.Length);            
        }

        /// <summary>
        /// Creates a secured connection (SSL/TLS)
        /// (The connection needs already to be established beforehand)
        /// </summary>
        /// <exception cref="NotConnectedException">When the connection is closed</exception>
        private void EstablishSSL(X509Certificate certificate)
        {
            if (!Connected)
            {
                throw new NotConnectedException("Cannot establish ssl when the connection is closed.");
            }

            // certificate validation
            ssl = new SslStream(client.GetStream(), false, new RemoteCertificateValidationCallback((snd, srvCertificate, chain, sslPolicyErrors) =>
                {                    
                    if (certificate == null)
                        return sslPolicyErrors == SslPolicyErrors.None;
                    else                    
                        // trust the certificate if we have it right here
                        return certificate.Equals(srvCertificate);
                }));
            // authenticate                   
            ssl.AuthenticateAsClient(hostname);            
        }

        #region Events

        /// <summary>
        /// Fired when an elvin message has been received
        /// </summary>
        public event EventHandler<MessageEvent> MessageReceived;

        protected void OnMessageReceived(Message message)
        {
            if (MessageReceived != null)
            {
                MessageReceived(this, new MessageEvent(message));
            }
        }

        #endregion
    }

    // State object for receiving data from remote device.
    public class StateObject
    {
        // Client socket
        public TcpClient workClient = null;
        // Size of receive buffer
        public const int BufferSize = 256;
        // Receive buffer
        public byte[] buffer = new byte[BufferSize];
        // The received bytes
        public MemoryStream receivedBytes = new MemoryStream();
    }
}
