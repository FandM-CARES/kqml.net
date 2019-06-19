using System;
using System.Collections.Generic;
using System.Text;
using System.IO;

namespace KQML
{
    public class KQMLQuotation : KQMLObject
    {
        public string QuoteType { get; }
        public KQMLObject KQMLObject { get; }

        public KQMLQuotation (string quoteType, KQMLObject kqmlObject)
        {
            QuoteType = quoteType;
            KQMLObject = kqmlObject;
        }

        public override string ToString()
        {
            return QuoteType + "-" + KQMLObject.ToString();
            
        }
        public void Write(StreamWriter stream)
        {
            stream.Write(ToString());
        }
    }
}
