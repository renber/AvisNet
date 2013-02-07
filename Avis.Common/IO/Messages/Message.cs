/*
 * port of org.avis.io.messages.Message
 */
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Avis.IO.Messages
{
    public abstract class Message
    {

        /**
 * The message's unique type ID.<p>
 * 
 * Message ID's (from Elvin Client Protocol 4.0 draft):
 * 
 * <pre>
 * UNotify        = 32,
 *   
 * Nack           = 48,   ConnRqst       = 49,
 * ConnRply       = 50,   DisconnRqst    = 51,
 * DisconnRply    = 52,   Disconn        = 53,
 * SecRqst        = 54,   SecRply        = 55,
 * NotifyEmit     = 56,   NotifyDeliver  = 57,
 * SubAddRqst     = 58,   SubModRqst     = 59,
 * SubDelRqst     = 60,   SubRply        = 61,
 * DropWarn       = 62,   TestConn       = 63,
 * ConfConn       = 64,
 *   
 * QnchAddRqst    = 80,   QnchModRqst    = 81,
 * QnchDelRqst    = 82,   QnchRply       = 83,
 * SubAddNotify   = 84,   SubModNotify   = 85,
 * SubDelNotify   = 86
 * </pre>
 */
        public abstract int TypeId { get; }

        public String Name
        {
            get
            {
                return this.GetType().Name;
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="outStream"></param>
        /// <exception cref="ProtocolCodecException"></exception>
        public abstract void Encode(Stream outStream);        

        /// <summary>
        /// 
        /// </summary>
        /// <param name="outStream"></param>
        /// <exception cref="ProtocolCodecException"></exception>
        public abstract void Decode(Stream inStream);

        public override String ToString()
        {
            return Name;
        }

    }
}
