using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Avis.IO.Messages
{
    /// <summary>
    /// Placeholder for QnchAddRqst, QnchModRqst and QnchDelRqst that
    /// allows them to be decoded and sent to server. Server will currently
    /// NACK.
    /// </summary>
    public class QuenchPlaceHolder : XidMessage
    {
        public const int ID = -2;

        public const int ADD = 80;
        public const int MODIFY = 81;
        public const int DELETE = 82;

        public int messageType;
        public int length;

        public override int TypeId
        {
            get { return ID; }
        }

        public QuenchPlaceHolder(int messageType, int length)
        {
            this.messageType = messageType;
            this.length = length;
        }

        public override void Decode(Stream inStream)
        {
            base.Decode(inStream);

            inStream.Seek(length - 4, SeekOrigin.Current);
        }

        public override void Encode(Stream outStream)
        {
            throw new NotSupportedException("This is just a quench placeholder for now");
        }
    }
}
