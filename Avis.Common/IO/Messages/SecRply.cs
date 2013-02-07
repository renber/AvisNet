using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Avis.IO.Messages
{
    public class SecRply : XidMessage
    {
        public const int ID = 55;

        public override int TypeId
        {
            get { return ID; }
        }

        public SecRply()
        {
            // zip
        }

        public SecRply(SecRqst inReplyTo)
            : base(inReplyTo)
        {

        }
    }
}
