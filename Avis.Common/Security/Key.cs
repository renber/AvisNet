using Avis.Immigrated;
using Avis.IO;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Avis.Security
{
/**
 * A key value used to secure notifications. A key is simply an
 * immutable block of bytes.
 * <p>
 * Elvin defines two types of key, <em>private</em> (or <em>raw</em>)
 * keys, and <em>public</em> (or <em>prime</em>) keys. A public
 * key is a one-way hash (e.g. using SHA-1) of a private key. A
 * private key may be any random data, or simply a password. A private
 * key is defined to match a public key if the corresponding hash of
 * its data matches the public key's data, e.g. if
 * <code>sha1 (privateKey.data) == publicKey.data</code>.
 * <p>
 * Note that this is not a public key system in the RSA sense but
 * that, like RSA, public keys can be shared in the open without loss
 * of security.
 * <p>
 * This class precomputes a hash code for the key data to accelerate
 * equals () and hashCode ().
 * 
 */
    
    /// <summary>
    /// A key value used to secure notifications. A key is simply an
    /// immutable block of bytes.
    /// </summary>
    public sealed class Key
    {
        /// <summary>
        /// The key's data block.
        /// </summary>
        public readonly byte[] Data;

        private int hash;

        /// <summary>
        /// Create a key from a password by using the password's UTF-8
        /// representation as the data block.
        /// </summary>
        /// <param name="password"></param>
        public Key(String password)
            : this(XdrCoding.ToUTF8(password))
        {

        }

        /// <summary>
        /// Create a key from a block of data.
        /// </summary>
        /// <param name="data"></param>
        public Key(byte[] data)
        {
            if (data == null || data.Length == 0)
                throw new ArgumentException("Key data cannot be empty");

            this.Data = data;
            this.hash = new ArrayEqualityComparer<byte>().GetHashCode(data);
        }

        /// <summary>
        /// Shortcut to generate the public (prime) key for a given scheme.
        /// </summary>
        /// <param name="scheme"></param>
        /// <returns></returns>
        public Key PublicKeyFor(KeyScheme scheme)
        {
            return scheme.PublicKeyFor(this);
        }

        public override bool Equals(Object o)
        {
            return o is Key && Equals((Key)o);
        }

        public bool Equals(Key key)
        {
            return hash == key.hash && new ArrayEqualityComparer<byte>().Equals(Data, key.Data);
        }

        public override int GetHashCode()
        {
            return hash;
        }

    }
}
