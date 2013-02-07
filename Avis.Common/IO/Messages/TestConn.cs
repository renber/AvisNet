using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Avis.IO.Messages
{
    public class TestConn : Message
    {
        public const int ID = 63;

        public readonly static TestConn Instance = new TestConn();

        public override int TypeId
        {
            get { return ID; }
        }

        private TestConn()
        {
            // zip
        }

        public override void Encode(System.IO.Stream outStream)
        {
            // zip
        }

        public override void Decode(System.IO.Stream inStream)
        {
            // zip
        }
    }
}
