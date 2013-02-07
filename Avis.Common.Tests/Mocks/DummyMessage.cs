using Avis.IO.Messages;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Avis.Common.Tests.Mocks
{
    public class DummyMessage : Message
    {
        public override int TypeId
        {
            get { return -42; }
        }

        public override void Encode(System.IO.Stream outStream)
        {
            if (EncodeCalled != null)
                EncodeCalled(this, EventArgs.Empty);
        }

        public override void Decode(System.IO.Stream inStream)
        {
            if (DecodeCalled != null)
                DecodeCalled(this, EventArgs.Empty);
        }

        public event EventHandler EncodeCalled;
        public event EventHandler DecodeCalled;
    }
}
