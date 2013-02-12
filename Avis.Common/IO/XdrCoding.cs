using Avis.Exceptions;
using Avis.Immigrated;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Avis.IO
{
    /// <summary>
    /// Encoding/decoding helpers for the Elvin XDR wire format.
    /// </summary>
    public static class XdrCoding
    {

        /**
   * Type codes from client protocol spec.
   *  enum {
   *       int32_tc  = 1,
   *       int64_tc  = 2,
   *       real64_tc = 3,
   *       string_tc = 4,
   *       opaque_tc = 5
   *   } value_typecode;
   */
        public const int TYPE_INT32 = 1;
        public const int TYPE_INT64 = 2;
        public const int TYPE_REAL64 = 3;
        public const int TYPE_STRING = 4;
        public const int TYPE_OPAQUE = 5;

        private static readonly byte[] EmptyBytes = new byte[0];

        public static byte[] ToUTF8(String s)
        {
            try
            {
                if (s.Length == 0)
                    return EmptyBytes;
                else
                    return Encoding.UTF8.GetBytes(s);
            }
            catch (EncoderFallbackException ex)
            {
                // shouldn't be possible to get an error encoding from UTF-16 to UTF-8.
                throw new Exception("Internal error", ex);
            }
        }
        
        /// <summary>
        /// Turn a UTF-8 byte array into a string.
        /// </summary>
        /// <param name="utf8Bytes">The bytes.</param>
        /// <param name="offset">The offset into bytes.</param>
        /// <param name="length"> The number of bytes to use.</param>
        /// <returns>The string.</returns>
        /// <exception cref="DecoderFallbackException">if the bytes do not represent a UTF-8 string.</exception>
        public static String FromUTF8(byte[] utf8Bytes, int offset, int length)
        {
            if (utf8Bytes.Length == 0)
                return "";
            else
                return Encoding.UTF8.GetString(utf8Bytes, offset, length);
        }

        /// <summary>
        /// Read a length-delimited 4-byte-aligned UTF-8 string.
        /// </summary>
        /// <param name="inStream"></param>
        /// <returns></returns>
        public static String getString(Stream inStream)
        {
            try
            {
                int length = getPositiveInt(inStream);

                if (length == 0)
                {
                    return "";
                }
                else
                {
                    String s;
                    using (BinReader r = new BinReader(inStream))
                    {
                        byte[] b = r.ReadBytes(length);
                        s = FromUTF8(b, 0, length);
                    }

                    inStream.Seek(paddingFor(length), SeekOrigin.Current);
                    return s;
                }
            }
            catch (Exception ex)
            {
                throw new ProtocolCodecException("Invalid UTF-8 string", ex);
            }
        }

        /// <summary>
        /// Write a length-delimited 4-byte-aligned UTF-8 string.
        /// </summary>
        /// <param name="outStream"></param>
        /// <param name="s"></param>
        public static void putString(Stream outStream, String s)
        {
            try
            {
                if (s.Length == 0)
                {
                    using (BinWriter w = new BinWriter(outStream))
                    {
                        w.Write(0);
                    }
                }
                else
                {
                    byte[] b = Encoding.UTF8.GetBytes(s);

                    using (BinWriter w = new BinWriter(outStream))
                    {
                        // write length
                        w.Write(b.Length);
                        // write string
                        w.Write(b);
                    }

                    putPadding(outStream, b.Length);
                }
            }
            catch (Exception ex)
            {
                // shouldn't be possible to get an error encoding from UTF-16 to UTF-8.
                throw new Exception("Internal error", ex);
            }
        }

        /// <summary>
        /// Generate null padding to 4-byte pad out a block of a given length
        /// </summary>
        /// <param name="outStream"></param>
        /// <param name="length"></param>
        public static void putPadding(Stream outStream, int length)
        {
            byte[] b = new byte[] { 0 };
            for (int count = paddingFor(length); count > 0; count--)
                outStream.Write(b, 0, 1);
        }

        /// <summary>
        /// Calculate the padding needed for the size of a block of bytes to
        /// be a multiple of 4.
        /// </summary>
        /// <param name="length"></param>
        /// <returns></returns>
        public static int paddingFor(int length)
        {
            // tricky eh? this is equivalent to (4 - length % 4) % 4
            return (4 - (length & 3)) & 3;
        }

        /// <summary>
        /// Write a name/value set.
        /// </summary>
        /// <param name="?"></param>
        public static void putNameValues(Stream outStream, Dictionary<String, Object> nameValues)
        {
            using (BinWriter w = new BinWriter(outStream))
            {
                w.Write(nameValues.Count);
                foreach (var entry in nameValues)
                {
                    putString(outStream, entry.Key);
                    putObject(outStream, entry.Value);
                }
            }
        }

        /// <summary>
        ///  Read a name/value set.
        /// </summary>
        /// <param name="inStream"></param>
        /// <returns></returns>
        public static Dictionary<String, Object> getNameValues(Stream inStream)
        {
            int pairs = getPositiveInt(inStream);

            if (pairs == 0)
                return new Dictionary<string, object>();

            Dictionary<String, Object> nameValues = new Dictionary<String, Object>();

            for (; pairs > 0; pairs--)
                nameValues.Add(getString(inStream), getObject(inStream));

            return nameValues;
        }

        public static void putObjects(Stream outStream, Object[] objects)
        {
            using (BinWriter w = new BinWriter(outStream))
            {
                w.Write(objects.Length);
            }

            foreach (Object o in objects)
                putObject(outStream, o);
        }

        public static Object[] getObjects(Stream inStream)
        {
            Object[] objects = new Object[getPositiveInt(inStream)];

            for (int i = 0; i < objects.Length; i++)
                objects[i] = getObject(inStream);

            return objects;
        }

        /// <summary>
        /// Put an object value in type_id/value format.
        /// </summary>
        /// <param name="outStream"></param>
        /// <param name="value"></param>
        public static void putObject(Stream outStream, Object value)
        {
            using (BinWriter w = new BinWriter(outStream))
            {
                if (value is String)
                {
                    w.Write(TYPE_STRING);
                    putString(outStream, (String)value);
                }
                else if (value is int)
                {
                    w.Write(TYPE_INT32);
                    w.Write((int)value);
                }
                else if (value is long)
                {
                    w.Write(TYPE_INT64);
                    w.Write((long)value);
                }
                else if (value is double)
                {
                    w.Write(TYPE_REAL64);
                    w.Write((double)value);
                }
                else if (value is byte[])
                {
                    w.Write(TYPE_OPAQUE);
                    putBytes(outStream, (byte[])value);                    
                }
                else if (value == null)
                {
                    throw new ArgumentException("Value cannot be null");
                }
                else
                {
                    throw new ArgumentException
                      ("Don't know how to encode " + value.GetType().Name);
                }
            }
        }

        /// <summary>
        /// Read an object in type_id/value format.
        /// </summary>
        /// <param name="inStream"></param>
        /// <returns></returns>
        public static Object getObject(Stream inStream)
        {
            using (BinReader r = new BinReader(inStream))
            {
                int type = r.ReadInt32();

                switch (type)
                {
                    case TYPE_INT32:
                        return r.ReadInt32();
                    case TYPE_INT64:
                        return r.ReadInt64();
                    case TYPE_REAL64:
                        return r.ReadDouble();
                    case TYPE_STRING:
                        return getString(inStream);
                    case TYPE_OPAQUE:
                        return getBytes(inStream);
                    default:
                        throw new ProtocolCodecException("Unknown type code: " + type);
                }
            }
        }

        /// <summary>
        /// Write a length-delimited, 4-byte-aligned byte array.
        /// </summary>
        /// <param name="outStream"></param>
        /// <param name="bytes"></param>
        public static void putBytes(Stream outStream, byte[] bytes)
        {
            using (BinWriter w = new BinWriter(outStream))
            {
                w.Write(bytes.Length);
                w.Write(bytes);
            }
            putPadding(outStream, bytes.Length);
        }

        /// <summary>
        /// Read a length-delimited, 4-byte-aligned byte array.
        /// </summary>
        /// <param name="inStream"></param>
        /// <returns></returns>
        public static byte[] getBytes(Stream inStream)
        {
            return getBytes(inStream, getPositiveInt(inStream));
        }

        /// <summary>
        /// Read a length-delimited, 4-byte-aligned byte array with a given length.
        /// </summary>
        /// <param name="inStream"></param>
        /// <param name="length"></param>
        /// <returns></returns>
        public static byte[] getBytes(Stream inStream, int length)
        {
            byte[] bytes = new byte[length];
            inStream.Read(bytes, 0, length);
            inStream.Seek(paddingFor(length), SeekOrigin.Current);

            return bytes;
        }

        public static void putBool(Stream outStream, bool value)
        {
            using (BinWriter w = new BinWriter(outStream))
            {
                w.Write(value ? 1 : 0);
            }
        }

        public static bool getBool(Stream inStream)
        {
            using (BinReader r = new BinReader(inStream))
            {
                int value = r.ReadInt32();

                if (value == 0)
                    return false;
                else if (value == 1)
                    return true;
                else
                    throw new ProtocolCodecException
                      ("Cannot interpret " + value + " as boolean");
            }
        }

        /// <summary>
        /// Read a length-demlimited array of longs.
        /// </summary>
        /// <param name="inStream"></param>
        /// <returns></returns>
        public static long[] getLongArray(Stream inStream)
        {
            long[] longs = new long[getPositiveInt(inStream)];

            using (BinReader r = new BinReader(inStream))
            {
                for (int i = 0; i < longs.Length; i++)
                    longs[i] = r.ReadInt64();
            }

            return longs;
        }

        /// <summary>
        /// Write a length-delimted array of longs.
        /// </summary>
        /// <param name="outStream"></param>
        /// <param name="longs"></param>
        public static void putLongArray(Stream outStream, long[] longs)
        {
            using (BinWriter w = new BinWriter(outStream))
            {
                w.Write(longs.Length);

                foreach (long l in longs)
                    w.Write(l);
            }
        }

        /// <summary>
        /// Read a length-demlimited array of strings.
        /// </summary>
        /// <param name="inStream"></param>
        /// <returns></returns>
        public static String[] getStringArray(Stream inStream)
        {
            String[] strings = new String[getPositiveInt(inStream)];

            for (int i = 0; i < strings.Length; i++)
                strings[i] = getString(inStream);

            return strings;
        }

        /// <summary>
        /// Write a length-delimted array of strings.
        /// </summary>
        /// <param name="outStream"></param>
        /// <param name="strings"></param>
        public static void putStringArray(Stream outStream, String[] strings)
        {
            using (BinWriter w = new BinWriter(outStream))
            {
                w.Write(strings.Length);
            }

            foreach (String s in strings)
                putString(outStream, s);
        }

        /// <summary>
        /// Read an int >= 0 or generate an exception.
        /// </summary>
        /// <param name="inStream"></param>
        /// <returns></returns>
        private static int getPositiveInt(Stream inStream)
        {
            using (BinReader r = new BinReader(inStream))
            {
                int value = r.ReadInt32();

                if (value >= 0)
                    return value;
                else
                    throw new ProtocolCodecException("Length cannot be negative: " + value);
            }
        }
    }
}
