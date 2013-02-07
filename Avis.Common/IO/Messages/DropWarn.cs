using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Avis.IO.Messages
{
    public class DropWarn : Message
    {
        public const int ID = 62;

        public override int TypeId
        {
            get { return ID; }
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
