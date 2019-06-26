using System.IO;

namespace KQML
{
    public class KQMLString : KQMLObject
    {
        public string Data { get; set; }
        public int Length => Data.Length;

        public KQMLString(string data = "")
        {
            Data = data;
        }

        public char CharAt(int n)
        {
            return Data[n];
        }
        public void Write(StreamWriter stream)
        {
            stream.Write(Data);
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
            return Data;
        }

        public char this[int i] => Data[i];
    }
}
