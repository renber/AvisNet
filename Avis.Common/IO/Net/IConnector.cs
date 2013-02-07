using Avis.IO.Messages;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Avis.IO.Net
{
    /// <summary>
    /// Connector interface    
    /// </summary>
    // abstracted mainly for mocking in unit tests
    public interface IConnector
    {
        bool Connected { get; }

        void Connect(String hostname, int port);

        void Close();

        void Send(Message message);

        event EventHandler<MessageEvent> MessageReceived;
    }

    public class MessageEvent : EventArgs
    {
        public Message Message { get; private set; }

        public MessageEvent(Message message)
        {
            Message = message;
        }
    }
}
