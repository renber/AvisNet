using Avis.Client.Types;
using Avis.Immigrated;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Avis.IO.Messages
{
    public class Nack : XidMessage
    {
        public const int ID = 48;

        public static readonly Object[] EMPTY_ARGS = new Object[0];

        public NackError error;
        public String message;
        public Object[] args;

        public Nack()
        {
            // zip
        }

        public override int TypeId
        {
            get { return ID; }
        }

        public Nack(XidMessage inReplyTo, int error, String message)
            : this(inReplyTo, error, message, EMPTY_ARGS)
        {

        }

        public Nack(XidMessage inReplyTo, int error, String message, params Object[] args)
            : base(inReplyTo)
        {
            if (message == null)
                throw new ArgumentNullException("Message cannot be null");

            this.error = (NackError)error;
            this.message = message;
            this.args = args;
        }

        /**
         * Return the error text for the NACK error code.
         * 
         * @see #errorTextFor(int)
         */
        public String ErrorCodeText
        {
            get { return ErrorTextFor(error); }
        }

        /**
         * Generate a formatted message from the message template returned
         * by the router. e.g. expand the %1 and %2 in "%1: Expression '%2'
         * does not refer to a name" to the values in <tt>arg [0]</tt> and
         * <tt>arg [1]</tt>.
         */
        public String FormattedMessage
        {
            get
            {
                if (args.Length == 0)
                {
                    return message;
                }
                else
                {
                    StringBuilder str = new StringBuilder(message);

                    for (int i = 0; i < args.Length; i++)
                        Replace(str, i + 1, args[i]);

                    return str.ToString();
                }
            }
        }

        /**
         * Replace embedded arg reference(s) with a value.
         * 
         * @param str The string builder to modify.
         * @param argNumber The argument number (1..)
         * @param arg The arg value.
         */
        private static void Replace(StringBuilder str, int argNumber, Object arg)
        {
            String tag = "%" + argNumber;

            int index;

            while ((index = str.ToString().IndexOf(tag)) != -1)
            {
                str.Remove(index, tag.Length);
                str.Insert(index, arg.ToString());
            }
        }

        /**
         * Return the error text for a given NACK error code.
         */
        public static String ErrorTextFor(NackError error)
        {
            switch (error)
            {
                case NackError.ProtIncompat:
                    return "Incompatible protocol";
                case NackError.ProtError:
                    return "Communication protocol error";
                case NackError.NoSuchSub:
                    return "Unknown subscription ID";
                case NackError.ImplLimit:
                    return "Exceeded client connection resource limit";
                case NackError.NotImpl:
                    return "Feature not implemented";
                case NackError.ParseError:
                    return "Subscription parse error";
                case NackError.ExpisTrivial:
                    return "Expression is trivial (constant)";
                default:
                    return "Error code " + error;
            }
        }

        public override void Encode(Stream outStream)
        {
            base.Encode(outStream);
            BinWriter.QuickWrite(outStream, (w) => w.Write((int)error));
            XdrCoding.putString(outStream, message);
            XdrCoding.putObjects(outStream, args);
        }

        public override void Decode(Stream inStream)
        {
            base.Decode(inStream);

            error = (NackError)BinReader.ReadInt32(inStream);
            message = XdrCoding.getString(inStream);
            args = XdrCoding.getObjects(inStream);
        }
    }
}
