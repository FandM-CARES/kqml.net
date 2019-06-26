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
            _log.Debug("KQMLReader ctor called");
            
        }

        public void Close()
        {
            if (Reader != null)
            {
                try
                {
                    Reader.ReadToEnd();
                }
                catch (IOException e)
                {
                    // connection may be closed before reading to end.
                    // that's fine, just move on
                    _log.Debug("Connection may have been closed before reading to end");
                }
                Reader.Dispose();
                _log.Debug("KQMLReader closed");
            }
        }

        public void Dispose()
        {
            Close();
        }

        public char ReadChar()
        {
            char ch = (char)Reader.Read();
            Inbuf.Append(ch);
            return ch;

        }

        public static bool IsSpecial(char ch)
        {
            string specials = @"<>=+-*/&^~_@$%:.!?|";
            if (specials.Contains(ch))
                return true;
            return false;

        }

        /// <summary>
        /// Checks if a token is a char.
        /// </summary>
        /// <param name="ch">char to be checked</param>
        /// <returns>Returns false for empty or '`\"#(), otherwise returns true</returns>
        public static bool IsTokenChar(char ch)
        {
            string nonTokenChars = "'`\"#()";
            if (char.IsWhiteSpace(ch) || nonTokenChars.Contains(ch))
                return false;
            return true;
        }

        public KQMLObject ReadExpr(bool backquoted = false)
        {
            _log.Debug("Reading Expression from " + Reader.ToString());

            int peek = Reader.Peek();
            if (peek < 0)
            {
                throw new EndOfStreamException("EOF received: " + peek);
            }
            char ch = (char)peek;
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
                    _log.Error("Bad command: " + Inbuf.ToString());
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
                    _log.Error("Not a character: " + Inbuf.ToString());
                    throw new KQMLBadCharacterException(Inbuf.ToString());
                }
            }
        }

        public KQMLToken ReadToken()
        {
            char ch;
            StringBuilder buf = new StringBuilder();
            for (int i = 0; i < 1000000; i++)
            {
                ch = (char)Reader.Peek();
                if (IsTokenChar(ch))
                {
                    buf.Append(ch);
                    ReadChar();
                }
                else break;
            }

            return new KQMLToken(buf.ToString());
        }

        public KQMLQuotation ReadQuotation(bool backquoted)
        {
            char ch = ReadChar();
            if (ch == '`')
                return new KQMLQuotation(ch.ToString(), ReadExpr(true));
            else if (ch == '\'' || ch == ',')
                return new KQMLQuotation(ch.ToString(), ReadExpr(backquoted));
            else
                return null;
        }

        public KQMLString ReadString()
        {
            char ch = ReadChar();
            return ch == '"' ? ReadQuotedString() : ReadHashedString();
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
                    _log.Error("Invalid hashed string in ReadHashedString()");
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
                // If it is a slash, check next character
                if (ch == '\\')
                {
                    ch = ReadChar();
                    if (ch == '\\')
                    {
                        // It is another slash, preserve both slashes
                        Inbuf.Append("\\\\");
                        continue;
                    }
                }
                buf.Append(ch);
            }
            return new KQMLString(buf.ToString());
        }

        public object ReadListForFile()
        {
            //TODO: ReadListForFile
            throw new NotImplementedException();
        }

        public KQMLList ReadList(bool backquoted = false)
        {
            KQMLList lst = new KQMLList();
            char ch = ReadChar();
            if (ch != '(')
            {
                _log.Error($"BadOpenException: {Inbuf.ToString()}");
                throw new KQMLBadOpenException(Inbuf.ToString());
            }
            SkipWhitespace();
            for (int i = 0; i < 1000000; i++)
            {
                ch = (char)Reader.Peek();
                if (ch == ')')
                    break;
                lst.Append(ReadExpr(backquoted));
                ch = (char)Reader.Peek();
                if (ch == -1)
                    return lst;
                if (ch != ')')
                {
                    if (ch != '(')
                        ReadWhitespace();
                }
            }
            ch = ReadChar();
            if (ch != ')')
            {
                _log.Error($"BadCloseException: {Inbuf.ToString()}");
                throw new KQMLBadCloseException(Inbuf.ToString());

            }
            return lst;
        }

        public void ReadWhitespace()
        {
            char ch = ReadChar();
            if (!char.IsWhiteSpace(ch))
            {
                _log.Error(
                    $"ReadWhiteSpace called without whitespace: {Inbuf.ToString()}");
                throw new KQMLExpectedWhitespaceException(Inbuf.ToString());
            }
            else
            {
                SkipWhitespace();
            }

        }

        public void SkipWhitespace()
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
            Inbuf = new StringBuilder();
            SkipWhitespace();
            Inbuf = new StringBuilder();
            KQMLObject expr = ReadExpr();
            if (expr is KQMLList)
                return new KQMLPerformative((KQMLList)expr);
            else
            {
                _log.Error(
                    $"ReadPerformative expected list, got {Inbuf.ToString()}");
                throw new KQMLExpectedListException(Inbuf.ToString());
            }

        }
    }
}
