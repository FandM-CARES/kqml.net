using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace KQML
{
    /// <summary>
    /// Represents a collection of KQMLObjects
    /// </summary>
    public class KQMLList : KQMLObject
    {
        public List<KQMLObject> Data { get; set; }
        public int Count { get { return Data.Count; } }

        public KQMLList()
        {
            Data = new List<KQMLObject>();
        }
        public KQMLList(string s)
        {
            Data = new List<KQMLObject>();
            Append(s); 
        }

        /// <summary>
        /// Initializes a new instance of <see cref="KQMList"/> class that contains elements copied from the spceified <see cref="List{object}"/>
        /// </summary>
        /// <param name="obj">The list of objects whose elements are copied to the new <see cref="KQMLList"/></param>
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
                else if (o is KQMLObject ko)
                    Append(ko);
                else
                    continue;
            }
        }
        /// <summary>
        /// Initializes a new instance of <see cref="KQMList"/> class that contains elements copied from the spceified list of KQMLObject
        /// </summary>
        /// <param name="list">The <see cref="List{KQMLObject}<"/> whose elements are copied to the new <see cref="KQMLList"/></param>
        public KQMLList(List<KQMLObject> list)
        {
            Data = new List<KQMLObject>();
            foreach (KQMLObject o in list)
            {
                Append(o);
            }
        }

        /// <summary>
        /// Gets the element at the specified index
        /// </summary>
        /// <param name="i">The zero-based index of the element to get</param>
        /// <returns>The element at the specified index</returns>
        public KQMLObject this[int i] => Data[i];

        /// <summary>
        /// Returns the string representation of the first element of the <see cref="KQMLList"/>
        /// </summary>
        /// <returns>The first element as a string</returns>
        public string Head()
        {
            return Data[0].ToString();
        }

        /// <summary>
        /// Returns the element associated with the specified keyword
        /// </summary>
        /// <param name="keyword">The key of the element to get</param>
        /// <returns>The element associated with the keyword. <c>null</c> if keyword not found</returns>
        /// <remarks><paramref name="keyword"/> needs not start with a colon.</remarks>
        public KQMLObject Get(string keyword)
        {

            if (!keyword.StartsWith(":"))
                keyword = ":" + keyword;
            int i = 0;
            foreach (object obj in Data)
            {
                if (obj.ToString().ToUpper().Equals(keyword.ToUpper()))
                {
                    return i < Data.Count - 1 ? Data[i + 1] : null;
                }
                ++i;
            }
            return null;
        }

        /// <summary>
        /// Returns the string represenataion element associated with the specified keyword
        /// </summary>
        /// <param name="keyword">The key of the element to get</param>
        /// <returns>value associated with the keyword as a string</returns>
        public string Gets(string keyword)
        {
            KQMLObject param = Get(keyword);
            if (!param.Equals(null))
            {
                return param.ToString();
                //using ToString instead of StringValue because not all implementations of KQMLObject has StringValue
            }
            return null;
        }

        /// <summary>
        /// Adds a string to the end of the <see cref="KQMLList"/> as an KQMLToken
        /// </summary>
        /// <param name="str">The string to be added.</param>
        public void Append(string str)
        {
            KQMLToken token = new KQMLToken(str);
            Data.Add(token);
        }
        /// <summary>
        /// Adds a KQMLObject to the end of the <see cref="KQMLList"/>
        /// </summary>
        /// <param name="obj">The KQMLObject to be added.</param>
        public void Append(KQMLObject obj)
        {
            Data.Add(obj);
        }

        /// <summary>
        /// Adds an object to the front of the <see cref="KQMLList"/>
        /// </summary>
        /// <param name="obj">The object to be added</param>
        public void Push(object obj)
        {
            if (obj is string str)
            {
                KQMLToken kqml = new KQMLToken(str);
                Data.Insert(0, kqml);

            }
            else if (obj is KQMLObject kqml)
            {
                Data.Insert(0, kqml);
            }

        }

        /// <summary>
        /// Inserts an element into the <see cref="KQMLList"/> at the specified index
        /// </summary>
        /// <param name="index">The zero-based index at which <paramref name="obj"/> should be inserted</param>
        /// <param name="obj">The object to insert</param>
        public void InsertAt(int index, object obj)
        {
            if (obj is string str)
            {
                KQMLToken kqml = new KQMLToken(str);
                Data.Insert(index, kqml);

            }
            else if (obj is KQMLObject kqml)
            {
                Data.Insert(index, kqml);
            }

        }

        /// <summary>
        /// Removes the element at a specified index
        /// </summary>
        /// <param name="index">The zero-based index of the element to remove</param>
        /// <exception cref="IndexOutOfRangeException"><paramref name="index"/> is less than 0 or greater than <see cref="Count"/></exception>
        public void RemoveAt(int index)
        {
            if (index > Data.Count || index < 0)
                throw new IndexOutOfRangeException();
            else
            {
                Data.RemoveAt(index);
            }

        }

        /// <summary>
        /// Set the value associated with a specified key
        /// </summary>
        /// <param name="keyword">The key of the value to set</param>
        /// <param name="value">The new value</param>
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

        /// <summary>
        /// Writes a text representation of the <see cref="KQMLList"/> to an output
        /// </summary>
        /// <param name="output">The stream to write to</param>
        public void Write(TextWriter output)
        {
            string fullString = "(" + string.Join(" ", Data.Select(element => element.ToString())) + ")";
            output.Write(fullString);
        }

        /// <summary>
        /// Factory Method to create KQMLList from a string. Calls KQMLReader.ReadList
        /// </summary>
        /// <param name="s">String used to generate list</param>
        /// <returns>Returns a KQMLList</returns>
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
                    sreader?.Dispose();
                }
            }
            return null;
        }

        /// <summary>
        /// Creates a copy of the <see cref="KQMLList"/> starting at the specified index
        /// </summary>
        /// <param name="from">The zero based index to start copying</param>
        /// <returns></returns>
        public KQMLList Sublist(int from)
        {
            return new KQMLList(Data.Skip(from).ToList());
        }

        /// <summary>
        /// Set the element of the list after the given keyword 
        /// </summary>
        /// <param name="keyword">
        /// The keyword parameter to find in the list.
        /// Putting a colon before the keyword is optional, if no colon is
        /// given, it is added automatically(e.g. "keyword" will be found as
        /// ":keyword" in the list).
        /// </param>
        /// <param name="value">Text representation of the new value. It will be instantiated as a <see cref="KQMLString"/></param>
        public void Sets(string keyword, string value)
        {
            KQMLString kqmlValue = new KQMLString(value);
            Set(keyword, kqmlValue);
        }

        /// <summary>
        /// Returns the zero-based index of the first occurrence of a value in the <see cref="KQMLList"/> o.
        /// </summary>
        /// <param name="o">The object to locate</param>
        /// <returns>The zero-based starting index of the search.</returns>
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

        /// <summary>
        /// Same as <seealso cref="IndexOf(object)", except casing does not matter/>.
        /// </summary>
        /// <param name="o">The object to locate</param>
        /// <returns>The zero-based starting index of the search.</returns>
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



        public override string ToString()
        {
            string str = "(";
            str += string.Join(" ", from thing in Data select thing.ToString());
            return str += ")";
        }



    }
}
