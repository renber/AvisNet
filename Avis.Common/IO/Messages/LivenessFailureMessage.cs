using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Avis.IO.Messages
{
    public class LivenessFailureMessage : SyntheticMessage
    {
        public const int ID = -3;

        public override int TypeId
        {
            get { return ID; }
        }
    }
}
