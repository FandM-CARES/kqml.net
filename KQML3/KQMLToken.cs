using log4net;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text.RegularExpressions;

namespace KQML
{
    public class KQMLToken : KQMLString
    {
        private static readonly ILog _log = LogManager.GetLogger(typeof(KQMLReader));

        /// <summary>
        /// Initializes a new instance of the <see cref="KQMLToken"/> class that contains a specified string 
        /// </summary>
        /// <param name="s">The string to wrap</param>
        public KQMLToken(string s = "")
        {
            Data = s;
        }

        /// <summary>
        /// Changes underlying string to lower case
        /// </summary>
        /// <returns>Text representation of the <see cref="KQMLToken"/> in lower case</returns>
        public string Lower()
        {
            return Data.ToLower();
        }

        /// <summary>
        /// Changes underlying string to upper case
        /// </summary>
        /// <returns>Text representation of the <see cref="KQMLToken"/> in upper case</returns>
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

        /// <summary>
        /// Parses the text representation of the <see cref="KQMLToken"/> into package and bare name
        /// </summary>
        /// <returns>A list with package and bare name</returns>
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

        /// <summary>
        /// Get the package of the <see cref="KQMLToken"/>
        /// </summary>
        /// <returns>The text representation of the package</returns>
        public string GetPackage()
        {
            return ParsePackage()[0];
        }

        /// <summary>
        /// Determines if the <see cref="KQMLToken"/> has a package
        /// </summary>
        /// <returns><c>true</c> if a package exists, <c>false</c> if it does not</returns>
        public bool HasPackage()
        {
            return !string.IsNullOrEmpty(GetPackage());
        }

        /// <summary>
        /// Determines if the <see cref="KQMLToken"/> is a keyword. 
        /// </summary>
        /// <returns><c>true</c> if it begins with a colon, <c>false</c> if it does not</returns>
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
       
        public override int GetHashCode()
        {
            return Data.GetHashCode();
        }
    }
}

