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
    public class Keys
    {

        /** An empty, immutable key collection. */
        public static readonly Keys EMPTY_KEYS = new EmptyKeys();

        private static readonly DualKeySet EMPTY_DUAL_KEYSET = new DualKeySet(true);
        private static readonly SingleKeySet EMPTY_SINGLE_KEYSET = new EmptySingleKeys();

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
            add(keys);
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


        /**
         * Shortcut to efficiently generate a key collection that represents
         * this key collection's union with another.
         * 
         * @param keys The keys to add.
         * 
         * @return If keys is empty, this method will simply return this
         *         collection. If this collection is empty, keys will be
         *         returned. Otherwise a new collection instance is created
         *         as the union of both.
         */
        public Keys addedTo(Keys keys)
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

                newKeys.add(keys);

                return newKeys;
            }
        }

        /**
         * Add a key for single key scheme.
         *  
         * @param scheme The key scheme.
         * @param key The key to add.
         * 
         * @see #remove(SingleKeyScheme, Key)
         */
        public virtual void add(SingleKeyScheme scheme, Key key)
        {
            newKeysetFor(scheme).add(key);
        }

        /**
         * Remove a key for single key scheme.
         *  
         * @param scheme The key scheme.
         * @param key The key to remove.
         * 
         * @see #add(SingleKeyScheme, Key)
         */
        public virtual void remove(SingleKeyScheme scheme, Key key)
        {
            IKeySet keys;

            if (keySets.TryGetValue(scheme, out keys))
            {
                keys.remove(key);

                if (keys.IsEmpty)
                    keySets.Remove(scheme);
            }
        }

        /**
         * Add a key for dual key scheme.
         *  
         * @param scheme The key scheme.
         * @param subset The key subset (PRODUCER or CONSUMER) to add the key to. 
         * @param key The key to add.
         * 
         * @see #remove(DualKeyScheme, org.avis.security.DualKeyScheme.Subset, Key)
         */
        public virtual void add(DualKeyScheme scheme,
                         DualKeyScheme.Subset subset, Key key)
        {
            newKeysetFor(scheme).keysFor(subset).Add(key);
        }

        /**
         * Remove a key for dual key scheme.
         * 
         * @param scheme The key scheme.
         * @param subset The key subset (PRODUCER or CONSUMER) to remove the
         *          key from.
         * @param key The key to remove.
         * 
         * @see #add(DualKeyScheme, org.avis.security.DualKeyScheme.Subset,
         *      Key)
         */
        public virtual void remove(DualKeyScheme scheme,
                            DualKeyScheme.Subset subset,
                            Key key)
        {
            IKeySet keySet;

            if (keySets.TryGetValue(scheme, out keySet))
            {
                ((DualKeySet)keySet).keysFor(subset).Remove(key);

                if (keySet.IsEmpty)
                    keySets.Remove(scheme);
            }
        }

        /**
         * Add all keys in a collection.
         * 
         * @param keys The keys to add.
         * 
         * @see #remove(Keys)
         */
        public virtual void add(Keys keys)
        {
            if (keys == this)
                throw new ArgumentException("Cannot add key collection to itself");

            foreach (KeyScheme scheme in keys.keySets.Keys)
                add(scheme, keys.keySets[scheme]);
        }

        private void add(KeyScheme scheme, IKeySet keys)
        {
            if (!keys.IsEmpty)
                newKeysetFor(scheme).add(keys);
        }

        /**
         * Remove all keys in a collection.
         * 
         * @param keys The keys to remove.
         * 
         * @see #add(Keys)
         */
        public virtual void remove(Keys keys)
        {
            if (keys == this)
                throw new ArgumentException("Cannot remove key collection from itself");

            foreach (KeyScheme scheme in keys.keySets.Keys)
            {
                IKeySet myKeys;

                if (keySets.TryGetValue(scheme, out myKeys))
                {
                    myKeys.remove(keys.keysetFor(scheme));

                    if (myKeys.IsEmpty)
                        keySets.Remove(scheme);
                }
            }
        }

        /**
         * Create a new key collection with some keys added/removed. This
         * does not modify the current collection.
         * 
         * @param toAdd Keys to add.
         * @param toRemove Keys to remove
         * 
         * @return A new key set with keys added/removed. If both add/remove
         *         key sets are empty, this returns the current instance.
         *         
         * @see #deltaFrom(Keys)
         */
        public Keys delta(Keys toAdd, Keys toRemove)
        {
            if (toAdd.IsEmpty && toRemove.IsEmpty)
            {
                return this;
            }
            else
            {
                Keys keys = new Keys(this);

                keys.add(toAdd);
                keys.remove(toRemove);

                return keys;
            }
        }

        /**
         * Compute the changes between one key collection and another.
         * 
         * @param keys The target key collection.
         * @return The delta (i.e. key sets to be added and removed)
         *         required to change this collection into the target.
         * 
         * @see #delta(Keys, Keys)
         */
        public Delta deltaFrom(Keys keys)
        {
            if (keys == this)
                return Delta.EMPTY_DELTA;

            Keys addedKeys = new Keys();
            Keys removedKeys = new Keys();

            foreach (KeyScheme scheme in KeyScheme.schemes())
            {
                IKeySet existingKeyset = keysetFor(scheme);
                IKeySet otherKeyset = keys.keysetFor(scheme);

                addedKeys.add(scheme, otherKeyset.subtract(existingKeyset));
                removedKeys.add(scheme, existingKeyset.subtract(otherKeyset));
            }

            return new Delta(addedKeys, removedKeys);
        }

        /**
         * Get the key set for a given scheme. This set should not be
         * modified.
         * 
         * @param scheme The scheme.
         * @return The key set for the scheme. Will be empty if no keys are
         *         defined for the scheme.
         * 
         * @see #keysetFor(DualKeyScheme)
         * @see #keysetFor(SingleKeyScheme)
         */
        private IKeySet keysetFor(KeyScheme scheme)
        {
            IKeySet keys;

            if (!keySets.TryGetValue(scheme, out keys))
                return scheme.isDual() ? (IKeySet)EMPTY_DUAL_KEYSET : (IKeySet)EMPTY_SINGLE_KEYSET;
            else
                return keys;
        }

        /**
         * Get the key set for a dual scheme. This set should not be
         * modified.
         * 
         * @param scheme The scheme.
         * @return The key set for the scheme. Will be empty if no keys are
         *         defined for the scheme.
         * 
         * @see #keysetFor(KeyScheme)
         * @see #keysetFor(SingleKeyScheme)
         */
        public DualKeySet keysetFor(DualKeyScheme scheme)
        {
            IKeySet keys;

            if (!keySets.TryGetValue(scheme, out keys))
                return EMPTY_DUAL_KEYSET;
            else
                return (DualKeySet)keys;
        }

        /**
         * Get the key set for a single scheme. This set should not be
         * modified.
         * 
         * @param scheme The scheme.
         * @return The key set for the scheme. Will be empty if no keys are
         *         defined for the scheme.
         *         
         * @see #keysetFor(KeyScheme)
         * @see #keysetFor(DualKeyScheme)
         */
        public SingleKeySet keysetFor(SingleKeyScheme scheme)
        {            
            IKeySet keys;

            if (!keySets.TryGetValue(scheme, out keys))
                return EMPTY_SINGLE_KEYSET;
            else
                return (SingleKeySet)keys;
        }

        /**
         * Lookup/create a key set for a scheme.
         */
        private IKeySet newKeysetFor(KeyScheme scheme)
        {
            if (scheme.isDual())
                return newKeysetFor((DualKeyScheme)scheme);
            else
                return newKeysetFor((SingleKeyScheme)scheme);
        }

        /**
         * Lookup/create a key set for a single key scheme.
         */
        private SingleKeySet newKeysetFor(SingleKeyScheme scheme)
        {
            IKeySet keys;

            if (!keySets.TryGetValue(scheme, out keys))
            {
                keys = new SingleKeySet();

                keySets.Add(scheme, keys);
            }

            return (SingleKeySet)keys;
        }

        /**
         * Lookup/create a key set for a single key scheme.
         */
        private DualKeySet newKeysetFor(DualKeyScheme scheme)
        {
            IKeySet keys;

            if (!keySets.TryGetValue(scheme, out keys))
            {
                keys = new DualKeySet();

                keySets.Add(scheme, keys);
            }

            return (DualKeySet)keys;
        }

        /**
         * Test whether a given key collection matches this one for the
         * purpose of notification delivery.
         * 
         * @param producerKeys The producer keys to match against this
         *          (consumer) key collection.
         * @return True if a consumer using this key collection could
         *         receive a notification from a producer with the given
         *         producer key collection.
         */
        public bool match(Keys producerKeys)
        {
            if (IsEmpty || producerKeys.IsEmpty)
                return false;

            foreach (var entry in producerKeys.keySets)
            {
                KeyScheme scheme = entry.Key;
                IKeySet keyset = entry.Value;

                if (keySets.ContainsKey(scheme) &&
                    scheme.match(keyset, keySets[scheme]))
                {
                    return true;
                }
            }

            return false;
        }

        public void encode(Stream outStream)
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
                    w.Write(scheme.id);
                }


                if (scheme.isDual())
                {
                    DualKeySet dualKeySet = (DualKeySet)keySet;

                    using (BinWriter w = new BinWriter(outStream))
                    {
                        w.Write(2);
                    }

                    encodeKeys(outStream, dualKeySet.producerKeys);
                    encodeKeys(outStream, dualKeySet.consumerKeys);
                }
                else
                {
                    using (BinWriter w = new BinWriter(outStream))
                    {
                        w.Write(1);
                    }

                    encodeKeys(outStream, (SingleKeySet)keySet);
                }
            }
        }

        public static Keys decode(Stream inStream)
        {
            int length;

            using (BinReader r = new BinReader(inStream))
            {
                length = r.ReadInt32();
            }

            if (length == 0)
                return EMPTY_KEYS;

            try
            {
                Keys keys = new Keys();

                for (; length > 0; length--)
                {
                    using (BinReader r = new BinReader(inStream))
                    {
                        KeyScheme scheme = KeyScheme.schemeFor(r.ReadInt32());
                        int keySetCount = r.ReadInt32();

                        if (scheme.isDual())
                        {
                            if (keySetCount != 2)
                                throw new ProtocolCodecException("Dual key scheme with " + keySetCount + " key sets");

                            DualKeySet keyset = keys.newKeysetFor((DualKeyScheme)scheme);

                            decodeKeys(inStream, keyset.producerKeys);
                            decodeKeys(inStream, keyset.consumerKeys);
                        }
                        else
                        {
                            if (keySetCount != 1)
                                throw new ProtocolCodecException
                                  ("Single key scheme with " + keySetCount + " key sets");

                            decodeKeys(inStream, keys.newKeysetFor((SingleKeyScheme)scheme));
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

        private static void encodeKeys(Stream outStream, ISet<Key> keys)
        {
            using (BinWriter w = new BinWriter(outStream))
            {
                w.Write(keys.Count);
            }

            foreach (Key key in keys)
                XdrCoding.putBytes(outStream, key.data);
        }

        private static void decodeKeys(Stream inStream, ISet<Key> keys)
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
                if (!keysetFor(scheme).Equals(keys.keysetFor(scheme)))
                    return false;
            }

            return true;
        }

        public override int GetHashCode()
        {
            // todo opt get a better hash code?
            int hash = 0;

            foreach (KeyScheme scheme in keySets.Keys)
                hash ^= 1 << scheme.id;

            return hash;
        }


    }
}
