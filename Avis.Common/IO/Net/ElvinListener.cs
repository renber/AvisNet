using Avis.IO.Messages;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Avis.IO.Net
{
    /// <summary>
    /// TCP-Server & Client which handles message receiving and sending
    /// </summary>
    public class ElvinListener
    {
        SocketAcceptor socket;
        CancellationTokenSource cts;

        public ElvinListener()
        {
            
        }        

        public void Listen(IPAddress address, int port)
        {
            socket = new SocketAcceptor(address, port);
            cts = new CancellationTokenSource();
            socket.Listen(new Action<Socket>(Incoming), cts.Token);
        }

        public void Incoming(Socket s)
        {
            Thread.Sleep(200);

            using (MemoryStream bigBuf = new MemoryStream())
            {
                byte[] smallBuf = new byte[2048];
                
                while (s.Available > 0)
                {
                    int len = s.Receive(smallBuf);
                    bigBuf.Write(smallBuf, 0, len);                    
                }

                if (bigBuf.Length > 0)
                {
                    bigBuf.Position = 0;
                    Message msg = ClientFrameCodec.Instance.Decode(bigBuf);
                    OnMessageReceived(msg);
                }
            }            
        }

        public void Send(Message message, String hostName, int port)
        {
            Task.Run(() => {
            TcpClient client = new TcpClient();
            client.Connect(hostName, port);

            ClientFrameCodec.Instance.Encode(message, client.GetStream());
            });
        }

        public void Stop()
        {
            if (cts != null)
            {
                cts.Cancel();
                cts = null;
            }
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
}
