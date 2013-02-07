using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Avis.Security
{
    /// <summary>
    /// A pair of key sets (producer/consumer) used for dual key schemes.
    /// </summary>
    public class DualKeySet : IKeySet
    {

        public readonly HashSet<Key> producerKeys;
        public readonly HashSet<Key> consumerKeys;

        public DualKeySet()
        {
            this.producerKeys = new HashSet<Key>();
            this.consumerKeys = new HashSet<Key>();
        }

        /**
         * Create an immutable empty instance.
         * 
         * @param immutable Used as a flag.
         */
        public DualKeySet(bool immutable)
        {
            this.producerKeys = new HashSet<Key>();
            this.consumerKeys = new HashSet<Key>();
        }

        public DualKeySet(IEnumerable<Key> producerKeys, IEnumerable<Key> consumerKeys)
        {
            this.producerKeys = new HashSet<Key>(producerKeys);
            this.consumerKeys = new HashSet<Key>(consumerKeys);
        }

        /**
         * Get the keys for a producer or consumer.
         * 
         * @param subset One of PRODUCER or CONSUMER.
         */
        public HashSet<Key> keysFor(DualKeyScheme.Subset subset)
        {
            if (subset == DualKeyScheme.Subset.Producer)
                return producerKeys;
            else
                return consumerKeys;
        }

        public int Count
        {
            get { return producerKeys.Count + consumerKeys.Count; }
        }

        public bool IsEmpty
        {
            get { return producerKeys.Count == 0 && consumerKeys.Count == 0; }
        }

        public void add(IKeySet theKeys)
        {
            DualKeySet keys = (DualKeySet)theKeys;

            keys.producerKeys.ToList().ForEach(x => producerKeys.Add(x));
            keys.consumerKeys.ToList().ForEach(x => consumerKeys.Add(x));
        }

        public void remove(IKeySet theKeys)
        {
            DualKeySet keys = (DualKeySet)theKeys;

            keys.producerKeys.ToList().ForEach(x => producerKeys.Remove(x));
            keys.consumerKeys.ToList().ForEach(x => consumerKeys.Remove(x));
        }

        public bool add(Key key)
        {
            throw new NotSupportedException("Cannot add to a dual key set");
        }

        public bool remove(Key key)
        {
            throw new NotSupportedException("Cannot remove from a dual key set");
        }

        public IKeySet subtract(IKeySet theKeys)
        {
            DualKeySet keys = (DualKeySet)theKeys;

            return new DualKeySet(producerKeys.Except(keys.producerKeys),
                                   consumerKeys.Except(keys.consumerKeys));
        }

        public override bool Equals(object obj)
        {
            return obj is DualKeySet && Equals((DualKeySet)obj);
        }


        public bool Equals(DualKeySet keyset)
        {
            return producerKeys.SequenceEqual(keyset.producerKeys) &&
                   consumerKeys.SequenceEqual(keyset.consumerKeys);
        }

        public override int GetHashCode()
        {
            var c = HashSet<Key>.CreateSetComparer();

            return c.GetHashCode(producerKeys) ^ c.GetHashCode(consumerKeys);
        }
    }
}
