using System;
using System.IO;

namespace KQML
{
    /// <summary>
    /// A KQML representation of strings
    /// </summary>
    public class KQMLString : KQMLObject
    {
        public string Data { get; set; }
        public int Length => Data.Length;

        /// <summary>
        /// Creates a new instance of the <see cref="KQMLString"/> class with specified data
        /// </summary>
        /// <param name="data">string to be represented in KQML</param>
        public KQMLString(string data = "")
        {
            Data = data;
        }

        /// <summary>
        /// Gets the character at a specified index
        /// </summary>
        /// <param name="n">The zero-based index to get</param>
        /// <returns>The character at the specified index</returns>
        public char CharAt(int n)
        {
            return Data[n];
        }

        /// <summary>
        /// Write the text representation to a specified stream
        /// </summary>
        /// <param name="stream">The stream to write to</param>
        public void Write(StreamWriter stream)
        {
            stream.Write('"' + Data + '"');
        }

        public override bool Equals(object obj)
        {
            if (obj is KQMLString)
            {
                KQMLString other = (KQMLString)obj;
                return Data.Equals(other.Data);
            }
            return base.Equals(obj);
        }

        public override int GetHashCode()
        {
            return Data.GetHashCode();
        }

        public override string ToString()
        {
            return '"' + Data + '"';
        }

        /// <summary>
        /// Gets the character at a specified index
        /// </summary>
        /// <param name="i">The zero-based index to get</param>
        /// <returns>The character at the specified index</returns>
        public char this[int i] => Data[i];

        public static implicit operator int(KQMLString input)
        {
            int num;
            if (int.TryParse(input.Data, out num))
            {
                return num;
            }
            throw new ArgumentException($"Cannot convert to type Int32: {input.Data}");
        }
       
    }
}
