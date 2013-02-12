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

        public readonly HashSet<Key> ProducerKeys;
        public readonly HashSet<Key> ConsumerKeys;

        public DualKeySet()
        {
            this.ProducerKeys = new HashSet<Key>();
            this.ConsumerKeys = new HashSet<Key>();
        }

        /// <summary>
        /// Create an immutable empty instance.
        /// </summary>
        /// <param name="immutable"></param>
        public DualKeySet(bool immutable)
        {
            this.ProducerKeys = new HashSet<Key>();
            this.ConsumerKeys = new HashSet<Key>();
        }

        public DualKeySet(IEnumerable<Key> producerKeys, IEnumerable<Key> consumerKeys)
        {
            this.ProducerKeys = new HashSet<Key>(producerKeys);
            this.ConsumerKeys = new HashSet<Key>(consumerKeys);
        }

        /// <summary>
        /// Get the keys for a producer or consumer.
        /// </summary>
        /// <param name="subset"></param>
        /// <returns></returns>
        public HashSet<Key> KeysFor(DualKeyScheme.Subset subset)
        {
            if (subset == DualKeyScheme.Subset.Producer)
                return ProducerKeys;
            else
                return ConsumerKeys;
        }

        public int Count
        {
            get { return ProducerKeys.Count + ConsumerKeys.Count; }
        }

        public bool IsEmpty
        {
            get { return ProducerKeys.Count == 0 && ConsumerKeys.Count == 0; }
        }

        public void Add(IKeySet theKeys)
        {
            DualKeySet keys = (DualKeySet)theKeys;

            keys.ProducerKeys.ToList().ForEach(x => ProducerKeys.Add(x));
            keys.ConsumerKeys.ToList().ForEach(x => ConsumerKeys.Add(x));
        }

        public void Remove(IKeySet theKeys)
        {
            DualKeySet keys = (DualKeySet)theKeys;

            keys.ProducerKeys.ToList().ForEach(x => ProducerKeys.Remove(x));
            keys.ConsumerKeys.ToList().ForEach(x => ConsumerKeys.Remove(x));
        }

        public bool Add(Key key)
        {
            throw new NotSupportedException("Cannot add to a dual key set");
        }

        public bool Remove(Key key)
        {
            throw new NotSupportedException("Cannot remove from a dual key set");
        }

        public IKeySet Subtract(IKeySet theKeys)
        {
            DualKeySet keys = (DualKeySet)theKeys;

            return new DualKeySet(ProducerKeys.Except(keys.ProducerKeys),
                                   ConsumerKeys.Except(keys.ConsumerKeys));
        }

        public override bool Equals(object obj)
        {
            return obj is DualKeySet && Equals((DualKeySet)obj);
        }


        public bool Equals(DualKeySet keyset)
        {
            return ProducerKeys.SequenceEqual(keyset.ProducerKeys) &&
                   ConsumerKeys.SequenceEqual(keyset.ConsumerKeys);
        }

        public override int GetHashCode()
        {
            var c = HashSet<Key>.CreateSetComparer();

            return c.GetHashCode(ProducerKeys) ^ c.GetHashCode(ConsumerKeys);
        }
    }
}
