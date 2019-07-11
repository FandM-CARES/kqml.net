﻿using log4net;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text.RegularExpressions;

namespace KQML
{
    public class KQMLToken : KQMLString
    {
        private static readonly ILog _log = LogManager.GetLogger(typeof(KQMLReader));


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
            _log.Debug($"package: {package}, bareName: {bareName}");
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
        public static implicit operator int(KQMLToken input)
        {
            int num;
            if (int.TryParse(input.Data, out num))
            {
                return num;
            }
            throw new ArgumentException($"Cannot convert to type Int32: {input.Data}");
        }




        public override int GetHashCode()
        {
            return Data.GetHashCode();
        }
    }
}

