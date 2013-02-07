using Avis.Exceptions;
using Avis.Immigrated;
using Avis.IO.Messages;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Avis.IO
{
    /// <summary>
    /// Base class for Elvin XDR frame codecs. Reads/writes messages
    /// to/from the Elvin XDR frame format 
    /// </summary>
    public abstract class FrameCodec
    {
        public void Encode(Message message, Stream outStream)
        {
            // buffer is auto deallocated
            MemoryStream buffer = new MemoryStream();
            message.Encode(buffer);

            // write frame type
            int frameSize = (int)buffer.Length + 4; // (= TypeId + data len)
            BinWriter.QuickWrite(outStream, (w) =>
                {
                    w.Write(frameSize);
                    w.Write(message.TypeId);
                });
            buffer.Position = 0;
            buffer.CopyTo(outStream);

            // if (isEnabled (TRACE) && buffer.limit () <= MAX_BUFFER_DUMP)
            //  trace ("Codec output: " + buffer.getHexDump (), this);

            // sanity check frame is 4-byte aligned
            if (frameSize % 4 != 0)
                throw new ProtocolCodecException
                  ("Frame length not 4 byte aligned for " + message.GetType().Name);

            /*int maxLength = maxFrameLengthFor (session);
    
            if (frameSize <= maxLength)
            {
              // write out whole frame
              buffer.flip ();
              out.write (buffer);
            } else
            {
              throw new FrameTooLargeException (maxLength, frameSize);
            }*/
        }

        public Message Decode(MemoryStream inStream)
        {
            int frameSize = BinReader.ReadInt32(inStream);
            long dataStart = inStream.Position;


            Message message = null;

            try
            {
                int messageType = BinReader.ReadInt32(inStream);

                message = NewMessage(messageType, frameSize);

                if (frameSize % 4 != 0)
                    throw new ProtocolCodecException("Frame length not 4 byte aligned");

                /*if (frameSize > maxLength)
                  throw new FrameTooLargeException (maxLength, frameSize);*/

                message.Decode(inStream);

                long bytesRead = inStream.Position - dataStart;

                if (bytesRead != frameSize)
                {
                    throw new ProtocolCodecException
                      ("Some input not read for " + message.GetType().Name + ": " +
                       "Frame header said " + frameSize +
                       " bytes, but only read " + bytesRead);
                }

                return message;
            }
            catch (ProtocolCodecException ex)
            {
                ErrorMessage error = new ErrorMessage(ex, message);

                // fill in XID if possible
                if (message is XidMessage && inStream.Length >= 12)
                {
                    inStream.Position = 8;
                    int xid = BinReader.ReadInt32(inStream);

                    if (xid > 0)
                        ((XidMessage)message).xid = xid;
                }

                return error;
            }
        }

        /**
         * Create a new instance of a message given a message type code and
         * frame length.
         */
        protected abstract Message NewMessage(int messageType, int frameSize);
    }
}
