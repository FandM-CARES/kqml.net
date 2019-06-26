using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using KQML.KQMLExceptions;

namespace KQML
{
    public class KQMLPerformative : KQMLList
    {
        public KQMLPerformative(List<KQMLObject> objects) : base(objects)
        {
            Validate(objects);
        }

        public KQMLPerformative(string str) : base(str)
        {

        }

        public static KQMLPerformative ListToPerformative(KQMLList list)
        {
            return new KQMLPerformative(list.Data);
        }
        private static void Validate(List<KQMLObject> objects)
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

        public KQMLList ToList()
        {
            return new KQMLList(Data);
        }
        




    }
}
