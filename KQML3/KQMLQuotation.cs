using System;
using System.Collections.Generic;
using System.Text;
using System.IO;

namespace KQML
{
    public class KQMLQuotation : KQMLObject
    {
        /// <summary>
        /// Get the type of the quote
        /// </summary>
        public string QuoteType { get; }
        public KQMLObject KQMLObject { get; }

        /// <summary>
        /// Initializes a new instance of the <see cref="KQMLQuotation"/> class wiht specified quote type and value
        /// </summary>
        /// <param name="quoteType">The type of quote of the new <see cref="KQMLQuotation"/></param>
        /// <param name="kqmlObject">The value of the new <see cref="KQMLQuotation"/></param>
        public KQMLQuotation (string quoteType, KQMLObject kqmlObject)
        {
            QuoteType = quoteType;
            KQMLObject = kqmlObject;
        }

        public override string ToString()
        {
            return QuoteType + "-" + KQMLObject.ToString();
            
        }

        /// <summary>
        /// Writes a text representation of the <see cref="KQMLQuotation"/> to a stream
        /// </summary>
        /// <param name="stream">The stream to write to</param>
        public void Write(StreamWriter stream)
        {
            stream.Write(ToString());
        }
    }
}
