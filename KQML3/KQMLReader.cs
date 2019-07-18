using System;
using System.Collections;
using System.Collections.Generic;
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
        
        /// <summary>
        /// Initializes a new instance of the <see cref="KQMLReader"/> class with the specified <see cref="StreamReader"/> 
        /// </summary>
        /// <param name="r">The <see cref="StreamReader"/>to be wrapped</param>
        public KQMLReader(StreamReader r)
        {
            Reader = r;
            Inbuf = new StringBuilder();
            _log.Debug("KQMLReader ctor called");
            
        }

        /// <summary>
        /// Closes the <see cref="KQMLReader"/>
        /// </summary>
        public void Close()
        {
            if (Reader != null)
            {
                try
                {
                    Reader.ReadToEnd();
                }
                catch (IOException)
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

        /// <summary>
        /// Reads the next character of the input stream
        /// </summary>
        /// <returns>The next character from the input stream</returns>
        public char ReadChar()
        {
            char ch = (char)Reader.Read();
            Inbuf.Append(ch);
            return ch;

        }

        /// <summary>
        /// Determines if a character is a special character
        /// </summary>
        /// <param name="ch"></param>
        /// <returns><c>true</c> it is special, <c>false</c> if otherwise</returns>
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

        /// <summary>
        /// Reads the next expression in the input stream
        /// </summary>
        /// <param name="backquoted"></param>
        /// <returns>A KQML representation of the next expression</returns>
        public KQMLObject ReadExpr(bool backquoted = false)
        {
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

        /// <summary>
        /// Read until the next non-char and treat result as a <see cref="KQMLToken"/>
        /// </summary>
        /// <returns>A <see cref="KQMLToken"/> representing the next group of characters</returns>
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

        /// <summary>
        /// Read the next group of characters as a <see cref="KQMLQuotation"/>
        /// </summary>
        /// <returns>A <see cref="KQMLQuotation"/> that representing the next group of characters</returns>
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

        /// <summary>
        /// Read the next group of characters as a <see cref="KQMLString"/>
        /// </summary>
        /// <returns>KQML representation of the next group of characters</returns>
        public KQMLString ReadString()
        {
            char ch = ReadChar();
            return ch == '"' ? ReadQuotedString() : ReadHashedString();
        }

        /// <summary>
        /// Read the next group of characters as a <see cref="KQMLString"/> but hashed
        /// </summary>
        /// <returns>KQML representation of the next group of characters</returns>
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

        /// <summary>
        /// Read the next group of characters as a <see cref="KQMLString" /> but quoted
        /// </summary>
        /// <returns>KQML representation of the next group of characters</returns>
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

        /// <summary>
        /// Read the next group of characters a <see cref="KQMLList"/>
        /// </summary>
        /// <param name="backquoted">Whether the list is backquoted</param>
        /// <returns>KQML representation of the next group of characters as a list</returns>
        /// <exception cref="KQMLBadOpenException">If the next group of characters does not begin with "("</exception>
        /// <exception cref="KQMLBadCloseException">If the next expression does not end with ")"</exception>
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

        /// <summary>
        /// Read the next whitespace
        /// </summary>
        /// <exception cref="KQMLExpectedWhitespaceException">When the next character is not a whitespace</exception>
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

        /// <summary>
        /// Reads over consecutive incoming whitespaces
        /// </summary>
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

        /// <summary>
        /// Reads the next group of characters as a <see cref="KQMLPerformative"/>
        /// </summary>
        /// <returns>KQML representation of the next group of characters</returns>
        /// <exception cref="KQMLExpectedListException">when the next group of characters cannot be represented as a <see cref="KQMLList"/></exception>
        public KQMLObject ReadPerformative()
        {
            Inbuf = new StringBuilder();
            SkipWhitespace();
            Inbuf = new StringBuilder();
            KQMLObject expr = ReadExpr();
            if (expr is KQMLList exprList)
                return KQMLPerformative.ListToPerformative(exprList);
            else
            {
                _log.Error(
                    $"ReadPerformative expected list, got {Inbuf.ToString()}");
                throw new KQMLExpectedListException(Inbuf.ToString());
            }

        }
    }
}
