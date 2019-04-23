using System;
using System.IO;
using System.Linq;
using System.Text;
using KQML.KQMLExceptions;

namespace KQML
{
    public class KQMLReader
    {
        public StreamReader Reader;
        public StringBuilder Buff;

        public KQMLReader(StreamReader r)
        {
            Reader = r;
            Buff = new StringBuilder();
        }

        public void Close()
        {
            Reader.Close();
        
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
            if (!(ch == ' ') && nonTokenChars.Contains(ch))
                return true;
            return false;
        }

        public KQMLObject ReadExpr(bool backquoted=false)
        {
            char ch = (char)Reader.Peek();
            if (ch == '\'' || ch == '`')
                return ReadQuotation(backquoted);
            else if (ch == '"' || ch == '#')
                return ReadString();
            else if (ch == '(')
                return ReadList(backquoted);
            else if (ch ==',')
            {
                if (!backquoted)
                {
                    ch = ReadChar();
                    throw new KQMLBadCommandException(Buff.ToString());
                }
                else
                    ReadQuotation(backquoted);
                
            }
            else
            {
                if (IsTokenChar(ch))
                    return ReadToken();
                else
                {
                    ch = ReadChar();
                    throw new KQMLBadCharacterException(Buff.ToString());
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
                    Buff.Append(ch);
                    ReadChar();
                }
                else break;
            }

            return new KQMLToken(Buff.ToString());
        }

        public KQMLList ReadList(bool backquoted)
        {
            throw new NotImplementedException();
        }

        public KQMLQuotation ReadQuotation(bool backquoted)
        {
            //TODO
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
            throw new NotImplementedException();
        }

        public KQMLString ReadQuotedString()
        {
            Buff = new StringBuilder();
            
        }
    }
}
