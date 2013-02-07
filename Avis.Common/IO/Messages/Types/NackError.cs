using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Avis.Client.Types
{
    /// <summary>
    /// Selected NACK codes: see sec 7.4.2 of client spec
    /// </summary>
    public enum NackError
    {
        ProtIncompat = 0001,
        ProtError = 1001,
        NoSuchSub = 1002,
        ImplLimit = 2006,
        NotImpl = 2007,
        ParseError = 2101,
        ExpisTrivial = 2110
    }
}
