using Avis.IO;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Avis.Client
{
    /**
 * Connection options sent by the client to the server.
 * <p>
 * 
 * In addition to the options sent by the client to the router
 * detailed below, the router may also add a "Vendor-Identification"
 * option to its reply with a string identifying the router
 * implementation, e.g. "Avis 1.2" or "elvind 4.4.0".
 * 
 * <h2>Standard Elvin Connection Options</h2>
 * 
 * <p>
 * See http://elvin.org/specs for details on client connection
 * options.
 * </p>
 * 
 * <dl>
 * <dt>Packet.Max-Length</dt>
 * <dd>Max packet length acceptable from a client.</dd>
 * 
 * <dt>Subscription.Max-Count</dt>
 * <dd>Maximum number of subscriptions allowed by a single client.</dd>
 * 
 * <dt>Subscription.Max-Length</dt>
 * <dd>Maximum length, in bytes, of any subscription expression.</dd>
 * 
 * <dt>Receive-Queue.Max-Length</dt>
 * <dd>The maximum size of the router's per-client incoming packet
 * queue, in bytes. If the queue exceeds this size, the router will
 * throttle the data stream from the client until the queue drops
 * below this value.</dd>
 * 
 * <dt>TCP.Send-Immediately</dt>
 * <dd>Set whether the TCP NO_DELAY flag is enabled for sockets on
 * the server side. 1 = send immediately (TCP NO_DELAY = true), 0 = do
 * not necessarily send immediately, buffer data for optimal
 * throughput (TCP NO_DELAY = false). Set this to 1 if you experience
 * lag with "real time" applications that require minimal delivery
 * latency, but note that this may result in an overall reduction in
 * throughput.</dd>
 * 
 * <dt>Attribute.Name.Max-Length</dt>
 * <dd>The maximum length, in bytes, of an attribute name.</dd>
 * 
 * <dt>Attribute.Max-Count</dt>
 * <dd>The maximum number of attributes on a notification.</dd>
 * 
 * <dt>Attribute.Opaque.Max-Length</dt>
 * <dd>Maximum length, in bytes, for opaque values.</dd>
 * 
 * <dt>Attribute.String.Max-Length</dt>
 * <dd>Maximum length, in bytes, for opaque values. Note that this
 * value is not the number of characters: some characters may take up
 * to 5 bytes to represent using the required UTF-8 encoding.</dd>
 * 
 * <dt>Receive-Queue.Drop-Policy</dt>
 * <dd>This property describes the desired behaviour of the router's
 * packet receive queue if it exceeds the negotitated maximum size.
 * Values: "oldest", "newest", "largest", "fail"</dd>
 * 
 * <dt>Send-Queue.Drop-Policy</dt>
 * <dd>This property describes the desired behaviour of the router's
 * packet send queue if it exceeds the negotitated maximum size.
 * Values: "oldest", "newest", "largest", "fail"</dd>
 * 
 * <dt>Send-Queue.Max-Length</dt>
 * <dd>The maximum length (in bytes) of the routers send queue.</dd>
 * </dl>
 * 
 * @author Matthew Phillips
 */
    public class ConnectionOptions
    {

        public static readonly ConnectionOptions EMPTY_OPTIONS = new ConnectionOptions(new Dictionary<String, Object>());

        public Dictionary<String, Object> values { get; private set; }
        public bool includeLegacy { get; set; }

        /// <summary>
        /// Create an empty instance.
        /// </summary>
        public ConnectionOptions()
            : this(new Dictionary<String, Object>())
        {

        }

        protected ConnectionOptions(Dictionary<String, Object> values)
        {
            this.values = values;
            this.includeLegacy = true;
        }



        /// <summary>
        /// Generate a dictionary view of this connection option set, automatically
        /// adding legacy connection options as required.
        /// </summary>
        /// <returns></returns>

        public Dictionary<String, Object> asMapWithLegacy()
        {
            if (values.Count == 0)
                return new Dictionary<string, object>();

            Dictionary<String, Object> options = new Dictionary<String, Object>();

            foreach (var entry in values)
            {
                if (includeLegacy)
                {
                    Object value = entry.Value;

                    /*
                     * TCP.Send-Immediately maps to router.coalesce-delay which
                     * has opposite meaning.
                     */
                    if (entry.Key == "TCP.Send-Immediately")
                        value = value.Equals(0) ? 1 : 0;

                    options.Add(LegacyConnectionOptions.NewToLegacy(entry.Key), value);
                }

                options.Add(entry.Key, entry.Value);
            }

            return options;
        }

        /**
   * Convert options that may contain legacy settings to new-style
   * ones as required.
   * 
   * @param legacyOptions input options.
   * @return A set of options with any legacy options mapped to the
   *         new style ones.
   */
        public static Dictionary<String, Object> convertLegacyToNew
          (Dictionary<String, Object> legacyOptions)
        {
            if (legacyOptions.Count == 0)
                return new Dictionary<String, Object>();

            Dictionary<String, Object> options = new Dictionary<String, Object>();

            foreach (var entry in legacyOptions)
            {
                Object value = entry.Value;

                /*
                 * router.coalesce-delay maps to TCP.Send-Immediately which has
                 * opposite meaning.
                 */
                if (entry.Key == "router.coalesce-delay")
                    value = value.Equals(0) ? 1 : 0;

                string s = LegacyConnectionOptions.LegacyToNew(entry.Key);
                if (!options.ContainsKey(s))
                {
                    options.Add(s, value);
                }
            }

            return options;
        }

        /// <summary>
        /// Generate the difference between this option set and an actual set
        /// returned by the server.
        /// </summary>
        /// <param name="options"></param>
        /// <returns></returns>
        public Dictionary<String, Object> differenceFrom(Dictionary<String, Object> options)
        {
            Dictionary<String, Object> diff = new Dictionary<String, Object>();

            foreach (var entry in values)
            {
                Object actualValue = options[entry.Key];

                if (!entry.Value.Equals(actualValue))
                    diff.Add(entry.Key, actualValue);
            }

            return diff;
        }

        /// <summary>
        /// Set a connection option.
        /// </summary>
        /// <param name="name">The option name.</param>
        /// <param name="value">The value. Must be a string or a number. Use null to clear.</param>
        /// <exception cref="ArgumentException">value is not a String or Integer</exception>
        public void set(String name, Object value)
        {
            if (value == null)
            {
                values.Remove(name);
            }
            else if (value is String || value is int)
            {
                if (values.ContainsKey(name))
                    values.Remove(name);

                values.Add(name, value);
            }
            else
            {
                throw new ArgumentException("Value must be a string or integer: " + value);
            }
        }

        /// <summary>
        /// Set an integer value.
        /// </summary>
        /// <param name="name"></param>
        /// <param name="value"></param>
        public void set(String name, int value)
        {
            set(name, value);
        }

        /// <summary>
        /// Set a boolean value. Elvin connection options are actually either
        /// strings or integers: this is a shortcut for setting an int value
        /// to 0 or 1.
        /// </summary>
        /// <param name="name"></param>
        /// <param name="value"></param>
        public void set(String name, bool value)
        {
            set(name, value ? 1 : 0);
        }

        /// <summary>
        /// Set a string value
        /// </summary>
        /// <param name="name"></param>
        /// <param name="value"></param>
        public void set(String name, String value)
        {
            set(name, value);
        }

        /// <summary>
        /// Set a number of options at once.
        /// </summary>
        /// <param name="options"></param>
        public void setAll(Dictionary<String, Object> options)
        {
            foreach (var entry in options)
                set(entry.Key, entry.Value);
        }

        /// <summary>
        /// Get the value for a connection option, or null if not defined.
        /// </summary>
        /// <param name="name"></param>
        /// <returns></returns>
        public Object get(String name)
        {
            Object o;
            if (values.TryGetValue(name, out o))
            {
                return o;
            }

            return null;
        }

        public T get<T>(String name)
        {
            if (typeof(T) == typeof(bool))
            {
                int i = get<int>(name);
                if (i == null || (i != 1 && i != 0))
                {
                    throw new ArgumentException();
                }
                else
                {
                    return (T)((object)(i == 1));
                }                
            }
            else
            {
                Object o = get(name);
                if (o != null && o.GetType() == typeof(T))
                    return (T)o;
                else
                    throw new ArgumentException();
            }
        }

        public T get<T>(String name, T defaultValue)
        {
            if (typeof(T) == typeof(bool))
            {
                int i = get<int>(name);
                if (i == null || (i != 1 && i != 0))
                {
                    return defaultValue;
                }
                else
                {
                    return (T)((object)(i == 1));
                }
            }
            else
            {
                Object o = get(name);
                if (o != null && o.GetType() == typeof(T))
                    return (T)o;
                else
                    return defaultValue;
            }
        }
    }
}
