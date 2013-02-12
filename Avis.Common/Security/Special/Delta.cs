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
        public static Delta EMPTY_DELTA = new Delta(Keys.EmptyKeys, Keys.EmptyKeys);

        public readonly Keys Added;
        public readonly Keys Removed;

        public Delta(Keys added, Keys removed)
        {
            this.Added = added;
            this.Removed = removed;
        }

        public bool IsEmpty
        {
            get
            {
                return Added.IsEmpty && Removed.IsEmpty;
            }
        }
    }
}
