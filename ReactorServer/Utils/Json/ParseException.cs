using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ReactorServer.Utils.Json
{
    /// <summary>
    /// An unchecked exception to indicate that an input does not qualify as valid JSON.
    /// </summary>
    public sealed class ParseException : Exception
    {

        /// <summary>
        /// The absolute index of the character at which the error occurred.
        /// The index of the first character of a document is 0.
        /// </summary>
        public int offset { get; private set; }

        /// <summary>
        /// The number of the line in which the error occurred. The first line counts as 1.
        /// </summary>
        public int line { get; private set; }

        /// <summary>
        /// The index of the character at which the error occurred, relative to the line.
        /// The index of the first character of a line is 0.
        /// </summary>
        public int column { get; private set; }

        internal ParseException(string message, int offset, int line, int column)
            : base(message + " at " + line + ":" + column)
        {
            this.offset = offset;
            this.line = line;
            this.column = column;
        }
    }
}
