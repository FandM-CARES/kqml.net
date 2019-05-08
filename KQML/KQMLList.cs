using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace KQML
{
    public class KQMLList : KQMLObject
    {
        public List<KQMLObject> Data { get; }
        public int Count { get { return Data.Count; } }

        public KQMLList()
        {
            Data = new List<KQMLObject>();
        }
        public KQMLList(string s)
        {
            Data = new List<KQMLObject>();
            Append(s);//class method Append, not Add 
        }

        public KQMLList(List<object> obj)
        {
            Data = new List<KQMLObject>();
            foreach (object o in obj)
            {
                if (o is string)
                {
                    string s = (string)o;
                    Append(s);
                }
                else if (!(o is KQMLObject))
                    continue;
                else
                    Append(o);
            }
        }
        //this is here becuase apparently this case does not call the List of objects constructor
        public KQMLList(List<KQMLObject> list)
        {
            Data = new List<KQMLObject>();
            foreach (KQMLObject o in list)
            {
                Append(o);
            }
        }
        public KQMLObject this[int i]
        {
            get
            {
                return Data[i];
            }
        }

        public string Head()
        {
            return Data[0].ToString();
        }

        public KQMLObject Get(string keyword)
        {

            if (!keyword.StartsWith(":"))
                keyword = ":" + keyword;
            int i = 0;
            foreach (object obj in Data)
            {
                if (obj.ToString().ToUpper().Equals(keyword.ToUpper()))
                {
                    if (i < Data.Count - 1)
                        return Data[i + 1];
                    else
                        return null;
                }
                ++i;
            }
            return null;
        }
        public string Gets(string keyword)
        {
            KQMLObject param = Get(keyword);
            if (!param.Equals(null))
            {
                return param.ToString();
                //using ToString instead of StringValue because not all implementations of KQMLOjbect has StringValue
            }
            return null;
        }

        public void Append(object obj)
        {
            if (obj is string)
            {
                string str = (string)obj;
                KQMLToken token = new KQMLToken(str);
                Data.Add(token);
            }
            else if (obj is KQMLObject)
            {
                Data.Add((KQMLObject)obj);
            }
        }
        public void Push(object obj)
        {
            if (obj is string)
            {
                string str = (string)obj;
                KQMLToken kqml = new KQMLToken(str);
                Data.Insert(0, kqml);

            }
            else if (obj is KQMLObject)
            {
                KQMLObject kqml = (KQMLObject)obj;
                Data.Insert(0, kqml);
            }

        }

        public void InsertAt(int index, object obj)
        {
            if (obj is string)
            {
                string str = (string)obj;
                KQMLToken kqml = new KQMLToken(str);
                Data.Insert(index, kqml);

            }
            else if (obj is KQMLObject)
            {
                KQMLObject kqml = (KQMLObject)obj;
                Data.Insert(index, kqml);
            }

        }

        public void RemoveAt(int index)
        {
            if (index > Data.Count)
                throw new IndexOutOfRangeException();
            else
            {
                Data.RemoveAt(index);
            }

        }
        public void Set(string keyword, object value)
        {
            if (!keyword.StartsWith(":"))
                keyword = ":" + keyword;
            int i = 0;
            bool found = false;
            foreach (object obj in Data)
            {
                if (obj.ToString().ToLower().Equals(keyword.ToLower()))
                {
                    if (i < Data.Count - 1)
                    {
                        if (value is string)
                        {
                            KQMLToken kqml = new KQMLToken((string)value);
                            Data[i + 1] = kqml;
                            found = true;
                        }
                        else if (value is KQMLObject)
                        {
                            KQMLObject kqml = (KQMLObject)value;
                            Data[i + 1] = kqml;
                            found = true;
                        }
                        else
                            throw new ArrayTypeMismatchException();
                        break;
                    }

                }
                ++i;
            }
            if (!found)
            {
                KQMLToken keyToken = new KQMLToken(keyword);
                if (value is string)
                {
                    KQMLToken valueToken = new KQMLToken((string)value);
                    Data.Add(keyToken);
                    Data.Add(valueToken);
                }
                else if (value is KQMLObject)
                {
                    KQMLObject valueObject = (KQMLObject)value;
                    Data.Add(keyToken);
                    Data.Add(valueObject);
                }
            }

        }
        public void Write()
        {
            throw new NotImplementedException();
        }
 
        public static KQMLList FromString(string s)
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
                        return kreader.ReadList();
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

        public KQMLList Sublist(int from)
        {
            return new KQMLList(Data.Skip(from).ToList());
        }

        public void Sets(string keyword, string value)
        {
            KQMLString kqmlValue = new KQMLString(value);
            Set(keyword, kqmlValue);
        }

        public int IndexOf(object o)
        {
            if (o is string)
            {
                string str = (string)o;
                KQMLToken token = new KQMLToken(str);
                return Data.IndexOf(token);
            }
            else
            {
                KQMLObject kqmlo = (KQMLObject)o;
                return Data.IndexOf(kqmlo);
            }
        }

        public int IndexOfIgnoreCase(string keyword)
        {
            for (int i = 0; i < Data.Count; i++)
            {
                KQMLObject kqmlo = Data[i];
                if (kqmlo.ToString().ToLower().Equals(keyword.ToLower()))
                    return i;
            }
            return -1;
        }

        //how is line 245 in py legal
        //public int IndexOfString(string str)
        //{
        //    throw new NotImplementedException();
        //}

        public override string ToString()
        {
            string str = "(";
            str += string.Join(" ", from thing in Data select thing.ToString());
            return str += ")";
        }



    }
}
