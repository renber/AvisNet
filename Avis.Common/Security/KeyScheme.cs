using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Avis.Security
{
    /*   An enumeration of supported Elvin security schemes. A key scheme
    * defines a mode of sending or receiving notifications securely.
    * 
    * <h3>The Producer Scheme</h3>
    * 
    * In the producer scheme, consumers of notifications ensure that a
    * notification producer is known to them. The producer uses the
    * private key, and consumers use the public key. If the producer
    * keeps its private key secure, consumers can be assured they are
    * receiving notifications from a trusted producer.
    * 
    * <h3>The Consumer Scheme</h3>
    * 
    * In the consumer scheme, producers of notifications ensure that a
    * notification consumer is known to them, i.e. the producer controls
    * who can receive its notifications. In this scheme -- the reverse of
    * the producer scheme -- the consumer uses the private key, and
    * producers use the public key. If the consumer keeps its private key
    * secure, then the producer can be assured that only the trusted
    * consumer can receive its notifications.
    * 
    * <h3>The Dual Scheme</h3>
    * 
    * The dual scheme combines both the producer and consumer schemes, so
    * that both ends can send and receive securely. Typically both ends
    * exchange public keys, and each end then emits notifications with
    * both its private key and the public key(s) of its intended
    * consumer(s) attached. Similarly, each end would subscribe using its
    * private key and the public key(s) of its intended producer(s).
    * 
    * <h3>Avis Key Scheme API</h3>
    * 
    * The Elvin Producer and Consumer schemes both use a single set of
    * keys, whereas the Dual scheme requires both a consumer key set and
    * a producer key set. The schemes that require a single set of keys
    * are defined by an instance of {@link SingleKeyScheme}, the Dual
    * scheme is defined by an instance of {@link DualKeyScheme}.
    * <p>
    * Each key scheme also defines a {@link #keyHash secure hash} for
    * generating its public keys: see the documentation on
    * {@linkplain Key security keys} for more information on public and
    * private keys used in key schemes.
    * 
    * <h3>Supported Schemes</h3>
    * 
    * Avis currently supports just the SHA-1 secure hash as defined in
    * version 4.0 of the Elvin protocol. As such, three schemes are
    * available: {@link #SHA1_CONSUMER SHA1-Consumer},
    * {@link #SHA1_PRODUCER SHA1-Producer} and
    * {@link #SHA1_DUAL SHA1-Dual}.*/

    /// <summary>
    /// An enumeration of supported Elvin security schemes. A key scheme
    /// defines a mode of sending or receiving notifications securely.
    /// </summary>
    public abstract class KeyScheme
    {
        /// <summary>
        /// The SHA-1 Dual key scheme.
        /// </summary>
        public static readonly DualKeyScheme Sha1Dual = new DualKeyScheme(1, SecureHash.SHA1);

        /// <summary>
        /// The SHA-1 Producer key scheme.
        /// </summary>
        public static readonly SingleKeyScheme Sha1Producer = new SingleKeyScheme(2, SecureHash.SHA1, true, false);

        /// <summary>
        /// The SHA-1 Consumer key scheme.
        /// </summary>
        public static readonly SingleKeyScheme Sha1Consumer = new SingleKeyScheme(3, SecureHash.SHA1, false, true);

        private static readonly ISet<KeyScheme> _schemes = new HashSet<KeyScheme>() { Sha1Consumer, Sha1Producer, Sha1Dual };

        /// <summary>
        /// The unique ID of the scheme. This is the same as the on-the-wire
        /// ID used by Elvin.
        /// </summary>
        public readonly int Id;

        /// <summary>
        /// True if this scheme is a producer scheme.
        /// </summary>
        public readonly bool Producer;

        /// <summary>
        /// True of this scheme is a consumer scheme.
        /// </summary>
        public readonly bool Consumer;

        /// <summary>
        /// The secure hash used in this scheme.
        /// </summary>
        public SecureHash KeyHash;

        /// <summary>
        /// The unique, human-readable name of this scheme.
        /// </summary>
        public readonly String name;

        /// <summary>
        /// The set of all supported schemes.
        /// </summary>
        /// <returns></returns>
        public static ISet<KeyScheme> Schemes
        {
            get
            {
                return _schemes;
            }            
        }

        public KeyScheme(int id, SecureHash keyHash, bool producer, bool consumer)
        {
            this.Id = id;
            this.Producer = producer;
            this.Consumer = consumer;
            this.KeyHash = keyHash;
            this.name = CreateName();
        }

        /**
         * True if the scheme requires dual key sets.
         */
        public bool IsDual()
        {
            return Producer && Consumer;
        }

        /**
         * Create the public (aka prime) key for a given private (aka raw)
         * key using this scheme's hash.
         */
        public Key PublicKeyFor(Key privateKey)
        {
            return new Key(KeyHash.Hash(privateKey.Data));
        }

        /**
         * Match a producer/consumer keyset in the current scheme.
         * 
         * @param producerKeys The producer keys.
         * @param consumerKeys The consumer keys.
         * @return True if a consumer using consumerKeys could receive a
         *         notification from a producer with producerKeys in this
         *         scheme.
         */
        public bool Match(IKeySet producerKeys, IKeySet consumerKeys)
        {
            if (IsDual())
            {
                DualKeySet keys1 = (DualKeySet)producerKeys;
                DualKeySet keys2 = (DualKeySet)consumerKeys;

                return MatchKeys(keys1.ProducerKeys, keys2.ProducerKeys) &&
                       MatchKeys(keys2.ConsumerKeys, keys1.ConsumerKeys);

            }
            else if (Producer)
            {
                return MatchKeys((SingleKeySet)producerKeys,
                                  (SingleKeySet)consumerKeys);
            }
            else
            {
                return MatchKeys((SingleKeySet)consumerKeys,
                                  (SingleKeySet)producerKeys);
            }
        }

        /**
         * Match a set of private keys with a set of public keys.
         * 
         * @param privateKeys A set of private (aka raw) keys.
         * @param publicKeys A set of public (aka prime) keys.
         * @return True if at least one private key mapped to its public
         *         version (using this scheme's hash) was in the given
         *         public key set.
         */
        private bool MatchKeys(ISet<Key> privateKeys, ISet<Key> publicKeys)
        {
            foreach (Key privateKey in privateKeys)
            {
                if (publicKeys.Contains(PublicKeyFor(privateKey)))
                    return true;
            }

            return false;
        }

        public override bool Equals(object obj)
        {
            return obj == this;
        }

        public override int GetHashCode()
        {
            return Id;
        }

        public override string ToString()
        {
            return name;
        }

        private String CreateName()
        {
            StringBuilder str = new StringBuilder();

            str.Append(KeyHash.ToString()).Append('-');

            if (IsDual())
                str.Append("dual");
            else if (Producer)
                str.Append("producer");
            else
                str.Append("consumer");

            return str.ToString();
        }

        /**
         * Look up the scheme for a given ID.
         *
         * @throws IllegalArgumentException if id is not a known scheme ID.
         */
        public static KeyScheme SchemeFor(int id)
        {
            switch (id)
            {
                case 1:
                    return Sha1Dual;
                case 2:
                    return Sha1Producer;
                case 3:
                    return Sha1Consumer;
                default:
                    throw new ArgumentException("Invalid key scheme ID: " + id);
            }
        }       
    }
}
