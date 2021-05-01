using System;
using System.Collections.Generic;

namespace CustomProject
{
    public struct Token
    {
        public static Token EOF { get => new Token(Kind.EOF, "", null); }

        public enum Kind
        {
            EOF,
            Error,
            EndStatement,
            Comma,

            DelimOpenParenthesis,
            DelimCloseParenthesis,
            DelimOpenBracket,
            DelimCloseBracket,

            LiteralNil,
            LiteralBoolean,
            LiteralNumber,
            LiteralString,
            LiteralChar,

            Identifier,

            KeywordVar,
            KeywordConst,
            KeywordFn,
            KeywordClass,
            KeywordEnd,
            KeywordIf,
            KeywordElif,
            KeywordElse,
            KeywordWhile,
            KeywordBreak,
            KeywordContinue,
            KeywordReturn,
            KeywordPrint,

            OpBang,
            OpPlus,
            OpDash,
            OpStar,
            OpSlash,
            OpEqual,
            OpDoubleEqual,
            OpBangEqual,
            OpOr,
            OpAnd,
            OpLeftAngle,
            OpRightAngle,
            OpLeftAngleEqual,
            OpRightAngleEqual,
            OpDot,
        }

        public Kind kind;
        public string source;
        public Value value;

        public Token(Kind kind, string source, Value value)
        {
            this.kind = kind;
            this.source = source;
            this.value = value;
        }

        public override string ToString()
        {
            return string.Format("{{ {0}, \"{1}\", {2} }}", kind, source.Replace(Environment.NewLine, "\\n"), value);
        }
    }
    
    public class Tokenizer
    {
        private string source;
        private int tokenStart;
        private int tokenLength;

        private char CurrentChar
        {
            get
            {
                if (tokenStart + tokenLength < source.Length)
                {
                    return source[tokenStart + tokenLength];
                }
                else
                {
                    return '\0';
                }
            }
        }

        private char PreviousChar
        {
            get
            {
                int idx = tokenStart + tokenLength - 1;
                if (idx < source.Length && idx >= 0)
                {
                    return source[idx];
                }
                else
                {
                    return '\0';
                }
            }
        }

        private string TokenString
        {
            get
            {
                if (tokenStart >= source.Length)
                {
                    return "";
                }
                else
                {
                    int length = Math.Min(source.Length, tokenStart + tokenLength) - tokenStart;
                    return source.Substring(tokenStart, length);
                }
            }
        }

        public Tokenizer()
        {
            source = null;
            tokenStart = 0;
            tokenLength = 0;
        }

        public List<Token> Tokenize(string source)
        {
            this.source = source;
            var tokens = new List<Token>();

            while (tokenStart < source.Length)
            {
                Token token = ProcessToken();
                tokens.Add(token);
                tokenStart += tokenLength;
                if (ProcessPossibleEndStatement(out Token eos))
                {
                    tokens.Add(eos);
                    tokenStart += tokenLength;
                }
            }

            tokens.Add(Token.EOF);

            return tokens;
        }

        private void SkipWhitespace()
        {
            while (tokenStart < source.Length && char.IsWhiteSpace(source[tokenStart]))
            {
                tokenStart++;
            }
        }

        private Token ProcessToken()
        {
            tokenLength = 0;

            Token token;

            SkipWhitespace();

            if (tokenStart == source.Length)
            {
                return Token.EOF;
            }

            if (char.IsDigit(source[tokenStart]))
            {
                token = ProcessNumber();
            }
            else if (source[tokenStart] == '"')
            {
                token = ProcessString();
            }
            else if (source[tokenStart] == '\'')
            {
                token = ProcessCharacter();
            }
            else if (char.IsLetter(source[tokenStart]))
            {
                token = ProcessIdentifierOrKeyword();
            }
            else
            {
                token = ProcessOperator();
            }

            return token;
        }

        private Token ProcessNumber()
        {
            do
            {
                tokenLength++;
            } while (char.IsDigit(CurrentChar));

            if (CurrentChar == '.')
            {
                do
                {
                    tokenLength++;
                } while (char.IsDigit(CurrentChar));
            }

            string numberString = TokenString;
            bool success = float.TryParse(numberString, out float number);

            if (!success)
            {
                return new Token(Token.Kind.Error, numberString, null);
            }

            Value numberVal = new NumberValue(number);
            return new Token(Token.Kind.LiteralNumber, numberString, numberVal);
        }

        private Token ProcessString()
        {
            // skip " character
            tokenStart++;
            do
            {
                tokenLength++;
            } while (CurrentChar != '"');

            string stringString = TokenString;
            Value stringVal = new StringValue(stringString);
            Token token = new Token(Token.Kind.LiteralString, stringString, stringVal);

            // skip last " character
            tokenLength++;

            return token;
        }

