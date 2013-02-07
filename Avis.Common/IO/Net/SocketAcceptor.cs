using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Avis.IO.Net
{
    public class SocketAcceptor
    {
        private TcpListener m_listener;
        CancellationToken m_cancellationToken;

        public SocketAcceptor(IPAddress address, int port)
        {
            m_listener = new TcpListener(address, port);
            m_listener.ExclusiveAddressUse = false;
        }

        public Task Listen(Action<Socket> acceptClientAction, CancellationToken cancellationToken = default(CancellationToken))
        {            
            m_listener.Start();
            m_cancellationToken = cancellationToken;
            cancellationToken.Register(() => m_listener.Stop());
            return Task.Run(async () =>
            {
                while (true)
                {
                    cancellationToken.ThrowIfCancellationRequested();
                    acceptClientAction(await m_listener.AcceptSocketAsync());
                }
            }, cancellationToken);
        }
    }
}
