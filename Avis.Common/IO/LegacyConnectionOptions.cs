using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Avis.IO
{
    /**
 * Defines legacy client connection option compatibility. Provides a
 * two-way mapping between old-style connection options and the new
 * scheme defined in the 4.0 client specification.
 * 
 * Compatibility:
 * 
 * <pre>
 *      Standard Name               | Compatibility Name
 *      ----------------------------+------------------------------------
 *      Attribute.Max-Count         | router.attribute.max-count
 *      Attribute.Name.Max-Length   | router.attribute.name.max-length
 *      Attribute.Opaque.Max-Length | router.attribute.opaque.max-length
 *      Attribute.String.Max-Length | router.attribute.string.max-length
 *      Packet.Max-Length           | router.packet.max-length
 *      Receive-Queue.Drop-Policy   | router.recv-queue.drop-policy
 *      Receive-Queue.Max-Length    | router.recv-queue.max-length
 *      Send-Queue.Drop-Policy      | router.send-queue.drop-policy
 *      Send-Queue.Max-Length       | router.send-queue.max-length
 *      Subscription.Max-Count      | router.subscription.max-count
 *      Subscription.Max-Length     | router.subscription.max-length
 *      Supported-Key-Schemes       | router.supported-keyschemes
 *      Vendor-Identification       | router.vendor-identification
 *      ----------------------------+------------------------------------
 * </pre>
 */
    public sealed class LegacyConnectionOptions
    {
        private static readonly Dictionary<String, String> legacyToNew;
        private static readonly Dictionary<String, String> newToLegacy;

        static LegacyConnectionOptions()
        {
            legacyToNew = new Dictionary<String, String>();
            newToLegacy = new Dictionary<String, String>();

            AddLegacy("router.attribute.max-count", "Attribute.Max-Count");
            AddLegacy("router.attribute.name.max-length",
                     "Attribute.Name.Max-Length");
            AddLegacy("router.attribute.opaque.max-length",
                     "Attribute.Opaque.Max-Length");
            AddLegacy("router.attribute.string.max-length",
                     "Attribute.String.Max-Length");
            AddLegacy("router.packet.max-length", "Packet.Max-Length");
            AddLegacy("router.recv-queue.drop-policy",
                     "Receive-Queue.Drop-Policy");
            AddLegacy("router.recv-queue.max-length",
                     "Receive-Queue.Max-Length");
            AddLegacy("router.send-queue.drop-policy",
                     "Send-Queue.Drop-Policy");
            AddLegacy("router.send-queue.max-length",
                     "Send-Queue.Max-Length");
            AddLegacy("router.subscription.max-count",
                     "Subscription.Max-Count");
            AddLegacy("router.subscription.max-length",
                     "Subscription.Max-Length");
            AddLegacy("router.supported-keyschemes", "Supported-Key-Schemes");
            AddLegacy("router.vendor-identification",
                     "Vendor-Identification");
            AddLegacy("router.coalesce-delay",
                     "TCP.Send-Immediately");
        }

        private LegacyConnectionOptions()
        {
            // zip
        }

        private static void AddLegacy(String oldOption, String newOption)
        {
            legacyToNew.Add(oldOption, newOption);
            newToLegacy.Add(newOption, oldOption);
        }

        public static String LegacyToNew(String option)
        {
            if (legacyToNew.ContainsKey(option))
                return legacyToNew[option];
            else
                return option;
        }

        public static String NewToLegacy(String option)
        {
            if (newToLegacy.ContainsKey(option))
                return newToLegacy[option];
            else
                return option;
        }

        public static void SetWithLegacy(Dictionary<String, Object> options,
                                          String option, Object value)
        {
            options.Add(option, value);
            options.Add(NewToLegacy(option), value);
        }
    }
}
