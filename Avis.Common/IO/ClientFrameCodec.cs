/*
 * port of org.avis.io.ClientFrameCodec
 */

using Avis.Exceptions;
using Avis.IO.Messages;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Avis.IO
{
    public class ClientFrameCodec : FrameCodec
    {
        public static readonly ClientFrameCodec Instance = new ClientFrameCodec ();

        protected override Message NewMessage(int messageType, int frameSize)
        {
            switch (messageType)
            {
                case ConnRqst.ID:
                    return new ConnRqst();
                case ConnRply.ID:
                    return new ConnRply();
                case DisconnRqst.ID:
                    return new DisconnRqst();
                case DisconnRply.ID:
                    return new DisconnRply();
                case Disconn.ID:
                    return new Disconn();
                case SubAddRqst.ID:
                    return new SubAddRqst();
                case SubRply.ID:
                    return new SubRply();
                case SubModRqst.ID:
                    return new SubModRqst();
                case SubDelRqst.ID:
                    return new SubDelRqst();
                case Nack.ID:
                    return new Nack();
                case NotifyDeliver.ID:
                    return new NotifyDeliver();
                case NotifyEmit.ID:
                    return new NotifyEmit();
                case TestConn.ID:
                    return TestConn.Instance;
                case ConfConn.ID:
                    return ConfConn.INSTANCE;
                case SecRqst.ID:
                    return new SecRqst();
                case SecRply.ID:
                    return new SecRply();
                case UNotify.ID:
                    return new UNotify();
                case DropWarn.ID:
                    return new DropWarn();
                case QuenchPlaceHolder.ADD:
                case QuenchPlaceHolder.MODIFY:
                case QuenchPlaceHolder.DELETE:
                    return new QuenchPlaceHolder(messageType, frameSize - 4);
                default:
                    throw new ProtocolCodecException
                      ("Unknown message type: ID = " + messageType);
            }
        }
    }
}
