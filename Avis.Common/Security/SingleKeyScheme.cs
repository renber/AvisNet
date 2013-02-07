using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Avis.Security
{
    public sealed class SingleKeyScheme : KeyScheme
    {
        public SingleKeyScheme(int id, SecureHash keyHash, bool producer, bool consumer)
            : base(id, keyHash, producer, consumer)
        {
            
        }
    }
}
