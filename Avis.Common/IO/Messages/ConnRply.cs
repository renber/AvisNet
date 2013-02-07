using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Avis.IO.Messages
{
    public class ConnRply : XidMessage
    {
        public const int ID = 50;

        /** Options requested by client that are supported. */
        public Dictionary<String, Object> options;

        public override int TypeId
        {
            get { return ID; }
        }

        public ConnRply()
        {
            // zip
        }

        public ConnRply(ConnRqst inReplyTo, Dictionary<String, Object> options)
            : base(inReplyTo)
        {            
            this.options = options;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="outStream"></param>
        /// <exception cref="ProtocolCodecException"></exception>
        public override void Encode(Stream outStream)
        {
            base.Encode (outStream);

            XdrCoding.putNameValues(outStream, options);
        }

        public override void Decode(Stream inStream)
        {
            base.Decode (inStream);

            options = XdrCoding.getNameValues(inStream);
        }
    }
}
