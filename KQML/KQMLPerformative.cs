using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using KQML.KQMLExceptions;

namespace KQML
{
    public class KQMLPerformative : KQMLObject
    {
        public KQMLList Data { get; }
        public int Count { get { return Data.Count; } }

        public KQMLPerformative(KQMLList objects)
        {
            Validate(objects);
            Data = objects;
        }

        public KQMLPerformative(string str)
        {
            Data = new KQMLList(str);

        }
        private static void Validate(KQMLList objects)
        {
            if (!(objects[0] is KQMLToken))
            {
                throw new Exception("Does not start with Token");
            }
            int i = 1;
            while (i < objects.Count)
            {
                if (!(objects[i] is KQMLToken))
                {
                    throw new KQMLBadPerformativeException("perfomative not a token");
                }
                else
                {
                    KQMLToken token = (KQMLToken)objects[i];
                    Console.Write(token[0]);
                    if (!token[0].Equals(':'))
                        throw new KQMLBadPerformativeException("perfomative not a keyword");
                }
                i += 1;
                if (i == objects.Count)
                    throw new KQMLBadPerformativeException("Missing value for keyword");
                i += 1;

            }
        }
        public string Head()
        {
            return Data.Head();
        }

        public KQMLObject Get(string keyword)
        {
            return Data.Get(keyword);
        }

        public string Gets(string keyword)
        {
            return Data.Gets(keyword);
        }
        public void Set(string keyword, object value)
        {
            Data.Set(keyword, value);
        }
        public void Sets(string keyword, string value)
        {
            Data.Sets(keyword, value);
        }
        public KQMLList ToList()
        {
            return Data;
        }
        public void Write(StreamWriter stream)
        {
            stream.Write(Data);
        }

        public static KQMLPerformative FromString(string s)
        {
            if (s != null)
            {
                byte[] byteArray = Encoding.ASCII.GetBytes(s);
                StreamReader sreader = null;
                try
                {
                    sreader = new StreamReader(new MemoryStream(byteArray));
                    using (KQMLReader kreader = new KQMLReader(sreader))
                    {
                        sreader = null;
                        return new KQMLPerformative(kreader.ReadList());
                    }
                }
                finally
                {
                    if (sreader != null)
                        sreader.Dispose();
                }
            }
            return null;
        }

        public override string ToString()
        {
            return Data.ToString();
        }



    }
}
