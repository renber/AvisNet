using MiscUtil.Conversion;
using MiscUtil.IO;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Avis.Immigrated
{
    /// <summary>
    /// A binary reader which uses UTF-8, leaves the stream open after its disposal
    /// and uses Big Endian Encoding
    /// </summary>
    public class BinReader : EndianBinaryReader
    {

        public BinReader(Stream stream) :
            base(EndianBitConverter.Big, stream, Encoding.UTF8, true)
        {
        }

        public static int ReadInt32(Stream stream)
        {
            using (var r = new BinReader(stream))
            {
                return r.ReadInt32();
            }
        }

        public static long ReadInt64(Stream stream)
        {
            using (var r = new BinReader(stream))
            {
                return r.ReadInt64();
            }
        }
    }
}
