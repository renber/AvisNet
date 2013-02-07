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
    /// A binary writer which uses UTF-8 and leaves the stream open after its disposal    
    /// and uses Big Endian
    /// </summary>
    public class BinWriter : EndianBinaryWriter
    {

        public BinWriter(Stream stream) :
            base(EndianBitConverter.Big, stream, Encoding.UTF8, true)
        {
            
        }

        /// <summary>
        /// Creates a BinWriter and uses the given delegate to write,
        /// then disposes the BinWriter
        /// </summary>
        /// <param name="DoWrite"></param>
        public static void QuickWrite(Stream stream, Action<BinWriter> DoWrite)
        {
            using (BinWriter w = new BinWriter(stream))
            {
                DoWrite(w);
            }
        }
    }
}
