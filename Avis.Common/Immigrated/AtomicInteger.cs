using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Avis.Immigrated
{
    /// <summary>
    /// Port of java.util.concurrent.atomic.AtomicInteger
    /// </summary>
    public class AtomicInteger
    {
        private const String lck = "L";
        private int i = 0;

        public int Value
        {
            get
            {
                return i;
            }
        }

        public AtomicInteger(int value)
        {
            i = value;
        }

        public int IncrementAndGet()
        {
            lock (lck)
            {
                i++;
                return i;
            }
        }

        public static implicit operator AtomicInteger(int i)
        {
            return new AtomicInteger(i);            
        }

        public static implicit operator int(AtomicInteger x) 
        {
            return x.Value;
        }

    }
}
