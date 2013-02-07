using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Avis.IO.Messages
{
    public class DisconnRply : XidMessage
    {

        public const int ID = 52;

        public override int TypeId
        {
            get { return ID; }
        }

        public DisconnRply()
        {
            // zip
        }

        public DisconnRply(DisconnRqst inReplyTo)
            : base(inReplyTo)
        {
            
        } 
    }
}
