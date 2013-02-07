using Avis.Exceptions;
using Avis.Immigrated;
using Avis.IO.Messages;
using Avis.Utils;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace Avis.Client
{
    /// <summary>
    /// A notification sent via an Elvin router. A notification is a set of
    /// field name/value pairs. Field values may be one of the following
    /// types:
    ///  <ul>
    /// <li><tt>int     => Elvin Int32</tt>
    /// <li><tt>long    => Elvin Int64</tt>
    /// <li><tt>double  => Elvin Real64</tt>
    /// <li><tt>String  => Elvin String</tt>
    /// <li><tt>byte [] => Elvin Opaque</tt>
    /// </ul>
    /// For efficiency, byte arrays passed in via the set () methods,
    /// constuctors, and clone () are not copied before being added to this
    /// object, nor are they copied before being returned by the get ()
    /// methods. Please note that modifying a byte array that is part of a
    /// notification can cause undefined behaviour: treat all values of a
    /// notification as immutable.
    /// </summary>
    public class Notification : IEnumerable<KeyValuePair<String, Object>>
    {
        Dictionary<String, Object> attributes = new Dictionary<String, Object>();

        public Dictionary<String, Object> Attributes
        {
            get
            {
                return attributes;
            }
        }

        public bool IsEmpty
        {
            get
            {
                return attributes.Count == 0;
            }
        }

        public int Count
        {
            get
            {
                return attributes.Count;
            }
        }

        /// <summary>
        /// Create an empty notification.
        /// </summary>
        public Notification()
        {

        }

        internal Notification(NotifyDeliver message)
        {
            this.attributes = message.attributes;
        }

        /// <summary>
        /// Create a notification from an array of name/value pairs.
        /// </summary>
        /// <exception cref="ArgumentException">if attributes do not represent a valid notification</exception>
        public Notification(params Object[] attributes)
        {
            if (attributes.Length % 2 != 0)
                throw new ArgumentException("Attributes must be a list of name/value pairs");

            for (int i = 0; i < attributes.Length; i += 2)
                Set(CheckField(attributes[i]), attributes[i + 1]);
        }

        public Notification(Dictionary<String, Object> map)
        {
            SetAll(map);
        }

        public Notification(String ntfnExpr)
        {
            using (MemoryStream ms = new MemoryStream(Encoding.UTF8.GetBytes(ntfnExpr), false))
            {
                parse(this, ms);
            }
        }

        /**
   * Create an instance from an encoded notification read from a stream.
   * See {@link #parse(Notification, Reader)}.
   */
        public Notification(Stream inStream)
        {
            parse(this, inStream);
        }


        /**
        * Parse an expression representing a notification and populate the
        * given notification with the values. The format of this expression
        * is compatible with that used by the <code>ec</code> and
        * <code>ep</code> utilities. For example:
        * 
        * <pre>
        *   An-Int32: 42
        *   An-Int64: 24L
        *   A-Real64: 3.14
        *   String:   &quot;String with a \&quot; in it&quot;
        *   Opaque:   [01 02 0f ff]
        *   A field with a \: in it: 1
        * </pre>
        * 
        * The parser ignores lines starting with "$" and stops on end of
        * stream or "---".
        * 
        * @param ntfn The notification to add values to.
        * @param in The source to read the expression from.
        * @throws IOException If reader throws an IO exception.
        * @throws InvalidFormatException If there is an error in the format
        *           of the expression. The notification may contain a
        *           partial set of values already successfully read.
        */
        /// <summary>
        /// Parse an expression representing a notification and populate the
        /// given notification with the values.
        /// </summary>
        /// <param name="ntfn"></param>
        /// <param name="inStream"></param>
        /// <exception cref="InvalidFormatException"></exception>
        public static void parse(Notification ntfn, Stream inStream)
        {
            using (StreamReader reader = new StreamReader(inStream))
            {
                String line = null;
                while (!reader.EndOfStream)
                {
                    line = reader.ReadLine();

                    if (line.StartsWith("$"))
                        continue; // skip this line

                    if (line.Trim().StartsWith("---"))
                        break; // notification terminator

                    try
                    {
                        parseLine(ntfn, line);
                    }
                    catch (InvalidFormatException ex)
                    {
                        throw new InvalidFormatException("Notification line \"" + line + "\": " + ex.Message);
                    }
                }
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="ntfn"></param>
        /// <param name="line"></param>
        /// <exception cref="InvalidFormatException"></exception>
        private static void parseLine(Notification ntfn, String line)
        {
            int colon = TextUtils.FindFirstNonEscaped(line, ':');

            if (colon == -1)
                throw new InvalidFormatException("No \":\" separating name and value");
            else if (colon == line.Length - 1)
                throw new InvalidFormatException("Missing value");

            String name = TextUtils.StripBackslashes(line.Substring(0, colon).Trim());
            String valueExpr = line.Substring(colon + 1).Trim();

            ntfn.attributes.Add(name, TextUtils.StringToValue(valueExpr));
        }

        /// <summary>
        /// Remove all attributes from the notification.
        /// </summary>
        public void Clear()
        {
            attributes.Clear();
        }

        /// <summary>
        /// Generate a string value of the notification. The format is
        // compatible with that used by the ec/ep commands.   
        /// </summary>
        /// <returns></returns>
        public override String ToString()
        {
            return TextUtils.FormatNotification(attributes);
        }

        /// <summary>
        /// Copy all values in a map into this notification.
        /// </summary>
        /// <param name="?"></param>
        /// <param name="?"></param>
        public void SetAll<TKey, TValue>(Dictionary<TKey, TValue> map)
        {
            foreach (var entry in map)
                Set(CheckField(entry.Key), entry.Value);
        }

        /// <summary>
        /// Set a field value.
        /// </summary>
        /// <param name="name"></param>
        /// <param name="value"></param>
        public void Set(String name, Object value)
        {
            if (attributes.ContainsKey(name))
                attributes.Remove(name);

            if (value != null)
                attributes.Add(name, CheckValue(value));
        }

        public void Remove(String name)
        {
            attributes.Remove(name);
        }

        /// <summary>
        /// Test if this notification has a field with a given name.
        /// </summary>
        /// <param name="name"></param>
        /// <returns></returns>
        public bool HasField(String name)
        {
            return attributes.ContainsKey(name);
        }

        public IEnumerator<KeyValuePair<string, object>> GetEnumerator()
        {
            return attributes.GetEnumerator();
        }

        System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator()
        {
            return attributes.GetEnumerator();
        }

        public override bool Equals(Object o)
        {
            if (o == this)
                return true;

            if (o is Notification)
                return Equals((Notification)o);
            else
                return false;
        }

        /// <summary>
        /// Compare two notifications.
        /// </summary>
        /// <param name="ntfn"></param>
        /// <returns></returns>
        public bool Equals(Notification ntfn)
        {
            /*
             * NB: Cannot use HashMap.equals () as it does not compare byte
             * arrays by value.
             */

            if (this == ntfn)
            {
                return true;
            }
            else if (attributes.Count != ntfn.attributes.Count)
            {
                return false;
            }
            else
            {
                foreach (var entry in attributes)
                {
                    Object o;
                    if (ntfn.attributes.TryGetValue(entry.Key, out o))
                    {
                        if (!ValuesEqual(entry.Value, o))
                            return false;
                    }
                    else
                        return false;
                }
            }
            return true;
        }

        private static bool ValuesEqual(Object value1, Object value2)
        {
            if (value1 == value2)
                return true;
            else if (value1 == null || value2 == null)
                return false;
            else if (value1.GetType() != value2.GetType())
                return false;
            else if (value1 is byte[] && value2 is byte[])
                return Enumerable.SequenceEqual((byte[])value1, (byte[])value2);
            else
                return value1.Equals(value2);
        }

        public override int GetHashCode()
        {
            int hashCode = 31;

            foreach (var entry in this)
            {
                hashCode ^= entry.Key.GetHashCode();
                if (entry.Value is byte[])
                {
                    hashCode ^= (int)new ArrayHasher().ComputeHash((byte[])entry.Value);
                }
                else
                    hashCode ^= entry.Value.GetHashCode();
            }

            return hashCode;
        }

        private static Object CheckValue(Object value)
        {
            if ((value is String ||
                 value is int ||
                 value is long ||
                 value is double ||
                 value is byte[]))
            {
                return value;
            }
            else
            {
                throw new ArgumentException("Value must be a string, integer, long, double or byte array");
            }
        }

        private static String CheckField(Object field)
        {
            if (field is String)
            {
                return (String)field;
            }
            else
            {
                throw new ArgumentException("Name must be a string: \"" + field + "\"");
            }
        }
    }
}
