using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Avis.Client.Types
{
    /// <summary>
    /// Extends System.EventArgs to add useful utilities such as
    /// general data association.
    /// </summary>
    public class AvisEventArgs : EventArgs
    {
        private Dictionary<String, Object> data;

        public AvisEventArgs()
        {
            data = new Dictionary<String, Object>();
        }

        public AvisEventArgs(Dictionary<String, Object> data)
        {
            this.data = data;
        }

        /// <summary>
        /// Set some generic data associated with the event.
        /// </summary>
        /// <param name="key"></param>
        /// <param name="value"></param>
        public void SetData(String key, Object value)
        {
            if (data.ContainsKey(key))
                data.Remove(key);

            if (value != null)
                data.Add(key, value);
        }

        /// <summary>
        /// Get some data previously associated with the event.
        /// </summary>
        /// <param name="key"></param>
        /// <returns>The data, or null if none set for the key.</returns>
        public Object GetData(String key)
        {
            Object o;
            if (data.TryGetValue(key, out o))
                return o;
            else
                return null;
        }
    }
}
