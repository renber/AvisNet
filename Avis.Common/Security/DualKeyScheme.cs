using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Avis.Security
{
    /// <summary>
    /// A key scheme that requires a pair of keys. e.g. SHA-1 Dual.
    /// </summary>
    public sealed class DualKeyScheme : KeyScheme
    {
        /**
   * Specifies which of the two subsets of a dual scheme a key is part
   * of: the producer subset (for sending notifications) or consumer
   * subset (for receiving notifications).
   */
        public enum Subset { Producer, Consumer }

        public DualKeyScheme(int id, SecureHash keyHash)
            : base(id, keyHash, true, true)
        {

        }
    }
}
