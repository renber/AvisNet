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

        void add(IKeySet keys);

        void remove(IKeySet keys);

        bool add(Key key);

        bool remove(Key key);

        /**
         * Return this key with the given set removed.
         */
        IKeySet subtract(IKeySet keys);
    }
}
