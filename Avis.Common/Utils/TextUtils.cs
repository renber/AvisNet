using Avis.Exceptions;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Avis.Utils
{
    /// <summary>
    /// General text formatting utilities.
    /// </summary>
    public static class TextUtils
    {
        private static readonly char [] HEX_TABLE = 
            {'0', '1', '2', '3', '4', '5', '6', '7', '8', '9', 
            'a', 'b', 'c', 'd', 'e', 'f'};

        /// <summary>
        /// Generate human friendly string dump of a Dictionary.
        /// </summary>
        /// <typeparam name="TKey"></typeparam>
        /// <typeparam name="TValue"></typeparam>
        /// <param name="map"></param>
        /// <returns></returns>
        public static String MapToString<TKey, TValue>(Dictionary<TKey, TValue> map)
        {
            StringBuilder str = new StringBuilder();
            bool first = true;

            foreach (var entry in map)
            {
                if (!first)
                    str.Append(", ");

                first = false;

                str.Append('{');
                str.Append(entry.Key).Append(" = ").Append(entry.Value);
                str.Append('}');
            }

            return str.ToString();
        }

        public static String FormatNotification(Dictionary<String, Object> attributes)
        {
            var list = attributes.OrderBy(x => x.Key);
            StringBuilder sb = new StringBuilder();
            bool first = true;
            foreach (var s in list)
            {
                if (!first)
                {
                    sb.Append("\n");
                }
                else
                {
                    first = false;
                }
                AppendEscaped(sb, s.Key, " :");
                sb.Append(": ");
                AppendValue(sb, s.Value);
            }

            return sb.ToString();
        }

        private static void AppendValue(StringBuilder str, Object value)
        {
            if (value is String)
            {
                str.Append('"');
                AppendEscaped(str, (String)value, "\"");
                str.Append('"');
            }
            else if (value is int || value is double || value is long)
            {
                str.Append(value);

                if (value is long)
                    str.Append('L');
            }
            else
            {
                str.Append('[');
                AppendHexBytes(str, (byte[])value);
                str.Append(']');
            }
        }

        /// <summary>
        /// Append a string to a builder, escaping (with '\') any instances
        /// of a set of special characters.
        /// </summary>
        /// <param name="builder"></param>
        /// <param name="?"></param>
        public static void AppendEscaped(StringBuilder builder, String str, String charsToEscape)
        {
            for (int i = 0; i < str.Length; i++)
            {
                char c = str[i];

                if (charsToEscape.IndexOf(c) != -1)
                    builder.Append('\\');

                builder.Append(c);
            }
        }

        /// <summary>
        /// Append a byte array to a builder in form: 01 e2 fe ff ...
        /// </summary>
        /// <param name="str"></param>
        /// <param name="bytes"></param>
        public static void AppendHexBytes(StringBuilder str, byte[] bytes)
        {
            bool first = true;

            foreach (byte b in bytes)
            {
                if (!first)
                {
                    str.Append(' ');
                }
                else
                {
                    first = false;
                }

                AppendHex(str, b);
            }
        }

        /// <summary>
        /// Append the hex form of a byte to a builder.
        /// </summary>
        /// <param name="str"></param>
        /// <param name="b"></param>
        public static void AppendHex(StringBuilder str, byte b)
        {
            str.Append(HEX_TABLE[(b >> 4) & 0x0F]);
            str.Append(HEX_TABLE[(b >> 0) & 0x0F]);
        }

        /// <summary>
        /// Find the first index of the given character, skipping instances
        /// that are escaped by '\'.
        /// </summary>
        public static int FindFirstNonEscaped(String str, char toFind)
        {
            return FindFirstNonEscaped(str, 0, toFind);
        }

        /// <summary>
        /// Find the first index of the given character, skipping instances
        /// that are escaped by '\'.
        /// </summary>
        public static int FindFirstNonEscaped(String str, int start, char toFind)
        {
            bool escaped = false;

            for (int i = start; i < str.Length; i++)
            {
                char c = str[i];

                if (c == '\\')
                {
                    escaped = true;
                }
                else
                {
                    if (!escaped && c == toFind)
                        return i;

                    escaped = false;
                }
            }

            return -1;
        }

        /// <summary>
        /// Remove any \'s from a string.
        /// </summary>
        /// <param name="text"></param>
        /// <returns></returns>
        /// <exception cref="InvalidFormatException"></exception>
        public static String StripBackslashes(String text)
        {
            if (text.IndexOf('\\') != -1)
            {
                StringBuilder buff = new StringBuilder(text.Length);

                for (int i = 0; i < text.Length; i++)
                {
                    char c = text[i];

                    if (c != '\\')
                    {
                        buff.Append(c);
                    }
                    else
                    {
                        i++;

                        if (i < text.Length)
                            buff.Append(text[i]);
                        else
                            throw new InvalidFormatException("Invalid trailing \\");
                    }
                }

                text = buff.ToString();
            }

            return text;
        }

        /// <summary>
        /// Parse a string expression as a value. Values may be quoted
        /// strings ("string"), numbers (0.1, 3, 123456789L), or byte arrays
        /// ([0a ff de ad]).
        /// </summary>
        /// <param name="expr"></param>
        /// <returns></returns>
        /// <exception cref="InvalidFormatException"></exception>
        public static Object StringToValue(String expr)
        {
            char firstChar = expr[0];

            if (firstChar == '"' || firstChar == '\'')
                return quotedStringToString(expr);
            else if (firstChar >= '0' && firstChar <= '9')
                return stringToNumber(expr);
            else if (firstChar == '[')
                return stringToOpaque(expr);
            else
                throw new InvalidFormatException("Unrecognised value expression: \"" + expr + "\"");
        }

        /// <summary>
        /// Parse a numeric int, long or double value. e.g. 32L, 3.14, 42.
        /// </summary>
        /// <param name="valueExpr"></param>
        /// <returns></returns>
        public static Object stringToNumber(String valueExpr)
        {
            try
            {
                if (valueExpr.IndexOf('.') != -1)
                    return Double.Parse(valueExpr);
                else if (valueExpr.EndsWith("L") || valueExpr.EndsWith("l"))
                    return long.Parse(valueExpr.Substring(0, valueExpr.Length - 1));
                else
                    return int.Parse(valueExpr);
            }
            catch (FormatException ex)
            {
                throw new InvalidFormatException("Invalid number: " + valueExpr);
            }
        }

        /// <summary>
        /// Parse a string value in the format "string", allowing escaped "'s
        /// inside the string.
        /// </summary>
        /// <param name="valueExpr"></param>
        /// <returns></returns>
        /// <exception cref="InvalidFormatException"></exception>
        public static String quotedStringToString(String valueExpr)
        {
            if (valueExpr.Length == 0)
                throw new InvalidFormatException("Empty string");

            char quote = valueExpr[0];

            if (quote != '\'' && quote != '"')
                throw new InvalidFormatException("String must start with a quote");

            int last = FindFirstNonEscaped(valueExpr, 1, quote);

            if (last == -1)
                throw new InvalidFormatException("Missing terminating quote in string");
            else if (last != valueExpr.Length - 1)
                throw new InvalidFormatException("Extra characters following string");

            return StripBackslashes(valueExpr.Substring(1, last));
        }

        /// <summary>
        /// Parse an opaque value expression e.g. [00 0f 01]. 
        /// </summary>
        /// <param name="valueExpr"></param>
        /// <returns></returns>
        /// <exception cref="InvalidFormatException"></exception>
        public static byte[] stringToOpaque(String valueExpr)
        {
            if (valueExpr.Length < 2)
                throw new InvalidFormatException("Opaque value too short");
            else if (valueExpr[0] != '[')
                throw new InvalidFormatException("Missing '[' at start of opaque");

            int closingBrace = valueExpr.IndexOf(']');

            if (closingBrace == -1)
                throw new InvalidFormatException("Missing closing \"]\"");
            else if (closingBrace != valueExpr.Length - 1)
                throw new InvalidFormatException("Junk at end of opaque value");

            return hexToBytes(valueExpr.Substring(1, closingBrace));
        }

        /// <summary>
        /// Parse a series of hex pairs as a sequence of unsigned bytes.
        /// Pairs may be separated by optional whitespace. e.g. "0A FF 00 01"
        /// or "deadbeef".
        /// </summary>
        /// <param name="?"></param>
        /// <returns></returns>
        /// <exception cref="InvalidFormatException"></exception>
        public static byte[] hexToBytes(String str)
        {
            str = str.Replace(" ", ""); // trim whitespace            

            if (str.Length % 2 != 0)
                throw new InvalidFormatException("Hex bytes must be a set of hex pairs");

            byte[] bytes = new byte[str.Length / 2];

            for (int i = 0; i < str.Length; i += 2)
                bytes[i / 2] = hexToByte(str.Substring(i, i + 2));

            return bytes;
        }

        /// <summary>
        /// Parse a string expression as a hex-coded unsigned byte.
        /// </summary>
        /// <param name="byteExpr"></param>
        /// <returns></returns>
        /// <exception cref="InvalidFormatException"></exception>
        public static byte hexToByte(String byteExpr)
        {
            if (byteExpr.Length == 0)
            {
                throw new InvalidFormatException("Byte value cannot be empty");
            }
            else if (byteExpr.Length > 2)
            {
                throw new InvalidFormatException
                  ("Byte value too long: \"" + byteExpr + "\"");
            }

            int value = 0;

            for (int i = 0; i < byteExpr.Length; i++)
                value = (value << 4) | HexValue(byteExpr[i]);

            return (byte)value;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="c"></param>
        /// <exception cref="InvalidFormatException"></exception>
        private static int HexValue(char c)
        {
            if (c >= '0' && c <= '9')
                return c - '0';
            else if (c >= 'a' && c <= 'f')
                return c - 'a' + 10;
            else if (c >= 'A' && c <= 'F')
                return c - 'A' + 10;
            else
                throw new InvalidFormatException("Not a valid hex character: " + c);
        }
    }
}
