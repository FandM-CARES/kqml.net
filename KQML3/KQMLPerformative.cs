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

        /// <summary>
        /// Initializes a new instance of <see cref="KQMLPerformative"/> from  <see cref="KQMLList"/>
        /// </summary>
        /// <param name="list">list to be converted</param>
        /// <returns></returns>
        public static KQMLPerformative ListToPerformative(KQMLList list)
        {
            Validate(list.Data);
            return new KQMLPerformative(list.Data);
        }

        /// <summary>
        /// Determines whether a list of objects can be converted into a KQMLPerformative
        /// </summary>
        /// <param name="objects">objectes to be evaluated</param>
        /// <exception cref="KQMLBadPerformativeException"></exception>
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

        /// <summary>
        /// Converts a <see cref="KQMLPerformative"/> to a <see cref="KQMLList"/>
        /// </summary>
        /// <returns>KQMLPerformative as a KQMLList</returns>
        public KQMLList ToList()
        {
            return new KQMLList(Data);
        }
        




    }
}
