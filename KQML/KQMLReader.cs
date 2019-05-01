using System;
using System.Collections;
using System.IO;
using System.Linq;
using System.Text;
using KQML.KQMLExceptions;
using log4net;

namespace KQML
{
    public class KQMLReader : IDisposable
    {
        public StreamReader Reader;
        public StringBuilder Inbuf;
        private static readonly ILog _log = LogManager.GetLogger(typeof(KQMLReader));

        public KQMLReader(StreamReader r)
        {
            Reader = r;
            Inbuf = new StringBuilder();
            
        }

        public void Close()
        {
            if (Reader != null)
            {
                Reader.ReadToEnd();
                Reader.Dispose();
            }
        }

        public void Dispose()
        {
            Close();
        }

        public char ReadChar()
        {
            char ch = (char)Reader.Read();
            return ch;

        }

        public static bool IsSpecial(char ch)
        {
            string specials = @"<>=+-*/&^~_@$%:.!?|";
            if (specials.Contains(ch))
                return true;
            return false;

        }
        public static bool IsTokenChar(char ch)
        {
            string nonTokenChars = "'`\"#()";
            if (!(string.IsNullOrEmpty(ch.ToString())) && nonTokenChars.Contains(ch))
                return true;
            return false;
        }

        public KQMLObject ReadExpr(bool backquoted = false)
        {
            char ch = (char)Reader.Peek();
            if (ch == '\'' || ch == '`')
                return ReadQuotation(backquoted);
            else if (ch == '"' || ch == '#')
                return ReadString();
            else if (ch == '(')
                return ReadList(backquoted);
            else if (ch == ',')
            {
                if (!backquoted)
                {
                    ch = ReadChar();
                    throw new KQMLBadCommandException(Inbuf.ToString());
                }
                else
                    return ReadQuotation(backquoted);

            }
            else
            {
                if (IsTokenChar(ch))
                    return ReadToken();
                else
                {
                    ch = ReadChar();
                    throw new KQMLBadCharacterException(Inbuf.ToString());
                }
            }
        }

        public KQMLToken ReadToken()
        {
            char ch;
            for (int i = 0; i < 1000000; i++)
            {
                ch = (char)Reader.Peek();
                if (IsTokenChar(ch))
                {
                    Inbuf.Append(ch);
                    ReadChar();
                }
                else break;
            }

            return new KQMLToken(Inbuf.ToString());
        }

        public KQMLQuotation ReadQuotation(bool backquoted)
        {
            char ch = (char)Reader.Peek();
            if (ch == '`')
                return new KQMLQuotation(ch.ToString(), ReadExpr(true));
            else if (ch == '\'' || ch == ',')
                return new KQMLQuotation(ch.ToString(), ReadExpr(backquoted));
            else
                return null;
        }

        public KQMLString ReadString()
        {
            char ch = (char)Reader.Peek();
            if (ch == '"')
                return ReadQuotedString();
            else
                return ReadHashedString();
        }

        private KQMLString ReadHashedString()
        {
            StringBuilder buf = new StringBuilder();
            int count = 0;
            char ch;
            for (int i = 0; i < 1000000; i++)
            {
                ch = ReadChar();
                if (ch == '"')
                    break;
                if (!char.IsDigit(ch))
                {
                    throw new KQMLBadHashException(buf.ToString());
                }
                else
                {
                    count = count * 10 + (int)ch;
                }


            }
            if (count == 0)
                return new KQMLString("");
            else
            {
                for (int i = 0; i < count; i++)
                {
                    buf.Append(ReadChar());
                }
            }
            return new KQMLString(buf.ToString());
        }

        public KQMLString ReadQuotedString()
        {
            StringBuilder buf = new StringBuilder();
            char ch;
            for (int i = 0; i < 1000000; i++)
            {
                ch = ReadChar();
                if (ch == '"')
                    break;
                if (ch == '\\')
                {
                    ch = ReadChar();
                    if (ch == '\\')
                    {
                        Inbuf.Append("\\\\");
                        continue;
                    }
                }
                Inbuf.Append(ch);
            }
            return new KQMLString(buf.ToString());
        }

        public object ReadListForFile()
        {
            throw new NotImplementedException();
        }

        public KQMLList ReadList(bool backquoted = false)
        {
            KQMLList lst = new KQMLList();
            char ch = ReadChar();
            if (ch != '(')
                throw new KQMLBadOpenException(Inbuf.ToString());
            SkipWhitespace();
            for (int i = 0; i < 1000000; i++)
            {
                ch = (char)Reader.Peek();
                if (ch == ')')
                    break;
                lst.Append(ReadExpr(backquoted));
                ch = (char)Reader.Peek();
                if (ch != ')')
                {
                    if (ch != '(')
                        ReadWhitespace();
                }
            }
            ch = ReadChar();
            if (ch != ')')
                throw new KQMLBadCloseException(Inbuf.ToString());
            return lst;
        }

        private void ReadWhitespace()
        {
            char ch = ReadChar();
            if (!char.IsWhiteSpace(ch))
            {
                //TODO log error
                throw new KQMLExpectedWhitespaceException(Inbuf.ToString());
            }
            else
            {
                SkipWhitespace();
            }

        }

        private void SkipWhitespace()
        {
            bool done = false;
            while (!done)
            {
                char ch = (char)Reader.Peek();
                if (!char.IsWhiteSpace(ch))
                    done = true;
                else
                    ReadChar();
            }
        }

        public KQMLObject ReadPerformative()
        {
            SkipWhitespace();
            KQMLObject expr = ReadExpr();
            if (expr is KQMLList)
                return new KQMLPerformative((KQMLList)expr);
            else
                throw new KQMLExpectedListException(Inbuf.ToString());
        }
    }
}
