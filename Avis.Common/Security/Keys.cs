using Avis.Exceptions;
using Avis.Immigrated;
using Avis.IO;
using Avis.Security.Special;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Avis.Security
{
 /**
 * A key collection used to secure notifications. A key collection
 * contains zero or more mappings from a {@linkplain KeyScheme key
 * scheme} to the {@linkplain Key keys} registered for that scheme.
 * <p>
 * Once in use, key collections should be treated as immutable
 * i.e. never be modified directly after construction.
 * <p>
 * See also section 7.4 of the client protocol spec.     
 * 
 */    
    
    /// <summary>
    /// A key collection used to secure notifications. A key collection
    /// contains zero or more mappings from a key scheme to the keys registered for that scheme.
    /// Once in use, key collections should be treated as immutable
    /// i.e. never be modified directly after construction.
    /// (See also section 7.4 of the client protocol spec.)
    /// </summary>
    public class Keys
    {

        /// <summary>
        /// An empty, immutable key collection.
        /// </summary>
        public static readonly Keys EmptyKeys = new EmptyKeys();

        private static readonly DualKeySet EmptyDualKeyset = new DualKeySet(true);
        private static readonly SingleKeySet EmptySingleKeyset = new EmptySingleKeys();

        /**
         * NB: this set must be kept normalised i.e. if there is a key
         * scheme in the map, then there must be a non-empty key set
         * associated with it.
         */
        private Dictionary<KeyScheme, IKeySet> keySets;

        public Keys()
        {
            // todo opt: since schemes are static, could use a more optimized map here
            keySets = new Dictionary<KeyScheme, IKeySet>(4);
        }

        public Keys(Keys keys)
            : this()
        {
            Add(keys);
        }

        /// <summary>
        /// True if no keys are in the collection.
        /// </summary>
        public bool IsEmpty
        {
            get
            {
                return keySets.Count == 0;
            }
        }

        /// <summary>
        /// Return the total number of keys in this key collection.
        /// </summary>
        public int Size
        {
            get
            {
                if (IsEmpty)
                    return 0;

                int size = 0;
                foreach (IKeySet keyset in keySets.Values)
                    size += keyset.Count;

                return size;
            }
        }

       /// <summary>
       /// Shortcut to efficiently generate a key collection that represents
       /// this key collection's union with another.
       /// </summary>
       /// <param name="keys">The keys to add.</param>
       /// <returns>If keys is empty, this method will simply return this
       ///        collection. If this collection is empty, keys will be
       ///         returned. Otherwise a new collection instance is created
       ///         as the union of both.</returns>
        public Keys AddedTo(Keys keys)
        {
            if (keys.IsEmpty)
            {
                return this;
            }
            else if (IsEmpty)
            {
                return keys;
            }
            else
            {
                Keys newKeys = new Keys(this);

                newKeys.Add(keys);

                return newKeys;
            }
        }

        /// <summary>
        /// Add a key for single key scheme.
        /// </summary>
        /// <param name="scheme">The key scheme.</param>
        /// <param name="key">The key to add.</param>
        public virtual void Add(SingleKeyScheme scheme, Key key)
        {
            NewKeysetFor(scheme).Add(key);
        }

        /// <summary>
        /// Remove a key for single key scheme.
        /// </summary>
        /// <param name="scheme">The key scheme.</param>
        /// <param name="key">The key to remove.</param>
        public virtual void Remove(SingleKeyScheme scheme, Key key)
        {
            IKeySet keys;

            if (keySets.TryGetValue(scheme, out keys))
            {
                keys.Remove(key);

                if (keys.IsEmpty)
                    keySets.Remove(scheme);
            }
        }

        /// <summary>
        /// Add a key for dual key scheme.
        /// </summary>
        /// <param name="scheme">The key scheme.</param>
        /// <param name="subset">The key subset (PRODUCER or CONSUMER) to add the key to. </param>
        /// <param name="key"> The key to add.</param>
        public virtual void Add(DualKeyScheme scheme,
                         DualKeyScheme.Subset subset, Key key)
        {
            NewKeysetFor(scheme).KeysFor(subset).Add(key);
        }

        /// <summary>
        /// Remove a key for dual key scheme.
        /// </summary>
        /// <param name="scheme">The key scheme.</param>
        /// <param name="subset">The key subset (PRODUCER or CONSUMER) to remove the key from.</param>
        /// <param name="key">The key to remove.</param>
        public virtual void Remove(DualKeyScheme scheme,
                            DualKeyScheme.Subset subset,
                            Key key)
        {
            IKeySet keySet;

            if (keySets.TryGetValue(scheme, out keySet))
            {
                ((DualKeySet)keySet).KeysFor(subset).Remove(key);

                if (keySet.IsEmpty)
                    keySets.Remove(scheme);
            }
        }

        /// <summary>
        /// Add all keys in a collection.
        /// </summary>
        /// <param name="keys">The keys to add.</param>
        public virtual void Add(Keys keys)
        {
            if (keys == this)
                throw new ArgumentException("Cannot add key collection to itself");

            foreach (KeyScheme scheme in keys.keySets.Keys)
                Add(scheme, keys.keySets[scheme]);
        }

        private void Add(KeyScheme scheme, IKeySet keys)
        {
            if (!keys.IsEmpty)
                NewKeysetFor(scheme).Add(keys);
        }

        /// <summary>
        /// Remove all keys in a collection.
        /// </summary>
        /// <param name="keys">The keys to remove</param>
        public virtual void remove(Keys keys)
        {
            if (keys == this)
                throw new ArgumentException("Cannot remove key collection from itself");

            foreach (KeyScheme scheme in keys.keySets.Keys)
            {
                IKeySet myKeys;

                if (keySets.TryGetValue(scheme, out myKeys))
                {
                    myKeys.Remove(keys.KeysetFor(scheme));

                    if (myKeys.IsEmpty)
                        keySets.Remove(scheme);
                }
            }
        }

        /// <summary>
        /// Create a new key collection with some keys added/removed. This
        /// does not modify the current collection.
        /// </summary>
        /// <param name="toAdd">Keys to add.</param>
        /// <param name="toRemove">Keys to remove</param>
        /// <returns>A new key set with keys added/removed. If both add/remove
        /// key sets are empty, this returns the current instance.</returns>
        public Keys Alter(Keys toAdd, Keys toRemove)
        {
            if (toAdd.IsEmpty && toRemove.IsEmpty)
            {
                return this;
            }
            else
            {
                Keys keys = new Keys(this);

                keys.Add(toAdd);
                keys.remove(toRemove);

                return keys;
            }
        }

        /// <summary>
        /// Compute the changes between one key collection and another.
        /// </summary>
        /// <param name="keys">The target key collection.</param>
        /// <returns>The delta (i.e. key sets to be added and removed)
        /// required to change this collection into the target.</returns>
        public Delta DeltaFrom(Keys keys)
        {
            if (keys == this)
                return Delta.EMPTY_DELTA;

            Keys addedKeys = new Keys();
            Keys removedKeys = new Keys();

            foreach (KeyScheme scheme in KeyScheme.Schemes)
            {
                IKeySet existingKeyset = KeysetFor(scheme);
                IKeySet otherKeyset = keys.KeysetFor(scheme);

                addedKeys.Add(scheme, otherKeyset.Subtract(existingKeyset));
                removedKeys.Add(scheme, existingKeyset.Subtract(otherKeyset));
            }

            return new Delta(addedKeys, removedKeys);
        }

        /// <summary>
        /// Get the key set for a given scheme. This set should not be modified.
        /// </summary>
        /// <param name="scheme">The scheme.</param>
        /// <returns>The key set for the scheme. Will be empty if no keys are
        ///        defined for the scheme.</returns>
        private IKeySet KeysetFor(KeyScheme scheme)
        {
            IKeySet keys;

            if (!keySets.TryGetValue(scheme, out keys))
                return scheme.IsDual() ? (IKeySet)EmptyDualKeyset : (IKeySet)EmptySingleKeyset;
            else
                return keys;
        }

        /// <summary>
        /// Get the key set for a dual scheme. This set should not be modified.
        /// </summary>
        /// <param name="scheme">The scheme.</param>
        /// <returns>The key set for the scheme. Will be empty if no keys are defined for the scheme.</returns>
        public DualKeySet KeysetFor(DualKeyScheme scheme)
        {
            IKeySet keys;

            if (!keySets.TryGetValue(scheme, out keys))
                return EmptyDualKeyset;
            else
                return (DualKeySet)keys;
        }

        /// <summary>
        /// Get the key set for a single scheme. This set should not be modified.
        /// </summary>
        /// <param name="scheme">The scheme.</param>
        /// <returns>The key set for the scheme. Will be empty if no keys are defined for the scheme.</returns>
        public SingleKeySet KeysetFor(SingleKeyScheme scheme)
        {            
            IKeySet keys;

            if (!keySets.TryGetValue(scheme, out keys))
                return EmptySingleKeyset;
            else
                return (SingleKeySet)keys;
        }

        /// <summary>
        /// Lookup/create a key set for a scheme.
        /// </summary>
        /// <param name="scheme"></param>
        /// <returns></returns>
        private IKeySet NewKeysetFor(KeyScheme scheme)
        {
            if (scheme.IsDual())
                return NewKeysetFor((DualKeyScheme)scheme);
            else
                return NewKeysetFor((SingleKeyScheme)scheme);
        }

        /// <summary>
        /// Lookup/create a key set for a single key scheme.
        /// </summary>
        /// <param name="scheme"></param>
        /// <returns></returns>
        private SingleKeySet NewKeysetFor(SingleKeyScheme scheme)
        {
            IKeySet keys;

            if (!keySets.TryGetValue(scheme, out keys))
            {
                keys = new SingleKeySet();

                keySets.Add(scheme, keys);
            }

            return (SingleKeySet)keys;
        }

        /// <summary>
        /// Lookup/create a key set for a single key scheme.
        /// </summary>
        /// <param name="scheme"></param>
        /// <returns></returns>
        private DualKeySet NewKeysetFor(DualKeyScheme scheme)
        {
            IKeySet keys;

            if (!keySets.TryGetValue(scheme, out keys))
            {
                keys = new DualKeySet();

                keySets.Add(scheme, keys);
            }

            return (DualKeySet)keys;
        }

        /// <summary>
        /// Test whether a given key collection matches this one for the
        /// purpose of notification delivery.
        /// </summary>
        /// <param name="producerKeys">The producer keys to match against this (consumer) key collection.</param>
        /// <returns>True if a consumer using this key collection could
        /// receive a notification from a producer with the given
        /// producer key collection.</returns>
        public bool Match(Keys producerKeys)
        {
            if (IsEmpty || producerKeys.IsEmpty)
                return false;

            foreach (var entry in producerKeys.keySets)
            {
                KeyScheme scheme = entry.Key;
                IKeySet keyset = entry.Value;

                if (keySets.ContainsKey(scheme) &&
                    scheme.Match(keyset, keySets[scheme]))
                {
                    return true;
                }
            }

            return false;
        }

        public void Encode(Stream outStream)
        {
            // number of key schemes in the list
            using (BinWriter w = new BinWriter(outStream))
            {
                w.Write(keySets.Count);
            }

            foreach (var entry in keySets)
            {
                KeyScheme scheme = entry.Key;
                IKeySet keySet = entry.Value;

                // scheme ID
                using (BinWriter w = new BinWriter(outStream))
                {
                    w.Write(scheme.Id);
                }


                if (scheme.IsDual())
                {
                    DualKeySet dualKeySet = (DualKeySet)keySet;

                    using (BinWriter w = new BinWriter(outStream))
                    {
                        w.Write(2);
                    }

                    EncodeKeys(outStream, dualKeySet.ProducerKeys);
                    EncodeKeys(outStream, dualKeySet.ConsumerKeys);
                }
                else
                {
                    using (BinWriter w = new BinWriter(outStream))
                    {
                        w.Write(1);
                    }

                    EncodeKeys(outStream, (SingleKeySet)keySet);
                }
            }
        }

        public static Keys Decode(Stream inStream)
        {
            int length;

            using (BinReader r = new BinReader(inStream))
            {
                length = r.ReadInt32();
            }

            if (length == 0)
                return EmptyKeys;

            try
            {
                Keys keys = new Keys();

                for (; length > 0; length--)
                {
                    using (BinReader r = new BinReader(inStream))
                    {
                        KeyScheme scheme = KeyScheme.SchemeFor(r.ReadInt32());
                        int keySetCount = r.ReadInt32();

                        if (scheme.IsDual())
                        {
                            if (keySetCount != 2)
                                throw new ProtocolCodecException("Dual key scheme with " + keySetCount + " key sets");

                            DualKeySet keyset = keys.NewKeysetFor((DualKeyScheme)scheme);

                            DecodeKeys(inStream, keyset.ProducerKeys);
                            DecodeKeys(inStream, keyset.ConsumerKeys);
                        }
                        else
                        {
                            if (keySetCount != 1)
                                throw new ProtocolCodecException
                                  ("Single key scheme with " + keySetCount + " key sets");

                            DecodeKeys(inStream, keys.NewKeysetFor((SingleKeyScheme)scheme));
                        }
                    }
                }

                return keys;
            }
            catch (ArgumentException ex)
            {
                // most likely an invalid KeyScheme ID
                throw new ProtocolCodecException("Could not decode keys.", ex);
            }
        }

        private static void EncodeKeys(Stream outStream, ISet<Key> keys)
        {
            using (BinWriter w = new BinWriter(outStream))
            {
                w.Write(keys.Count);
            }

            foreach (Key key in keys)
                XdrCoding.putBytes(outStream, key.Data);
        }

        private static void DecodeKeys(Stream inStream, ISet<Key> keys)
        {
            int len;
            using (BinReader r = new BinReader(inStream))
            {
                len = r.ReadInt32();
            }

            for (int keysetCount = len; keysetCount > 0; keysetCount--)
                keys.Add(new Key(XdrCoding.getBytes(inStream)));
        }

        public override bool Equals(object obj)
        {
            return obj is Keys && Equals((Keys)obj);
        }

        public bool Equals(Keys keys)
        {
            if (keySets.Count != keys.keySets.Count)
                return false;

            foreach (KeyScheme scheme in keys.keySets.Keys)
            {
                if (!KeysetFor(scheme).Equals(keys.KeysetFor(scheme)))
                    return false;
            }

            return true;
        }

        public override int GetHashCode()
        {
            // todo opt get a better hash code?
            int hash = 0;

            foreach (KeyScheme scheme in keySets.Keys)
                hash ^= 1 << scheme.Id;

            return hash;
        }


    }
}
