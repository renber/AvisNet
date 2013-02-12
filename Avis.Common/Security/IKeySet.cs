using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Avis.Security
{
    /**
 * A polymorphic key set stored as part of a {@link Keys} key
 * collection: may be either a single set of Key items or a dual set
 * for the dual key schemes. Clients should not generally need to
 * access key sets directly: use the {@link Keys} class instead.
 * 
 */
    public interface IKeySet
    {
        int Count { get; }

        bool IsEmpty { get; }

        void Add(IKeySet keys);

        void Remove(IKeySet keys);

        bool Add(Key key);

        bool Remove(Key key);

        /// <summary>
        /// Return this keyset with the given set removed.
        /// </summary>
        /// <param name="keys"></param>
        /// <returns></returns>
        IKeySet Subtract(IKeySet keys);
    }
}
