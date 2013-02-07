using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Avis.Security.Special
{
   /// <summary>
    /// Represents a delta (diff) between two key sets.
   /// </summary>
    public class Delta
    {
        public static Delta EMPTY_DELTA = new Delta(Keys.EMPTY_KEYS, Keys.EMPTY_KEYS);

        public readonly Keys added;
        public readonly Keys removed;

        public Delta(Keys added, Keys removed)
        {
            this.added = added;
            this.removed = removed;
        }

        public bool IsEmpty
        {
            get
            {
                return added.IsEmpty && removed.IsEmpty;
            }
        }
    }
}
