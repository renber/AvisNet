using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Avis.IO.Messages
{
    public interface IRequestMessage
    {
        Type ReplyType { get; }
    }
}
