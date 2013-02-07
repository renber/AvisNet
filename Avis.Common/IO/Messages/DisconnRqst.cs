using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Avis.IO.Messages
{
    public class DisconnRqst : RequestMessage<DisconnRply>
    {
        public const int ID = 51;

        public DisconnRqst()
            : base(nextXid())
        {

        }

        public override int TypeId
        {
            get { return ID; }
        }

        public override Type ReplyType
        {
            get { return typeof(DisconnRply); }
        }
    }
}
