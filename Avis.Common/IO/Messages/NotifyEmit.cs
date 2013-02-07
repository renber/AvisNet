using Avis.Security;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Avis.IO.Messages
{
    public class NotifyEmit : Notify
    {
        public const int ID = 56;

        public override int TypeId
        {
            get { return ID; }
        }

        public NotifyEmit()
            : base()
        {

        }

        public NotifyEmit(params Object[] attributes)
            : base(attributes)
        {

        }

        public NotifyEmit(Dictionary<String, Object> attributes)
            : this(attributes, true, Keys.EMPTY_KEYS)
        {

        }

        public NotifyEmit(Dictionary<String, Object> attributes,
                           bool deliverInsecure,
                           Keys keys)
            : base(attributes, deliverInsecure, keys)
        {

        }
    }
}
