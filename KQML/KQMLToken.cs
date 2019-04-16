using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Text.RegularExpressions;

namespace KQML
{
    public class KQMLToken : KQMLObject
    {
        public string Data { get; }
        public int Length
        {
            get { return Data.Length; }
        }

        public KQMLToken(string s = "")
        {
            Data = s;
        }

        public string Lower()
        {
            return Data.ToLower();
        }

        public string Upper()
        {
            return Data.ToUpper();
        }

        public void Write(StreamWriter stream) 
        {
            stream.Write(Data);
        }

        override public string ToString()
        {
            return Data;
        }

        public string StringValue()
        {
            return Data;
        }
        public List<string> ParsePackage()
        {
            string pattern1 = "([^:]+)::([^:]+)$";
            string pattern2 = @"([^:]+)::(\|[^\|]*\|)$";

            Regex r1 = new Regex(pattern1);
            Regex r2 = new Regex(pattern2);

            Match g1 = r1.Match(Data);
            Match g2 = r2.Match(Data);

            string package, bareName;
            if (g2.Length > 0)
            {
                package = g2.Groups[1].Value;
                bareName = g2.Groups[2].Value;

            }
            else if (g1.Length > 0)
            {
                package = g1.Groups[1].Value;
                bareName = g1.Groups[2].Value;
            }
            else
            {
                package = null;
                bareName = Data;
            }
            return new List<string> { package, bareName };
        }

        public string GetPackage()
        {
            return ParsePackage()[0];
        }
        public bool HasPackage()
        {
            return !string.IsNullOrEmpty(GetPackage());
        }

        public bool IsKeyword()
        {
            return Data.StartsWith(":");
        }



        public override bool Equals(object obj)
        {
            if (obj is KQMLToken)
            {
                KQMLToken tok = (KQMLToken)obj;
                return Data.Equals(tok.Data);
            }
            else if (obj is string)
            {
                string str = (string)obj;
                return Data.Equals(str);
            }
            else
                throw new ArgumentException("obj must be string or KQMLToken");
        }

        public bool EqualsIgnoreCase(object obj)
        {
            if (obj is KQMLToken)
            {
                KQMLToken tok = (KQMLToken)obj;
                return Data.ToLower().Equals(tok.Data.ToLower());
            }
            else if (obj is string)
            {
                string str = (string)obj;
                return Data.ToLower().Equals(str.ToLower());
            }
            else
                throw new ArgumentException("obj must be string or KQMLToken");
        }

        public char this[int i]
        {
            get { return Data[i]; }

        }
    }
}