        private Token ProcessCharacter()
        {
            // skip ' character
            tokenStart++;
            do
            {
                tokenLength++;
            } while (CurrentChar != '\'');

            if (tokenLength != 1)
            {
                return new Token(Token.Kind.Error, TokenString, null);
            }

            string charString = TokenString;
            Value charVal = new CharValue(charString[0]);
            Token token = new Token(Token.Kind.LiteralChar, charString, charVal);

            // skip last ' character
            tokenLength++;

            return token;
        }

        private Token ProcessIdentifierOrKeyword()
        {
            do
            {
                tokenLength++;
            } while (char.IsLetterOrDigit(CurrentChar));

            string word = TokenString;

            Token.Kind kind;
            Value value = null;
            switch (word)
            {
                case "nil":
                    kind = Token.Kind.LiteralNil;
                    value = new NilValue();
                    break;

                case "true":
                    kind = Token.Kind.LiteralBoolean;
                    value = new BooleanValue(true);
                    break;

                case "false":
                    kind = Token.Kind.LiteralBoolean;
                    value = new BooleanValue(false);
                    break;

                case "or":
                    kind = Token.Kind.OpOr;
                    break;

                case "and":
                    kind = Token.Kind.OpAnd;
                    break;

                case "var":
                    kind = Token.Kind.KeywordVar;
                    break;

                case "const":
                    kind = Token.Kind.KeywordConst;
                    break;

                case "fn":
                    kind = Token.Kind.KeywordFn;
                    break;

                case "class":
                    kind = Token.Kind.KeywordClass;
                    break;

                case "end":
                    kind = Token.Kind.KeywordEnd;
                    break;

                case "if":
                    kind = Token.Kind.KeywordIf;
                    break;

                case "elif":
                    kind = Token.Kind.KeywordElif;
                    break;

                case "else":
                    kind = Token.Kind.KeywordElse;
                    break;

                case "while":
                    kind = Token.Kind.KeywordWhile;
                    break;

                case "break":
                    kind = Token.Kind.KeywordBreak;
                    break;

                case "continue":
                    kind = Token.Kind.KeywordContinue;
                    break;

                case "return":
                    kind = Token.Kind.KeywordReturn;
                    break;

                case "print":
                    kind = Token.Kind.KeywordPrint;
                    break;

                default:
                    kind = Token.Kind.Identifier;
                    break;
            }

            return new Token(kind, word, value);
        }

        private Token ProcessOperator()
        {
            tokenLength = 1;
            Token.Kind kind;
            switch (PreviousChar)
            {
                case '.':
                    kind = Token.Kind.OpDot;
                    break;
                case ',':
                    kind = Token.Kind.Comma;
                    break;

                case '(':
                    kind = Token.Kind.DelimOpenParenthesis;
                    break;
                case ')':
                    kind = Token.Kind.DelimCloseParenthesis;
                    break;
                case '[':
                    kind = Token.Kind.DelimOpenBracket;
                    break;
                case ']':
                    kind = Token.Kind.DelimCloseBracket;
                    break;

                case '!':
                    if (CurrentChar == '=')
                    {
                        tokenLength++;
                        kind = Token.Kind.OpBangEqual;
                    }
                    else
                    {
                        kind = Token.Kind.OpBang;
                    }
                    break;
                case '+':
                    kind = Token.Kind.OpPlus;
                    break;
                case '-':
                    kind = Token.Kind.OpDash;
                    break;
                case '*':
                    kind = Token.Kind.OpStar;
                    break;
                case '/':
                    kind = Token.Kind.OpSlash;
                    break;
                case '=':
                    if (CurrentChar == '=')
                    {
                        tokenLength++;
                        kind = Token.Kind.OpDoubleEqual;
                    }
                    else
                    {
                        kind = Token.Kind.OpEqual;
                    }
                    break;
                case '<':
                    if (CurrentChar == '=')
                    {
                        tokenLength++;
                        kind = Token.Kind.OpLeftAngleEqual;
                    }
                    else
                    {
                        kind = Token.Kind.OpLeftAngle;
                    }
                    break;
                case '>':
                    if (CurrentChar == '=')
                    {
                        tokenLength++;
                        kind = Token.Kind.OpRightAngleEqual;
                    }
                    else
                    {
                        kind = Token.Kind.OpRightAngle;
                    }
                    break;

                default:
                    kind = Token.Kind.Error;
                    break;
            }
            string opString = TokenString;
            return new Token(kind, opString, null);
        }

        private bool ProcessPossibleEndStatement(out Token eos)
        {
            tokenLength = 0;
            if (tokenStart == source.Length || CurrentChar == '\n')
            {
                tokenLength = 1;
                eos = new Token(Token.Kind.EndStatement, TokenString, null);
                return true;
            }
            eos = new Token();
            return false;
        }
    }
}
