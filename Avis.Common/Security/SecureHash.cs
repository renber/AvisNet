using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

namespace Avis.Security
{
    public enum SecureHash
    {
        SHA1
    }

    public static class SecureHashExtensions
    {
        public static byte[] Hash(this SecureHash sh, byte[] input)
        {
            switch (sh)
            {
                case SecureHash.SHA1:
                    // compute SHA1 hash
                    // this is the functionality which was in org.avis.security.SHA1 (so this file had not to be ported)
                    var sha1 = new SHA1CryptoServiceProvider();
                    return sha1.ComputeHash(input);                    
                default:
                    throw new InvalidEnumArgumentException();
            }
        }
    }
}
