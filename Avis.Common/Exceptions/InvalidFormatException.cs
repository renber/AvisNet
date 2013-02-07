using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Avis.Exceptions
{
    /// <summary>
    /// Thrown when a parsing process detects some sort of invalid format
    /// in its input.
    /// </summary>
    public class InvalidFormatException : Exception
    {

        public InvalidFormatException(String message)
            : base(message)
        {

        }
    }
}
