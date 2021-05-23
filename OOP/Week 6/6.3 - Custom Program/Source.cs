// Program.cs
using System;

namespace CustomProject
{
    class MainClass
    {
        public static void Main(string[] args)
        {
            if (args.Length > 0)
            {
                string filepath = args[0];
                var interpreter = new Interpreter();
                interpreter.Interpret(filepath);
            }
            else
            {
                Console.WriteLine("No filepath given!");
            }
        }
    }
}

// Interpreter.cs
using System;
using System.IO;
using System.Text;

namespace CustomProject
{
    /// <summary>
    /// the main class for interpreting code.
    /// Is made of a tokenizer, parser and virtual machine.
    /// </summary>
    public class Interpreter
    {
        /// <summary>
        /// Responsible for turning raw source code into a list of tokens.
        /// </summary>
        private Tokenizer tokenizer;

        /// <summary>
        /// Responsible for turning a list of tokens into an AST.
        /// </summary>
        private Parser parser;

        /// <summary>
        /// Is used to execute AST and store its outputs.
        /// </summary>
        private VM vm;

        public Interpreter()
        {
            tokenizer = new Tokenizer();
            parser = new Parser();
            vm = new VM();
            vm.SetEnvironment(null, vm);
            LoadPrelude();
        }

        /// <summary>
        /// Interprets a single file of source code and executes it.
        /// </summary>
        /// <param name="filepath">filepath to source code.</param>
        public void Interpret(string filepath)
        {
            string source = GetSourceCode(filepath);
            var tokens = tokenizer.Tokenize(source);
            var program = parser.Parse(tokens);
            vm.Execute(program);
        }

        /// <summary>
        /// Opens and reads the given file and returns it as a single string.
        /// </summary>
        /// <param name="filepath">File to read.</param>
        /// <returns>Source code written in the file.</returns>
        private static string GetSourceCode(string filepath)
        {
            FileStream fstream = File.OpenRead(filepath);
            byte[] bytes = new byte[fstream.Length];
            fstream.Read(bytes, 0, (int)fstream.Length);
            return Encoding.ASCII.GetString(bytes);
        }

        /// <summary>
        /// Interprets source code for the languages prelude.
        /// </summary>
        private void LoadPrelude()
        {
            const string preludeSource =
            @"
            class String
                fn class.concat(*ss)
                    var result = """"
                    for s in ss
                        result.concat(s)
                    result
            ";

            var preludeTokens = tokenizer.Tokenize(preludeSource);
            var prelude = parser.Parse(preludeTokens);
            vm.Execute(prelude);
        }
    }
}

// Tokenizer.cs
using System;
using System.Collections.Generic;

namespace CustomProject
{
    /// <summary>
    /// A Token is a combination of characters with a semantic meaning
    /// in the language.
    /// </summary>
    public struct Token
    {
        /// <summary>
        /// The different semantic meanings of different combinations
        /// of characters.
        /// </summary>
        public enum Kind
        {
            EOF,
            EOL,
            Error,
            EndStatement,
            Comma,

            DelimOpenParenthesis,
            DelimCloseParenthesis,
            DelimOpenBracket,
            DelimCloseBracket,
            DelimPipe,

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
            KeywordSuper,
            KeywordIf,
            KeywordElif,
            KeywordElse,
            KeywordWhile,
            KeywordFor,
            KeywordIn,
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
            OpDoubleDot,
            OpDoubleDotEqual,
        }

        public Kind kind;
        public int indentation;
        public string source;
        public Value value;

        public Token(Kind kind, int indentation, string source, Value value)
        {
            this.kind = kind;
            this.indentation = indentation;
            this.source = source;
            this.value = value;
        }

        /// <value>New token where kind = EOF.</value>
        public static Token EOF { get => new Token(Kind.EOF, -1, "", null); }
        /// <value>New token where kind = EOL.</value>
        public static Token EOL { get => new Token(Kind.EOL, -1, "", null); }

        /// <summary>
        /// Wrapper for making Error tokens
        /// </summary>
        /// <param name="err">Error message.</param>
        /// <param name="indentation">Indentation level for this token.</param>
        /// <returns>An error token with error message.</returns>
        public static Token Error(string err, int indentation)
        {
            return new Token(Kind.Error, indentation, err, null);
        }

        public override string ToString()
        {
            return string.Format("{{ {0}, \"{1}\", {2} }}", kind, source.Replace(Environment.NewLine, "\\n"), value);
        }
    }

    /// <summary>
    /// Tokenizer is used to turn source code into a list of tokens
    /// </summary>
    public class Tokenizer
    {
        private string source;
        private int tokenStart;
        private int tokenLength;
        private int currentIndentation;

        /// <value>Gets the current character or 0 if at the end of the file.</value>
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

        /// <value>Gets the previous character or 0 if at the end of the file.</value>
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

        /// <value>Gets the next character or 0 if at the end of the file.</value>
        private char NextChar
        {
            get
            {
                int idx = tokenStart + tokenLength + 1;
                if (idx < source.Length)
                {
                    return source[idx];
                }
                else
                {
                    return '\0';
                }
            }
        }

        /// <value>Gets the source code of current token.</value>
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
            currentIndentation = 0;
        }

        /// <summary>
        /// Tokenizes given source code into a list of tokens.
        /// </summary>
        /// <param name="source">Source code to tokenize.</param>
        /// <returns>A list of tokens generated from the given source code.</returns>
        public List<Token> Tokenize(string source)
        {
            tokenStart = 0;
            tokenLength = 0;
            currentIndentation = 0;
            
            var tokens = new List<Token>();

            foreach (var line in source.Split('\n'))
            {
                if (line.Length == 0)
                    continue;

                this.source = line;
                tokenStart = 0;

                int oldIndentation = currentIndentation;
                ProcessIndentation();
                if (tokenStart == line.Length)
                {
                    currentIndentation = oldIndentation;
                    continue;
                }

                while (tokenStart < line.Length)
                {
                    Token token = ProcessToken();
                    if (token.kind != Token.Kind.EOL)
                        tokens.Add(token);
                    tokenStart += tokenLength;
                }

                tokens.Add(new Token(Token.Kind.EndStatement, currentIndentation, "", null));
                currentIndentation = 0;
            }

            tokens.Add(Token.EOF);

            return tokens;
        }

        /// <summary>
        /// Nudges 'tokenStart' forward until either a non-whitespace
        /// character or the end of the file is reached.
        /// </summary>
        private void SkipWhitespace()
        {
            while (tokenStart < source.Length && char.IsWhiteSpace(source[tokenStart]))
            {
                tokenStart++;
            }
        }

        /// <summary>
        /// Sets the current indentation for a line.
        /// Nudges 'tokenStart' forward until either a non-whitespace
        /// character or the end of the file is reached.
        /// </summary>
        private void ProcessIndentation()
        {
            while (tokenStart < source.Length && char.IsWhiteSpace(source[tokenStart]))
            {
                currentIndentation++;
                tokenStart++;
            }
        }

        /// <summary>
        /// Generates Single token from the source code.
        /// </summary>
        /// <returns>The Token that correspondes to the current glob of source code</returns>
        private Token ProcessToken()
        {
            tokenLength = 0;

            Token token;

            SkipWhitespace();

            if (tokenStart == source.Length)
            {
                return Token.EOL;
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

        /// <summary>
        /// Processes the source code assuming that it represents a number literal.
        /// </summary>
        /// <returns>A number token if success or an error token if not.</returns>
        private Token ProcessNumber()
        {
            do
            {
                tokenLength++;
            } while (char.IsDigit(CurrentChar));

            if (CurrentChar == '.' && char.IsDigit(NextChar))
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
                return Token.Error(numberString, currentIndentation);
            }

            Value numberVal = new NumberValue(number);
            return new Token(Token.Kind.LiteralNumber, currentIndentation, numberString, numberVal);
        }

        /// <summary>
        /// Processes the source code assuming that it represents a string literal.
        /// </summary>
        /// <returns>A string token if success or an error token if not.</returns>
        private Token ProcessString()
        {
            // skip " character
            tokenStart++;
            while (CurrentChar != '"' && CurrentChar != '\0')
            {
                tokenLength++;
            }

            if (CurrentChar != '"')
            {
                return Token.Error("Expected '\"' to terminate string literal not found.", currentIndentation);
            }

            string stringString = TokenString;
            Value stringVal = new StringValue(stringString);
            Token token = new Token(Token.Kind.LiteralString, currentIndentation, stringString, stringVal);

            // skip last " character
            tokenLength++;

            return token;
        }

        /// <summary>
        /// Processes the source code assuming that it represents a char literal.
        /// </summary>
        /// <returns>A character token if success or an error token if not.</returns>
        private Token ProcessCharacter()
        {
            // skip ' character
            tokenStart++;
            while (CurrentChar != '\'' && CurrentChar != '\0')
            {
                tokenLength++;
            }

            if (tokenLength != 1 || CurrentChar != '\'')
            {
                return Token.Error(TokenString, currentIndentation);
            }

            string charString = TokenString;
            Value charVal = new CharValue(charString[0]);
            Token token = new Token(Token.Kind.LiteralChar, currentIndentation, charString, charVal);

            // skip last ' character
            tokenLength++;

            return token;
        }

        /// <summary>
        /// Processes the source assuming that it represents either an identifier
        /// or a keyword of the language.
        /// </summary>
        /// <returns>Either an identifier token or a keyword token.</returns>
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

                case "super":
                    kind = Token.Kind.KeywordSuper;
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

                case "for":
                    kind = Token.Kind.KeywordFor;
                    break;

                case "in":
                    kind = Token.Kind.KeywordIn;
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

            return new Token(kind, currentIndentation, word, value);
        }

        /// <summary>
        /// Processes the source code assuming that it represents an opearator.
        /// </summary>
        /// <returns>An operator token if success or an error token if not.</returns>
        private Token ProcessOperator()
        {
            tokenLength = 1;
            Token.Kind kind;
            switch (PreviousChar)
            {
                case '.':
                    if (CurrentChar == '.')
                    {
                        tokenLength++;
                        if (CurrentChar == '=')
                        {
                            tokenLength++;
                            kind = Token.Kind.OpDoubleDotEqual;
                        }
                        else
                        {
                            kind = Token.Kind.OpDoubleDot;
                        }
                    }
                    else
                    {
                        kind = Token.Kind.OpDot;
                    }
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
                case '|':
                    kind = Token.Kind.DelimPipe;
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
            return new Token(kind, currentIndentation, opString, null);
        }
    }
}

// Parser.cs
using System;
using System.Collections.Generic;

namespace CustomProject
{
    /// <summary>
    /// Used to turn a list of tokens into an Abstract Syntax Tree.
    /// </summary>
    public class Parser
    {
        /// <summary>
        /// Each token is associated with a precedence level which is used to
        /// parse the tokens into the correct AST. For example:
        ///<c>1 + 2 * 3</c>.
        /// The <c>+</c> operator has a lower precedence than the <c>*</c> operator
        /// which means the <c>2</c> becomes the left hand side of the <c>*</c> operators
        /// AST node instead of it becoming the right hand side of the <c>+</c> operators
        /// AST node.
        /// </summary>
        private enum Precedence
        {
            None,
            Assignment, // =
            Or,         // or
            And,        // and
            Equality,   // == !=
            Comparison, // < > <= >=
            Term,       // + -
            Factor,     // * / %
            Unary,      // !
            Call,       // . () []
            Primary,
        }

        /// <summary>
        /// Used to distinguish between instantiation of a variable or a constant
        /// </summary>
        /// <example>
        /// <code>
        /// ParseVariableInstantiation(Variable);
        /// ParseVariableInstantiation(Constant);
        /// </code>
        /// </example>
        private enum InstantiationKind
        {
            Variable,
            Constant,
        }

        /// <summary>
        /// Signiture for a <see cref="ParseRule"/>s hook into the parser's private parsing
        /// methods.
        /// </summary>
        delegate void ParseFn(Parser p);

        /// <summary>
        /// Used to associate a token with a <see cref="Precedence"/> level, a prefix parsing method
        /// and an infix parsing method.
        /// </summary>
        private struct ParseRule
        {
            public Precedence precedence;
            public ParseFn prefix;
            public ParseFn infix;

            public ParseRule(Precedence precedence, ParseFn prefix, ParseFn infix)
            {
                this.precedence = precedence;
                this.prefix = prefix;
                this.infix = infix;
            }
        }

        /// <summary>
        /// Look up table for the parse rules of every <see cref="Token.Kind"/>.
        /// </summary>
        /// <remarks>
        /// Use <see cref="GetRule"/> to access a token's <see cref="ParseRule"/>.
        /// </remarks>
        private static ParseRule[] parseRules = new ParseRule[]
        {
            new ParseRule(Precedence.None, null, null), // EOF
            new ParseRule(Precedence.None, null, null), // EOL
            new ParseRule(Precedence.None, null, null), // Error
            new ParseRule(Precedence.None, null, null), // EndStatement
            new ParseRule(Precedence.None, null, null), // Comma

            new ParseRule(Precedence.Call, GroupParseFn, InvokeParseFn), // DelimOpenParenthesis
            new ParseRule(Precedence.None, null, null), // DelimCloseParenthesis
            new ParseRule(Precedence.Call, ListParseFn, BinaryParseFn), // DelimOpenBracket
            new ParseRule(Precedence.None, null, null), // DelimCloseBracket
            new ParseRule(Precedence.None, LambdaParseFn, null), // DelimPipe

            new ParseRule(Precedence.None, LiteralParseFn, null), // LiteralNil
            new ParseRule(Precedence.None, LiteralParseFn, null), // LiteralBoolean
            new ParseRule(Precedence.None, LiteralParseFn, null), // LiteralNumber
            new ParseRule(Precedence.None, LiteralParseFn, null), // LiteralString
            new ParseRule(Precedence.None, LiteralParseFn, null), // LiteralChar

            new ParseRule(Precedence.None, IdentifierParseFn, null), // Identifier

            new ParseRule(Precedence.None, null, null), // KeywordVar
            new ParseRule(Precedence.None, null, null), // KeywordConst
            new ParseRule(Precedence.None, null, null), // KeywordFn
            new ParseRule(Precedence.None, null, null), // KeywordClass
            new ParseRule(Precedence.None, null, null), // KeywordSuper
            new ParseRule(Precedence.None, null, null), // KeywordIf
            new ParseRule(Precedence.None, null, null), // KeywordElif
            new ParseRule(Precedence.None, null, null), // KeywordElse
            new ParseRule(Precedence.None, null, null), // KeywordWhile
            new ParseRule(Precedence.None, null, null), // KeywordFor
            new ParseRule(Precedence.None, null, null), // KeywordIn
            new ParseRule(Precedence.None, null, null), // KeywordBreak
            new ParseRule(Precedence.None, null, null), // KeywordContinue
            new ParseRule(Precedence.None, null, null), // KeywordReturn
            new ParseRule(Precedence.None, null, null), // KeywordPrint

            new ParseRule(Precedence.Unary, UnaryParseFn, null), // OpBang
            new ParseRule(Precedence.Term, null, BinaryParseFn), // OpPlus
            new ParseRule(Precedence.Term, UnaryParseFn, BinaryParseFn), // OpDash
            new ParseRule(Precedence.Factor, null, BinaryParseFn), // OpStar
            new ParseRule(Precedence.Factor, null, BinaryParseFn), // OpSlash
            new ParseRule(Precedence.Assignment, null, AssigmentParseFn), // OpEqual
            new ParseRule(Precedence.Equality, null, BinaryParseFn), // OpDoubleEqual
            new ParseRule(Precedence.Equality, null, BinaryParseFn), // OpBangEqual
            new ParseRule(Precedence.Or, null, BinaryParseFn), // OpOr
            new ParseRule(Precedence.And, null, BinaryParseFn), // OrAnd
            new ParseRule(Precedence.Comparison, null, BinaryParseFn), // OpLeftAngle
            new ParseRule(Precedence.Comparison, null, BinaryParseFn), // OpRightAngle
            new ParseRule(Precedence.Comparison, null, BinaryParseFn), // OpLeftAngleEqual
            new ParseRule(Precedence.Comparison, null, BinaryParseFn), // OpRightAngleEqual
            new ParseRule(Precedence.Call, null, MemberReferenceParseFn), // OpDot
            new ParseRule(Precedence.Or, null, BinaryParseFn), // OpDoubleDot
            new ParseRule(Precedence.Or, null, BinaryParseFn), // OpDoubleDotEqual
        };

        /// <summary>
        /// Gets the <see cref="ParseRule"/> for a given kind of token.
        /// </summary>
        /// <param name="kind">The kind of token that is associated with the desired <see cref="ParseRule"/></param>
        /// <returns>The <see cref="ParseRule"/> associated with the given kind of token.</returns>
        private static ParseRule GetRule(Token.Kind kind)
        {
            return parseRules[(int)kind];
        }

        private List<Token> tokens;
        private bool error;
        private int peekIndex;
        private int loopDepth;
        private int lambdaDepth;

        /// <summary>
        /// Used as a intermediate result whilst building the desired AST.
        /// </summary>
        private IAST ast;

        /// <value>The previous token in the token list.</value>
        private Token Previous { get => tokens[peekIndex - 1]; }

        /// <value>The current token in the token list.</value>
        private Token Current
        {
            get
            {
                if (peekIndex >= tokens.Count)
                {
                    return Token.EOF;
                }
                return tokens[peekIndex];
            }
        }

        public Parser()
        {
        }

        /// <summary>
        /// Advances to the next token. Guards against going outside the bounds
        /// of the token list.
        /// </summary>
        private void Advance()
        {
            if (peekIndex < tokens.Count)
            {
                peekIndex++;
            }
        }

        /// <summary>
        /// <list type="bullet">
        /// <item>Flags that an error has occured.</item>
        /// <item>Prints error message to the console.</item>
        /// <item>Advances until the beginning of the next statement.</item>
        /// </list>
        /// </summary>
        /// <param name="err">Exception that encapsulates the error message.</param>
        private void HandleError(Exception err)
        {
            error = true;
            Console.WriteLine(err.Message);
            while (!CheckPrevious(Token.Kind.EndStatement) && !Check(Token.Kind.EOF))
            {
                Advance();
            }
        }

        /// <summary>
        /// Checks if <see cref="Current"/> is the given token kind.
        /// </summary>
        /// <param name="kind">kind of token to check for.</param>
        /// <returns>True if 'Current' is the correct kind. False if not.</returns>
        private bool Check(Token.Kind kind)
        {
            return Current.kind == kind;
        }

        /// <summary>
        /// Checks if <see cref="Previous"/> is the given token kind.
        /// </summary>
        /// <param name="kind">kind of token to check for.</param>
        /// <returns>True if <see cref="Previous"/> is the correct kind. False if not.</returns>
        private bool CheckPrevious(Token.Kind kind)
        {
            return Previous.kind == kind;
        }

        /// <summary>
        /// Advances over <see cref="Current"/> if it matches the given kind.
        /// </summary>
        /// <param name="kind">Kind of token to match against.</param>
        /// <returns>True if it was the correct kind. False if not.</returns>
        private bool Next(Token.Kind kind)
        {
            if (!Check(kind))
                return false;
            Advance();
            return true;
        }

        /// <summary>
        /// Asserts that <see cref="Current"/> is the given kind.
        /// </summary>
        /// <param name="kind">Expected token kind.</param>
        /// <param name="format">Error message format.</param>
        /// <param name="args">Arguments for the given error message format.</param>
        /// <exception cref="Exception">
        /// If 'Current' was not the correct kind then an exception with the error message
        /// is thrown.
        /// </exception>
        private void Expect(Token.Kind kind, string format, params object[] args)
        {
            Assert(Next(kind), format, args);
        }

        /// <summary>
        /// Asserts that a condition is 'true'. throws if 'false'.
        /// </summary>
        /// <param name="condition">Condition to verify.</param>
        /// <param name="format">Format of the error message.</param>
        /// <param name="args">Arguments to be supplied to the format.</param>
        /// <exception cref="Exception">
        /// If 'condition' was 'false' then an exception with the error message is thrown.
        /// </exception>
        private void Assert(bool condition, string format, params object[] args)
        {
            if (!condition)
            {
                Error(format, args);
            }
        }

        /// <summary>
        /// Throws the generated error message as an exception.
        /// </summary>
        /// <param name="format">Format of the error string.</param>
        /// <param name="args">Arguments to be supplied to the format.</param>
        /// <exception cref="Exception"></exception>
        private void Error(string format, params object[] args)
        {
            throw new Exception(string.Format(format, args));
        }

        /// <summary>
        /// Generates an Abstract Syntax Tree from the given list of tokens.
        /// </summary>
        /// <param name="tokens">List of tokens to parse.</param>
        /// <returns>The generated Abstract Syntax Tree.</returns>
        public List<IAST> Parse(List<Token> tokens)
        {
            error = false;
            peekIndex = 0;
            loopDepth = 0;
            lambdaDepth = 0;

            this.tokens = tokens;
            List<IAST> program = new List<IAST>();

            while (!Check(Token.Kind.EOF))
            {
                try
                {
                    ParseDeclaration();
                    program.Add(ast);
                }
                catch (Exception e)
                {
                    HandleError(e);
                }
            }

            if (error)
            {
                program.Clear();
            }

            return program;
        }

        /// <summary>
        /// Parses a declaration. If no declaration to parse it attempts to parse
        /// a statement.
        /// </summary>
        /// <remarks>
        /// When this method returns, <see cref="ast"/> will be the AST that represents
        /// the declaration.
        /// </remarks>
        /// <exception cref="Exception">If there is a parse error.</exception>
        private void ParseDeclaration()
        {
            if (Next(Token.Kind.KeywordVar))
            {
                ParseVariableInstantiation(InstantiationKind.Variable);
                Expect(Token.Kind.EndStatement, "Unexpected expression(s) at the end of statement");
            }
            else if (Next(Token.Kind.KeywordConst))
            {
                ParseVariableInstantiation(InstantiationKind.Constant);
                Expect(Token.Kind.EndStatement, "Unexpected expression(s) at the end of statement");
            }
            else if (Next(Token.Kind.KeywordFn))
            {
                ParseFunctionDeclaration();
            }
            else if (Next(Token.Kind.KeywordClass))
            {
                ParseClassDeclaration();
            }
            else
            {
                ParseStatement();
            }
        }

        /// <summary>
        /// Parses a statement. if no statement to parse it attempts to parse
        /// an expression.
        /// </summary>
        /// <remarks>
        /// When this method returns, <see cref="ast"/> will be the AST that represents the
        /// statement.
        /// </remarks>
        /// <exception cref="Exception">If there is a parse error.</exception>
        private void ParseStatement()
        {
            if (Next(Token.Kind.KeywordIf))
            {
                ParseIfStatement();
            }
            else if (Next(Token.Kind.KeywordWhile))
            {
                ParseWhileStatement();
            }
            else if (Next(Token.Kind.KeywordFor))
            {
                ParseForStatement();
            }
            else if (Next(Token.Kind.KeywordBreak))
            {
                Assert(loopDepth != 0, "Encountered 'break' statement outside of a loop.");
                ParseBreakStatement();
                Expect(Token.Kind.EndStatement, "Unexpected expression(s) at the end of statement");
            }
            else if (Next(Token.Kind.KeywordContinue))
            {
                Assert(loopDepth != 0, "Encountered 'continue' statement outside of a loop.");
                ParseContinueStatement();
                Expect(Token.Kind.EndStatement, "Unexpected expression(s) at the end of statement");
            }
            else if (Next(Token.Kind.KeywordReturn))
            {
                Assert(lambdaDepth != 0, "Encountered 'return' statement outside of function.");
                ParseReturnStatement();
                Expect(Token.Kind.EndStatement, "Unexpected expression(s) at the end of statement");
            }
            else if (Next(Token.Kind.KeywordPrint))
            {
                ParsePrintStatement();
                Expect(Token.Kind.EndStatement, "Unexpected expression(s) at the end of statement");
            }
            else if (Next(Token.Kind.KeywordSuper))
            {
                ParseSuperStatement();
                Expect(Token.Kind.EndStatement, "Unexpected expression(s) at the end of statement");
            }
            else
            {
                ParseExpression();
                Expect(Token.Kind.EndStatement, "Unexpected expression(s) at the end of statement");
            }
        }

        /// <summary>
        /// Sets <see cref="ast"/> to an instance of a new <see cref="BreakStatement"/>
        /// </summary>
        private void ParseBreakStatement()
        {
            ast = new BreakStatement();
        }

        /// <summary>
        /// Sets <see cref="ast"/> to an instance of a new <see cref="ContinueStatement"/>
        /// </summary>
        private void ParseContinueStatement()
        {
            ast = new ContinueStatement();
        }

        /// <summary>
        /// Sets <see cref="ast"/> to an instance of a new <see cref="ReturnStatement"/> and
        /// parses potential return value expression.
        /// </summary>
        /// <exception cref="Exception">If parse error is thrown from <see cref="ParseExpression"/>.</exception>
        private void ParseReturnStatement()
        {
            IAST expr = null;

            if (!Check(Token.Kind.EndStatement))
            {
                ParseExpression();
                expr = ast;
            }

            ast = new ReturnStatement(expr);
        }

        /// <summary>
        /// Sets <see cref="ast"/> to an instance of a new <see cref="PrintStatement"/> and parses
        /// its accompanying expression.
        /// </summary>
        /// <exception cref="Exception">If call to <see cref="ParseExpression"/> throws an exception.</exception>
        private void ParsePrintStatement()
        {
            ParseExpression();
            ast = new PrintStatement(ast);
        }

        /// <summary>
        /// Parses a single expression and sets <see cref="ast"/> to its corresponding AST.
        /// </summary>
        /// <exception cref="Exception">If call to <see cref="ParsePrecedence"/> throws an exception.</exception>
        private void ParseExpression()
        {
            ParsePrecedence(Precedence.Assignment);
        }

        /// <summary>
        /// Parses an expression of a certain <see cref="Precedence"/> level.
        /// </summary>
        /// <param name="prec"><see cref="Precedence"/> level to parse at.</param>
        /// <exception cref="Exception">If calls to <see cref="GetRule"/> return a rule without a <see cref="ParseFn"/>.</exception>
        private void ParsePrecedence(Precedence prec)
        {
            Advance();
            ParseFn prefix = GetRule(Previous.kind).prefix;
            Assert(prefix != null, "Expected expression");

            prefix(this);

            while (prec <= GetRule(Current.kind).precedence)
            {
                Advance();
                ParseFn infix = GetRule(Previous.kind).infix;
                Assert(infix != null, "Remainder of expression absent.");
                infix(this);
            }
        }

        /// <summary>
        /// Parses an entire block of code given that block's indentation.
        /// </summary>
        /// <param name="indentation">Indentation of the block.</param>
        private void ParseBlock(int indentation)
        {
            Block block = new Block();
            while (Current.indentation > indentation)
            {
                try
                {
                    ParseDeclaration();
                    block.AddNode(ast);
                }
                catch (Exception e)
                {
                    HandleError(e);
                }
            }
            ast = block;
        }

        /// <summary>
        /// Parses a Variable / Constant Instantaition statement.
        /// </summary>
        /// <param name="instKind">Whether the variable is constant or not.</param>
        /// <exception cref="Exception">
        /// If call to <see cref="ParseExpression"/> throws or
        /// if 'instKind' is an invalid <see cref="InstantiationKind"/>.
        /// </exception>
        private void ParseVariableInstantiation(InstantiationKind instKind)
        {
            Expect(Token.Kind.Identifier, "Expected an identifier after '{0}' keyword.",
                instKind == InstantiationKind.Variable ? "var" : "const");
            string id = Previous.source;

            if (instKind == InstantiationKind.Variable && Check(Token.Kind.EndStatement))
            {
                ast = new VariableDeclaration(id);
                return;
            }
            else
            {
                Expect(Token.Kind.OpEqual, "Expected '=' after identifer.");
            }

            ParseExpression();
            IAST initilizer = ast;

            switch (instKind)
            {
                case InstantiationKind.Variable:
                    ast = new VariableInstantiation(id, initilizer);
                    break;

                case InstantiationKind.Constant:
                    ast = new ConstantInstantiation(id, initilizer);
                    break;

                default:
                    Error("Unknown InstantionKind: {0}", instKind);
                    break;
            }
        }

        /// <summary>
        /// Hook into <see cref="ParseGroup"/>.
        /// </summary>
        /// <param name="parser"><see cref="Parser"/> used to parse group expression.</param>
        private static void GroupParseFn(Parser parser)
        {
            parser.ParseGroup();
        }

        /// <summary>
        /// Parses a group expression / parenthesized expression.
        /// </summary>
        /// <exception cref="Exception">If ')' token is not present.</exception>
        private void ParseGroup()
        {
            ParseExpression();
            Expect(Token.Kind.DelimCloseParenthesis, "Expected ')' to terminate parenthesised expression.");
        }

        /// <summary>
        /// Hook into <see cref="ParseLiteral"/>.
        /// </summary>
        /// <param name="parser"><see cref="Parser"/> used to parse literal.</param>
        private static void LiteralParseFn(Parser parser)
        {
            parser.ParseLiteral();
        }

        /// <summary>
        /// Sets <see cref="ast"/> to an instance of a new <see cref="Literal"/>
        /// given <see cref="Previous"/>'s value.
        /// </summary>
        private void ParseLiteral()
        {
            ast = new Literal(Previous.value);
        }

        /// <summary>
        /// Hook into <see cref="ParseIdentifier"/>.
        /// </summary>
        /// <param name="parser"><see cref="Parser"/> used to parse identifier.</param>
        private static void IdentifierParseFn(Parser parser)
        {
            parser.ParseIdentifier();
        }

        /// <summary>
        /// Sets <see cref="ast"/> to an instance of a new <see cref="Identifier"/>
        /// given <see cref="Previous"/>'s source.
        /// </summary>
        private void ParseIdentifier()
        {
            ast = new Identifier(Previous.source);
        }

        /// <summary>
        /// Hook into <see cref="ParseUnary"/>.
        /// </summary>
        /// <param name="parser"><see cref="Parser"/> used to parse unary expression.</param>
        private static void UnaryParseFn(Parser parser)
        {
            parser.ParseUnary();
        }

        /// <summary>
        /// Parses a unary expression.
        /// </summary>
        /// <exception cref="Exception">If Call to <see cref="ParsePrecedence"/> throws.</exception>
        private void ParseUnary()
        {
            Token op = Previous;

            ParsePrecedence(Precedence.Unary);
            IAST expr = ast;

            switch (op.kind)
            {
                case Token.Kind.OpBang:
                    ast = new Not(expr);
                    break;
                case Token.Kind.OpDash:
                    ast = new Negation(expr);
                    break;
            }
        }

        /// <summary>
        /// Hook into <see cref="ParseBinary"/>.
        /// </summary>
        /// <param name="parser"><see cref="Parser"/> used to parse the binary expression.</param>
        private static void BinaryParseFn(Parser parser)
        {
            parser.ParseBinary();
        }

        /// <summary>
        /// Parse a binary expression.
        /// </summary>
        /// <exception cref="Exception">
        /// If call to <see cref="ParsePrecedence"/> throws or
        /// if <see cref="Previous"/> is not a binary operator token.
        /// </exception>
        private void ParseBinary()
        {
            Token op = Previous;

            IAST lhs = ast;

            Precedence nextPrec = GetRule(Previous.kind).precedence + 1;
            ParsePrecedence(nextPrec);
            IAST rhs = ast;

            switch (op.kind)
            {
                case Token.Kind.OpPlus:
                    ast = new Addition(lhs, rhs);
                    break;

                case Token.Kind.OpDash:
                    ast = new Subtraction(lhs, rhs);
                    break;

                case Token.Kind.OpStar:
                    ast = new Multiplication(lhs, rhs);
                    break;

                case Token.Kind.OpSlash:
                    ast = new Division(lhs, rhs);
                    break;

                case Token.Kind.OpDoubleEqual:
                    ast = new Equality(lhs, rhs);
                    break;

                case Token.Kind.OpBangEqual:
                    ast = new Not(new Equality(lhs, rhs));
                    break;

                case Token.Kind.OpOr:
                    ast = new Or(lhs, rhs);
                    break;

                case Token.Kind.OpAnd:
                    ast = new And(lhs, rhs);
                    break;

                case Token.Kind.OpLeftAngle:
                    ast = new LessThan(lhs, rhs);
                    break;

                case Token.Kind.OpRightAngle:
                    ast = new GreaterThan(lhs, rhs);
                    break;

                case Token.Kind.OpLeftAngleEqual:
                    ast = new Not(new GreaterThan(lhs, rhs));
                    break;

                case Token.Kind.OpRightAngleEqual:
                    ast = new Not(new LessThan(lhs, rhs));
                    break;

                case Token.Kind.OpDoubleDot:
                    ast = new RangeExpression(lhs, rhs, false);
                    break;

                case Token.Kind.OpDoubleDotEqual:
                    ast = new RangeExpression(lhs, rhs, true);
                    break;

                case Token.Kind.DelimOpenBracket:
                    Expect(Token.Kind.DelimCloseBracket, "Expected ']' to terminate subscript operation.");
                    ast = new Subscript(lhs, rhs);
                    break;

                default:
                    Error("Expected binary operator but found '{0}'.", op.source);
                    break;
            }
        }

        /// <summary>
        /// Hook into <see cref="ParseMemberReference"/>.
        /// </summary>
        /// <param name="parser"><see cref="Parser"/> used to parse the member reference expression.</param>
        private static void MemberReferenceParseFn(Parser parser)
        {
            parser.ParseMemberReference();
        }

        /// <summary>
        /// Parses a member reference.
        /// </summary>
        /// <remarks>
        /// <see cref="ast"/> may be set to a <see cref="MemberReference"/> instance or an
        /// <see cref="Invocation"/> if '(' immediately follows.
        /// </remarks>
        /// <exception cref="Exception">If there is a parse error.</exception>
        private void ParseMemberReference()
        {
            Expect(Token.Kind.Identifier, "Expected identifier after '.'.");
            string id = Previous.source;

            if (Next(Token.Kind.DelimOpenParenthesis))
            {
                var boundMethod = new BoundMethod(ast, id);

                Block args = new Block();
                if (!Check(Token.Kind.DelimCloseParenthesis))
                {
                    do
                    {
                        ParseExpression();
                        args.AddNode(ast);
                    } while (Next(Token.Kind.Comma) && !Check(Token.Kind.EOF));
                }
                Expect(Token.Kind.DelimCloseParenthesis, "Expected ')' to end method call.");

                ast = new Invocation(boundMethod, args);
            }
            else
            {
                ast = new MemberReference(ast, id);
            }
        }

        /// <summary>
        /// Hook into <see cref="ParseAssignment"/>.
        /// </summary>
        /// <param name="parser"><see cref="Parser"/> used to parse the assignment statement.</param>
        private static void AssigmentParseFn(Parser parser)
        {
            parser.ParseAssignment();
        }

        /// <summary>
        /// Parses an assignment statement.
        /// </summary>
        /// <remarks>
        /// <see cref="ast"/> will be set to a different type of assignment node
        /// depending on what is the left hand side node.
        /// </remarks>
        /// <exception cref="Exception">If there is a parse error.</exception>
        private void ParseAssignment()
        {
            IAST lhs = ast;
            if (lhs is Identifier id)
            {
                ParseAssignmentIdentifier(id);
            }
            else if (lhs is Subscript sub)
            {
                ParseAssignmentSubscript(sub);
            }
            else if (lhs is MemberReference memRef)
            {
                ParseAssignmentMemberReference(memRef);
            }
            else
            {
                Error("First operand of '=' is not assignable.");
            }   
        }

        /// <summary>
        /// Parses an assignment statement where the left hand side node is
        /// an <see cref="Identifier"/> node.
        /// </summary>
        /// <param name="id">The target of the assignment.</param>
        /// <exception cref="Exception">If call to <see cref="ParseExpression"/> throws.</exception>
        private void ParseAssignmentIdentifier(Identifier id)
        {
            ParseExpression();
            IAST assigner = ast;
            ast = new VariableAssignment(id.Id, assigner);
        }

        /// <summary>
        /// Parses an assignment statement where the left hand side node is
        /// a <see cref="Subscript"/> node.
        /// </summary>
        /// <param name="sub">The target of the assignment.</param>
        /// <exception cref="Exception">If call to <see cref="ParseExpression"/> throws.</exception>
        private void ParseAssignmentSubscript(Subscript sub)
        {
            IAST list = sub.Lhs;
            IAST subscript = sub.Rhs;

            ParseExpression();
            IAST assigner = ast;

            ast = new SubscriptAssignment(list, subscript, assigner);
        }

        /// <summary>
        /// Parses an assignment statement where the left hand side node is
        /// a <see cref="MemberReference"/> node.
        /// </summary>
        /// <param name="memRef">The target of the assignment.</param>
        /// <exception cref="Exception">If call to <see cref="ParseExpression"/> throws.</exception>
        private void ParseAssignmentMemberReference(MemberReference memRef)
        {
            IAST instance = memRef.Instance;
            string member = memRef.Member;

            ParseExpression();
            IAST assigner = ast;

            ast = new MemberReferenceAssignment(instance, member, assigner);
        }

        /// <summary>
        /// Hook into <see cref="ParseList"/>.
        /// </summary>
        /// <param name="parser"><see cref="Parser"/> used to parse the list expression.</param>
        private static void ListParseFn(Parser parser)
        {
            parser.ParseList();
        }

        /// <summary>
        /// Parses a list expression.
        /// </summary>
        /// <exception cref="Exception">If there was a parse error.</exception>
        private void ParseList()
        {
            ListExpression list = new ListExpression();
            if (!Check(Token.Kind.DelimCloseBracket))
            {
                do
                {
                    ParseExpression();
                    list.AddNode(ast);
                } while (Next(Token.Kind.Comma) && !Check(Token.Kind.EOF));
            }
            Expect(Token.Kind.DelimCloseBracket, "Expected ']' keyword to terminate List literal.");
            ast = list;
        }

        /// <summary>
        /// Hook into <see cref="ParseInvocation"/>.
        /// </summary>
        /// <param name="parser"><see cref="Parser"/> used to parse the invocation expression.</param>
        private static void InvokeParseFn(Parser parser)
        {
            parser.ParseInvocation();
        }

        /// <summary>
        /// Parses an invocation expression such as a function call etc.
        /// </summary>
        /// <exception cref="Exception">If there was a parse error.</exception>
        private void ParseInvocation()
        {
            IAST lhs = ast;

            Block args = new Block();

            if (!Next(Token.Kind.DelimCloseParenthesis))
            {
                do
                {
                    ParseExpression();
                    args.AddNode(ast);
                } while (Next(Token.Kind.Comma) && !Check(Token.Kind.EOF));
                Expect(Token.Kind.DelimCloseParenthesis, "Expected ')' after invocation.");
            }

            ast = new Invocation(lhs, args);
        }

        /// <summary>
        /// Parses a function declaration.
        /// </summary>
        /// <remarks>
        /// Is called in <see cref="ParseFunctionDeclaration"/> and
        /// in <see cref="ClassDeclaration"/> so is wrapped in its own method.
        /// </remarks>
        /// <exception cref="Exception">If there was a parse error.</exception>
        private void ParseFunctionDeclarationUnwrapped()
        {
            lambdaDepth++;

            Expect(Token.Kind.Identifier, "Expected identifier after 'fn' keyword.");
            string id = Previous.source;

            Expect(Token.Kind.DelimOpenParenthesis, "Expected '(' after identifier.");

            List<string> args = new List<string>();
            bool varargs = false;

            if (!Next(Token.Kind.DelimCloseParenthesis))
            {
                do
                {
                    if (Next(Token.Kind.Identifier))
                    {
                        args.Add(Previous.source);
                    }
                    else if (Next(Token.Kind.OpStar))
                    {
                        Expect(Token.Kind.Identifier, "Expected an identifier after '*'.");
                        varargs = true;
                        args.Add(Previous.source);
                        break; // no arguments after varargs
                    }
                    else
                    {
                        Error("Expected an identifier but found '{0}'", Current.source);
                    }
                } while (Next(Token.Kind.Comma) && !Check(Token.Kind.EOF));
                Expect(Token.Kind.DelimCloseParenthesis, "Expected ')' to terminate argument list.");
            }

            Expect(Token.Kind.EndStatement, "Body of function must be on subsequent lines.");
            ParseBlock(Previous.indentation);
            Block body = ast as Block;

            ast = new LambdaExpression(args, body, varargs, id);

            lambdaDepth--;
        }

        /// <summary>
        /// Parses a function declaration and sets <see cref="ast"/> to a new 
        /// <see cref="ConstantInstantiation"/> instance.
        /// </summary>
        /// <exception cref="Exception">If there was a parse error.</exception>
        private void ParseFunctionDeclaration()
        {
            ParseFunctionDeclarationUnwrapped();
            var lambda = ast as LambdaExpression;
            ast = new ConstantInstantiation(lambda.Id, lambda);
        }

        /// <summary>
        /// Hook into <see cref="ParseLambdaExpression"/>.
        /// </summary>
        /// <param name="parser"><see cref="Parser"/> used to parse the lambda expression.</param>
        private static void LambdaParseFn(Parser parser)
        {
            parser.ParseLambdaExpression();
        }

        /// <summary>
        /// Parses a lambda expression.
        /// </summary>
        /// <exception cref="Exception">If there was a parse error.</exception>
        private void ParseLambdaExpression()
        {
            List<string> args = new List<string>();
            bool varargs = false;
            if (!Next(Token.Kind.DelimPipe))
            {
                do
                {
                    if (Next(Token.Kind.Identifier))
                    {
                        args.Add(Previous.source);
                    }
                    else if (Next(Token.Kind.OpStar))
                    {
                        Expect(Token.Kind.Identifier, "Expected an identifier after '*'.");
                        varargs = true;
                        args.Add(Previous.source);
                        break; // no arguments after varargs
                    }
                    else
                    {
                        Error("Expected an identifier but found '{0}'", Current.source);
                    }
                } while (Next(Token.Kind.Comma) && !Check(Token.Kind.EOF));
                Expect(Token.Kind.DelimPipe, "Expected '|' to terminate argument list.");
            }

            Block body;
            if (Next(Token.Kind.EndStatement))
            {
                ParseBlock(Previous.indentation);
                body = ast as Block;
            }
            else
            {
                body = new Block();
                ParseExpression();
                body.AddNode(ast);
            }

            ast = new LambdaExpression(args, body, varargs);
        }

        /// <summary>
        /// Parses a class declaration.
        /// </summary>
        /// <exception cref="Exception">If there was a parse error.</exception>
        private void ParseClassDeclaration()
        {
            Expect(Token.Kind.Identifier, "Expected identifier after 'class' keyword.");
            string id = Previous.source;
            string superClass = null;

            if (Next(Token.Kind.DelimOpenParenthesis))
            {
                Expect(Token.Kind.Identifier, "Expected identifier after '('.");
                superClass = Previous.source;
                Expect(Token.Kind.DelimCloseParenthesis, "Expected ')'.");
            }

            Expect(Token.Kind.EndStatement, "Body of class must be on subsequent lines.");

            List<LambdaExpression> methods = new List<LambdaExpression>();
            List<LambdaExpression> classMethods = new List<LambdaExpression>();

            if (Current.indentation > Previous.indentation)
            {
                while (Next(Token.Kind.KeywordFn))
                {
                    bool classMethod = Next(Token.Kind.KeywordClass);
                    if (classMethod)
                    {
                        Expect(Token.Kind.OpDot, "Expected '.' after 'class' keyword.");
                    }

                    ParseFunctionDeclarationUnwrapped();

                    if (classMethod)
                    {
                        classMethods.Add(ast as LambdaExpression);
                    }
                    else
                    {
                        methods.Add(ast as LambdaExpression);
                    }
                }
            }

            ast = new ClassDeclaration(id, superClass, methods, classMethods);
        }

        /// <summary>
        /// Parses a super statement.
        /// </summary>
        /// <exception cref="Exception">If there was a parse error.</exception>
        private void ParseSuperStatement()
        {
            SuperStatement super = new SuperStatement();

            Expect(Token.Kind.DelimOpenParenthesis, "Expected '(' after 'super' keyword.");

            if (!Check(Token.Kind.DelimCloseParenthesis))
            {
                do
                {
                    ParseExpression();
                    super.AddNode(ast);
                } while (Next(Token.Kind.Comma) && !Check(Token.Kind.EOF));
            }

            Expect(Token.Kind.DelimCloseParenthesis, "Expected ')' to terminate 'super' statement.");

            ast = super;
        }

        /// <summary>
        /// Parses an if statement.
        /// </summary>
        /// <exception cref="Exception">If there was a parse error.</exception>
        private void ParseIfStatement()
        {
            ParseExpression();
            IAST cond = ast;

            Expect(Token.Kind.EndStatement, "Body of 'if' statement must be on subsequent lines.");

            int indent = Previous.indentation;
            ParseBlock(indent);
            Block then = ast as Block;

            IAST @else = null;
            if (Next(Token.Kind.KeywordElif))
            {
                ParseIfStatement();
                @else = ast;
            }
            else if (Next(Token.Kind.KeywordElse))
            {
                Expect(Token.Kind.EndStatement, "Body of 'else' block must be on subsequent lines.");
                ParseBlock(indent);
                @else = ast;
            }

            ast = new IfStatement(cond, then, @else);
        }

        /// <summary>
        /// Parses a while-loop statement.
        /// </summary>
        /// <exception cref="Exception">If there was a parse error.</exception>
        private void ParseWhileStatement()
        {
            loopDepth++;

            ParseExpression();
            IAST cond = ast;

            Expect(Token.Kind.EndStatement, "Body of 'while' statement must be on subsequent lines.");
            ParseBlock(Previous.indentation);
            Block body = ast as Block;

            ast = new WhileStatement(cond, body);

            loopDepth--;
        }

        /// <summary>
        /// Parses a for-loop statement.
        /// </summary>
        /// <exception cref="Exception">If there was a parse error.</exception>
        private void ParseForStatement()
        {
            loopDepth++;

            Expect(Token.Kind.Identifier, "Expected identifier after 'for' keyword.");
            string iter = Previous.source;

            string counter = null;
            if (Next(Token.Kind.Comma))
            {
                Expect(Token.Kind.Identifier, "Expected identifier of counter variable after ','.");
                counter = Previous.source;
            }

            Expect(Token.Kind.KeywordIn, "Expected 'in' keyword not found.");

            ParseExpression();
            IAST iterable = ast;

            Expect(Token.Kind.EndStatement, "Body of for loop must be on subsequent lines.");
            ParseBlock(Previous.indentation);
            Block body = ast as Block;

            ast = new ForStatement(iter, counter, iterable, body);

            loopDepth--;
        }
    }
}

// Value.cs
using System;
using System.Collections.Generic;
using System.Text;

namespace CustomProject
{
    /// <summary>
    /// Represents any possible runtime value that can exist in the language.
    /// </summary>
    public abstract class Value
    {
        /// <summary>
        /// Enumerates every possible type of value in the language.
        /// </summary>
        public enum ValueType
        {
            Unknown = 0,
            
            Nil,
            Boolean,
            Number,
            String,
            Char,
            Lambda,
            List,
            Class,
            Instance,
            Range,
            Native,
        }

        /// <summary>
        /// The <see cref="Value"/>'s <see cref="ValueType"/>.
        /// </summary>
        public ValueType Type { get; protected set; }

        public Value(ValueType type)
        {
            Type = type;
        }

        /// <summary>
        /// Asserts that the given <see cref="Value"/> is the expected <see cref="ValueType"/>.
        /// </summary>
        /// <param name="type">Expected <see cref="ValueType"/>.</param>
        /// <param name="value"><see cref="Value"/> being tested.</param>
        /// <param name="format">Format for the error message.</param>
        /// <param name="args">Arguments to be supplied to the format.</param>
        /// <exception cref="Exception">If types don't match.</exception>
        public static void AssertType(ValueType type, Value value, string format = null, params object[] args)
        {
            if (value.Type != type)
            {
                string message;
                if (format == null)
                {
                    message = string.Format("Expected '{0}' but was given '{1}'", type, value.Type);
                }
                else
                {
                    message = string.Format(format, args);
                }
                throw new Exception(message);
            }
        }

        /// <summary>
        /// Checks that the two given <see cref="Value"/>s are the same <see cref="ValueType"/>.
        /// </summary>
        /// <param name="a">First <see cref="Value"/>.</param>
        /// <param name="b">Second <see cref="Value"/>.</param>
        /// <returns>True if types match. False if not.</returns>
        public static bool TypesMatch(Value a, Value b)
        {
            return a.Type == b.Type;
        }

        /// <summary>
        /// Used to check for equality between two <see cref="Value"/>s.
        /// Does not check that 'other' is the same type as 'this'
        /// </summary>
        /// <param name="other"><see cref="Value"/> to compare against.</param>
        /// <returns>True if equal. False if not.</returns>
        public abstract bool UncheckedEqual(Value other);

        /// <summary>
        /// Checks for equality between two <see cref="Value"/>s.
        /// </summary>
        /// <param name="other"><see cref="Value"/> to compare against.</param>
        /// <returns>True if equal. False if not.</returns>
        public virtual bool Equal(Value other)
        {
            if (!TypesMatch(this, other))
            {
                return false;
            }
            return UncheckedEqual(other);
        }

        /// <summary>
        /// Check if <see cref="Value"/> is a <see cref="NilValue"/>.
        /// </summary>
        /// <returns>True if nil. False if not.</returns>
        public virtual bool IsNil()
        {
            return false;
        }

        /// <summary>
        /// Gets underlying boolean value.
        /// </summary>
        /// <exception cref="Exception">If not a <see cref="BooleanValue"/>.</exception>
        public virtual bool Boolean
        {
            get => throw new Exception("Value is not a boolean.");
        }

        /// <summary>
        /// Gets underlying float value.
        /// </summary>
        /// <exception cref="Exception">If not a <see cref="NumberValue"/>.</exception>
        public virtual float Number
        {
            get => throw new Exception("Value is not a number.");
        }

        /// <summary>
        /// Gets underlying string value.
        /// </summary>
        /// <exception cref="Exception">If not a <see cref="StringValue"/>.</exception>
        public virtual string String
        {
            get => throw new Exception("Value is not a string.");
        }

        /// <summary>
        /// Gets underlying char value.
        /// </summary>
        /// <exception cref="Exception">If not a <see cref="CharValue"/>.</exception>
        public virtual char Char
        {
            get => throw new Exception("Value is not a char.");
        }

        /// <summary>
        /// Gets underlying <see cref="LambdaExpression"/>.
        /// </summary>
        /// <exception cref="Exception">If not a <see cref="LambdaValue"/>.</exception>
        public virtual LambdaExpression Lambda
        {
            get => throw new Exception("Value is not a lambda.");
        }

        /// <summary>
        /// Gets underlying List.
        /// </summary>
        /// <exception cref="Exception">If not a <see cref="ListValue"/>.</exception>
        public virtual List<Value> List
        {
            get => throw new Exception("Value is not a list.");
        }

        /// <summary>
        /// Gets underlying <see cref="ClassValue"/>.
        /// </summary>
        /// <exception cref="Exception">If not a <see cref="ClassValue"/>.</exception>
        public virtual ClassValue Class
        {
            get => throw new Exception("Value is not a class.");
        }

        /// <summary>
        /// Gets underlying <see cref="InstanceValue"/>.
        /// </summary>
        /// <exception cref="Exception">If not a <see cref="InstanceValue"/>.</exception>
        public virtual InstanceValue Instance
        {
            get => throw new Exception("Value is not an instance.");
        }

        /// <summary>
        /// Gets underlying <see cref="RangeValue"/>.
        /// </summary>
        /// <exception cref="Exception">If not a <see cref="RangeValue"/>.</exception>
        public virtual RangeValue Range
        {
            get => throw new Exception("Value is not a class.");
        }
    }

    /// <summary>
    /// Represents 'nil' in the language.
    /// </summary>
    public class NilValue : Value
    {
        public NilValue()
            : base(ValueType.Nil)
        {
        }

        public override bool UncheckedEqual(Value other)
        {
            return other.IsNil();
        }

        public override bool IsNil()
        {
            return true;
        }

        public override string ToString()
        {
            return "nil";
        }
    }

    /// <summary>
    /// Represents any boolean value in the langauge.
    /// </summary>
    public class BooleanValue : Value
    {
        private bool value;

        /// <summary>
        /// Gets underlying boolean value.
        /// </summary>
        public override bool Boolean { get => value; }

        public BooleanValue(bool value)
            : base(ValueType.Boolean)
        {
            this.value = value;
        }

        public override bool UncheckedEqual(Value other)
        {
            return value == other.Boolean;
        }

        public override string ToString()
        {
            return value.ToString().ToLower();
        }
    }

    /// <summary>
    /// Represents a number value in the language.
    /// </summary>
    public class NumberValue : Value
    {
        private float value;

        /// <summary>
        /// Gets underlying float value.
        /// </summary>
        public override float Number { get => value; }

        public NumberValue(float value)
            : base(ValueType.Number)
        {
            this.value = value;
        }

        public override bool UncheckedEqual(Value other)
        {
            return value == other.Number;
        }

        public override string ToString()
        {
            return value.ToString();
        }
    }

    /// <summary>
    /// Represents a string value in the langauge.
    /// </summary>
    public class StringValue : Value
    {
        private string value;

        /// <summary>
        /// Gets underlying string value.
        /// </summary>
        public override string String { get => value; }

        public StringValue(string value)
            : base(ValueType.String)
        {
            this.value = value;
        }

        /// <summary>
        /// Replaces underlying string value.
        /// </summary>
        /// <param name="newValue">The replacement string.</param>
        public void ReplaceString(string newValue)
        {
            value = newValue;
        }

        public override bool UncheckedEqual(Value other)
        {
            return value == other.String;
        }

        public override string ToString()
        {
            return value.ToString();
        }
    }

    /// <summary>
    /// Represents a char value in the language.
    /// </summary>
    public class CharValue : Value
    {
        private char value;

        /// <summary>
        /// Gets underlying char value.
        /// </summary>
        public override char Char { get => value; }

        public CharValue(char value)
            : base(ValueType.Char)
        {
            this.value = value;
        }

        public override bool UncheckedEqual(Value other)
        {
            return value == other.Char;
        }

        public override string ToString()
        {
            return value.ToString();
        }
    }

    /// <summary>
    /// Represents a lambda value in the language.
    /// </summary>
    public class LambdaValue : Value
    {
        private LambdaExpression value;

        /// <summary>
        /// Get underlying <see cref="LambdaExpression"/>.
        /// </summary>
        public override LambdaExpression Lambda { get => value; }

        public LambdaValue(LambdaExpression value)
            : base(ValueType.Lambda)
        {
            this.value = value;
        }

        public override bool UncheckedEqual(Value other)
        {
            return value == other.Lambda;
        }

        public override string ToString()
        {
            StringBuilder builder = new StringBuilder("fn(");
            for (int i = 0; i < value.Args.Count; i++)
            {
                bool vararg = i == value.Args.Count - 1 && value.IsVarargs();
                builder.AppendFormat("{0}{1}", vararg ? "*" : "", value.Args[i]);
                if (i + 1 < value.Args.Count)
                {
                    builder.Append(", ");
                }
            }
            builder.Append(")");
            return builder.ToString();
        }
    }

    /// <summary>
    /// Represents a list value in the language.
    /// </summary>
    public class ListValue : Value
    {
        private List<Value> value;

        /// <summary>
        /// Gets underlying List.
        /// </summary>
        public override List<Value> List { get => value; }

        public ListValue(List<Value> value)
            : base(ValueType.List)
        {
            this.value = value;
        }

        public override bool UncheckedEqual(Value other)
        {
            List<Value> otherList = other.List;
            if (value.Count != otherList.Count)
            {
                return false;
            }
            for (int i = 0; i < value.Count; i++)
            {
                if (!value[i].Equal(otherList[i]))
                {
                    return false;
                }
            }
            return true;
        }

        public override string ToString()
        {
            StringBuilder builder = new StringBuilder("[");
            for (int i = 0; i < value.Count; i++)
            {
                builder.Append(value[i].ToString());
                if (i + 1 < value.Count)
                {
                    builder.Append(", ");
                }
            }
            builder.Append("]");
            return builder.ToString();
        }
    }

    /// <summary>
    /// Represents a class in the language.
    /// </summary>
    public class ClassValue : Value
    {
        public string Name { get; private set; }
        public Dictionary<string, LambdaValue> Methods { get; private set; }
        public Dictionary<string, LambdaValue> ClassMethods { get; private set; }
        public ClassValue SuperClass { get; private set; }

        /// <summary>
        /// Gets underlying <see cref="ClassValue"/>.
        /// </summary>
        /// <remarks>
        /// Used to down cast a <see cref="Value"/> into a <see cref="ClassValue"/>.
        /// </remarks>
        public override ClassValue Class { get => this; }

        public ClassValue(string name, ClassValue superClass)
            : base(ValueType.Class)
        {
            Name = name;
            Methods = new Dictionary<string, LambdaValue>();
            ClassMethods = new Dictionary<string, LambdaValue>();
            SuperClass = superClass;
        }

        public override bool UncheckedEqual(Value other)
        {
            return this == other;
        }

        public override string ToString()
        {
            StringBuilder builder = new StringBuilder();
            builder.AppendFormat("{0}", Name);

            if (SuperClass != null)
            {
                builder.AppendFormat("({0})", SuperClass.Name);
            }

            builder.Append(" {\n");

            foreach (var binding in Methods)
            {
                builder.AppendFormat("  {0}: {1}\n", binding.Key, binding.Value);
            }

            foreach (var binding in ClassMethods)
            {
                builder.AppendFormat("  class.{0}: {1}\n", binding.Key, binding.Value);
            }

            builder.Append("}");

            return builder.ToString();
        }
    }

    /// <summary>
    /// Represents an instance of a class in the language.
    /// </summary>
    public class InstanceValue : Value
    {
        private ClassValue @class;
        public Dictionary<string, Value> Fields { get; private set; }

        /// <summary>
        /// Gets the instance's <see cref="ClassValue"/>.
        /// </summary>
        public override ClassValue Class { get => @class; }

        /// <summary>
        /// Gets the underlying <see cref="InstanceValue"/>.
        /// </summary>
        /// <remarks>
        /// Used to down cast a <see cref="Value"/> to an <see cref="InstanceValue"/>.
        /// </remarks>
        public override InstanceValue Instance { get => this; }

        public InstanceValue(ClassValue @class)
            : base(ValueType.Instance)
        {
            this.@class = @class;
            Fields = new Dictionary<string, Value>();
        }

        /// <summary>
        /// Casts the instance to its super class and returns its actual class.
        /// </summary>
        /// <returns>The original class of the instance.</returns>
        public ClassValue UpCast()
        {
            var classValue = @class;
            @class = @class.SuperClass;
            return classValue;
        }

        /// <summary>
        /// Casts an instance to a given class.
        /// </summary>
        /// <param name="classValue">Class to cast the instance to.</param>
        public void Cast(ClassValue classValue)
        {
            @class = classValue;
        }

        public override bool UncheckedEqual(Value other)
        {
            if (!@class.UncheckedEqual(other.Class))
            {
                return false;
            }

            var myFields = Fields.Values.GetEnumerator();
            var theirFields = other.Instance.Fields.Values.GetEnumerator();

            while (myFields.MoveNext() && theirFields.MoveNext())
            {
                var mine = myFields.Current;
                var theirs = theirFields.Current;

                if (!mine.Equal(theirs))
                {
                    return false;
                }
            }

            return true;
        }

        public override string ToString()
        {
            StringBuilder builder = new StringBuilder();
            builder.AppendFormat("{0}(", Class.Name);

            var fields = Fields.GetEnumerator();
            for (int i = 0; i < Fields.Count; i++)
            {
                fields.MoveNext();
                builder.AppendFormat("{0}: {1}", fields.Current.Key, fields.Current.Value);
                if (i + 1 < Fields.Count)
                {
                    builder.Append(", ");
                }
            }

            builder.Append(")");
            return builder.ToString();
        }
    }

    /// <summary>
    /// Represents a range value in the language.
    /// </summary>
    /// <remarks>
    /// Ranges are mainly used in for-loops.
    /// </remarks>
    public class RangeValue : Value
    {
        public Value Start { get; private set; }
        public Value End { get; private set; }
        public bool Inclusive { get; private set; }

        /// <summary>
        /// Gets underlying <see cref="RangeValue"/>.
        /// </summary>
        public override RangeValue Range { get => this; }

        public RangeValue(Value start, Value end, bool inclusive)
            : base(ValueType.Range)
        {
            Start = start;
            End = end;
            Inclusive = inclusive;
        }

        public override bool UncheckedEqual(Value other)
        {
            return Start.Equal(other.Range.Start)
                && End.Equal(other.Range.End)
                && Inclusive == other.Range.Inclusive;
        }

        public override string ToString()
        {
            string op = Inclusive ? "..=" : "..";
            return string.Format("{0}{1}{2}", Start, op, End);
        }
    }
}

// AST.cs
using System;
using System.Collections.Generic;

namespace CustomProject
{
    /// <summary>
    /// Interface for every node in the Abstract Syntax Tree.
    /// </summary>
    public interface IAST
    {
        /// <summary>
        /// Executes the node and its children and returns the result.
        /// </summary>
        /// <param name="vm">
        /// If the node needs to access any variables or constants; or
        /// add some. It can do so via this parameter.
        /// </param>
        /// <returns>The resulting <see cref="Value"/> after executing the node.</returns>
        Value Execute(VM vm);
    }

    /// <summary>
    /// Represents a literal value in the AST.
    /// </summary>
    public class Literal : IAST
    {
        Value value;

        public Literal(Value value)
        {
            this.value = value;
        }

        /// <summary>
        /// Gets the literal value.
        /// </summary>
        /// <param name="vm">Not needed other than to conform to the <see cref="IAST"/> interface.</param>
        /// <returns>The literal value.</returns>
        public Value Execute(VM vm)
        {
            return value;
        }
    }

    /// <summary>
    /// Represents a block of code in the AST.
    /// </summary>
    public class Block : IAST
    {
        /// <summary>
        /// List of all AST nodes that are apart of the block.
        /// </summary>
        public List<IAST> Nodes { get; private set; }

        public Block()
        {
            Nodes = new List<IAST>();
        }

        /// <summary>
        /// Adds an AST node to <see cref="Nodes"/>.
        /// </summary>
        /// <param name="node">Node to add.</param>
        public void AddNode(IAST node)
        {
            Nodes.Add(node);
        }

        /// <summary>
        /// Executes all nodes in the block under a new scope and returns the result
        /// of the last node in the block.
        /// </summary>
        /// <param name="vm">The parent scope of this block.</param>
        /// <returns>Result of the last node of the block.</returns>
        public virtual Value Execute(VM vm)
        {
            VM scope = new VM(vm, vm.Global);
            Value ret = new NilValue();
            foreach (IAST expr in Nodes)
            {
                ret = expr.Execute(scope);
            }
            return ret;
        }
    }

    /// <summary>
    /// Represents a list expression in the AST.
    /// </summary>
    public class ListExpression : Block
    {
        /// <summary>
        /// Executes all element expressions of the list expression and returns them
        /// as a <see cref="ListValue"/>.
        /// </summary>
        /// <param name="vm">Environment to execute the list's element nodes in.</param>
        /// <returns>The resulting <see cref="ListValue"/>.</returns>
        public override Value Execute(VM vm)
        {
            List<Value> values = new List<Value>();
            foreach (IAST element in Nodes)
            {
                values.Add(element.Execute(vm));
            }
            return new ListValue(values);
        }
    }

    /// <summary>
    /// Represents a super statement in the AST.
    /// </summary>
    /// <remarks>A super statement is used to call a superclass's 'init()' method.</remarks>
    public class SuperStatement : Block
    {
        /// <summary>
        /// Calls the init method of an instance's superclass.
        /// </summary>
        /// <remarks>
        /// Expects a constant called 'self' to be present in the VM.
        /// Expects it to have a superclass with an 'init()' method.
        /// </remarks>
        /// <param name="vm">Used to access global scope.</param>
        /// <returns>A <see cref="NilValue"/>.</returns>
        /// <exception cref="Exception">If an expectation is not met or another runtime error occurs.</exception>
        public override Value Execute(VM vm)
        {
            if (!vm.Parent.Constants.ContainsKey("self") ||
                !(vm.Parent.Constants["self"] is InstanceValue))
            {
                throw new Exception("Can only call 'super()' inside a class.");
            }

            InstanceValue self = vm.Parent.Constants["self"].Instance;
            ClassValue selfClass = self.UpCast();

            if (!selfClass.Methods.ContainsKey("<SUPER>"))
            {
                throw new Exception(string.Format("Cannot call 'super()'. '{0}' is not a subclass.", selfClass.Name));
            }

            LambdaExpression super = selfClass.Methods["<SUPER>"].Lambda;

            if (Nodes.Count != super.Args.Count)
            {
                throw new Exception(string.Format(
                    "Argument Mistmatch! {0}.super takes {1} argument(s) but was given {2}.",
                    selfClass.Name, super.Args.Count, Nodes.Count));
            }

            VM @new = new VM(null, vm.Global);
            @new.Constants.Add("self", self);

            for (int i = 0; i < super.Args.Count; i++)
            {
                string argId = super.Args[i];
                Value arg = Nodes[i].Execute(vm);
                @new.Variables.Add(argId, arg);
            }

            try
            {
                super.Body.Execute(@new);
            }
            catch (ReturnStatement.Signal sig)
            {
                if (!sig.Value.IsNil())
                {
                    throw new Exception("Cannot return a value from class initializer.");
                }
            }

            self.Cast(selfClass);

            return new NilValue();
        }
    }

    /// <summary>
    /// Represents an identifier in the AST.
    /// </summary>
    public class Identifier : IAST
    {
        /// <summary>
        /// string representation of the identifier.
        /// </summary>
        public string Id { get; private set; }

        public Identifier(string id)
        {
            Id = id;
        }

        /// <summary>
        /// Gets the <see cref="Value"/> currently associated with the identifier.
        /// </summary>
        /// <param name="vm">Where the <see cref="Value"/> is expected to be found.</param>
        /// <returns>The <see cref="Value"/> that corresponds with the identifier.</returns>
        /// <exception cref="Exception">If identifier cannot be resolved.</exception>
        public Value Execute(VM vm)
        {
            if (vm.Variables.ContainsKey(Id))
            {
                return vm.Variables[Id];
            }
            else if (vm.Constants.ContainsKey(Id))
            {
                return vm.Constants[Id];
            }
            else if (vm.Parent != null)
            {
                return Execute(vm.Parent);
            }
            else if (vm.Global.Variables.ContainsKey(Id))
            {
                return vm.Global.Variables[Id];
            }
            else if (vm.Global.Constants.ContainsKey(Id))
            {
                return vm.Global.Constants[Id];
            }
            else
            {
                throw new Exception(string.Format("Unresolved identifier '{0}'.", Id));
            }
        }
    }

    /// <summary>
    /// Represents a variable assignment in the AST.
    /// </summary>
    public class VariableAssignment : IAST
    {
        protected string id;
        protected IAST assigner;

        public VariableAssignment(string id, IAST assigner)
        {
            this.id = id;
            this.assigner = assigner;
        }

        /// <summary>
        /// Assigns new <see cref="Value"/> to variable that corresponds with
        /// <see cref="id"/> by executing <see cref="assigner"/>.
        /// </summary>
        /// <param name="vm"></param>
        /// <returns>A <see cref="NilValue"/>.</returns>
        /// <exception cref="Exception">
        /// If <see cref="id"/> corresponds with a constant
        /// or couldn't be resolved at all.
        /// </exception>
        public virtual Value Execute(VM vm)
        {
            return DoExecute(vm, vm);
        }

        /// <summary>
        /// Looks for the variable that corresponds with <see cref="id"/> and
        /// changes its value to the result of executing <see cref="assigner"/>.
        /// </summary>
        /// <param name="vm">The environment to execute <see cref="assigner"/> in.</param>
        /// <param name="lookup">The environment that <see cref="id"/> is expected to exist in.</param>
        /// <returns>A <see cref="NilValue"/>.</returns>
        /// <exception cref="Exception">
        /// If <see cref="id"/> corresponds with a constant or cannot be resolved at all.
        /// </exception>
        private Value DoExecute(VM vm, VM lookup)
        {
            if (lookup.Variables.ContainsKey(id))
            {
                lookup.Variables[id] = assigner.Execute(vm);
            }
            else if (lookup.Parent != null)
            {
                return DoExecute(vm, lookup.Parent);
            }
            else if (vm.Global.Variables.ContainsKey(id))
            {
                vm.Global.Variables[id] = assigner.Execute(vm);
            }
            else
            {
                string errMessage;
                if (vm.Constants.ContainsKey(id))
                {
                    errMessage = string.Format("Attempt to assign to constant '{0}'.", id);
                }
                else
                {
                    errMessage = string.Format("Unresolved identifier '{0}'.", id);
                }
                throw new Exception(errMessage);
            }
            return new NilValue();
        }
    }

    /// <summary>
    /// Represents variable assignment through a subscript operation in the AST.
    /// </summary>
    public class SubscriptAssignment : IAST
    {
        IAST list;
        IAST subscript;
        IAST assigner;

        public SubscriptAssignment(IAST list, IAST subscript, IAST assigner)
        {
            this.list = list;
            this.subscript = subscript;
            this.assigner = assigner;
        }

        /// <summary>
        /// Assign new value to the element at the index returned by executing <see cref="subscript"/>
        /// in the list returned by executing <see cref="list"/>. The new value is returned by
        /// executing <see cref="assigner"/>.
        /// </summary>
        /// <param name="vm">The environment in which the child nodes are executed.</param>
        /// <returns>A <see cref="NilValue"/>.</returns>
        /// <exception cref="Exception">If <see cref="list"/> is not a <see cref="ListValue"/> or
        /// if <see cref="subscript"/> is not a <see cref="NumberValue"/>.</exception>
        public Value Execute(VM vm)
        {
            Value listVal = list.Execute(vm);
            Value idx = subscript.Execute(vm);

            Value.AssertType(Value.ValueType.List, listVal,
                "First operand of '[]' expected to be 'List' but was given '{0}'.", listVal.Type);
            Value.AssertType(Value.ValueType.Number, idx,
                "Second operand of '[]' expected to be 'Number' but was given '{0}'.", idx.Type);

            List<Value> values = listVal.List;
            float i = idx.Number;

            values[(int)i] = assigner.Execute(vm);

            return new NilValue();
        }
    }

    /// <summary>
    /// Represents variable assignment through a member reference in the AST.
    /// </summary>
    public class MemberReferenceAssignment : IAST
    {
        IAST instance;
        string member;
        IAST assigner;

        public MemberReferenceAssignment(IAST instance, string member, IAST assigner)
        {
            this.instance = instance;
            this.member = member;
            this.assigner = assigner;
        }

        /// <summary>
        /// Assigns new value to a field of an instance.
        /// The new value is gotten by executing <see cref="assigner"/>.
        /// The instance is gotten by executing <see cref="instance"/>.
        /// <see cref="member"/> is used to access the correct field of the instance.
        /// </summary>
        /// <param name="vm">Environment to execute <see cref="instance"/> and <see cref="assigner"/> in.</param>
        /// <returns>A <see cref="NilValue"/>.</returns>
        /// <exception cref="Exception">If <see cref="instance"/> doesn't return a <see cref="InstanceValue"/>.</exception>
        public Value Execute(VM vm)
        {
            Value instValue = instance.Execute(vm);
            Value.AssertType(Value.ValueType.Instance, instValue,
                "First operand of '.' expected to be an instance of a class but was given '{0}'.", instValue.Type);
            InstanceValue inst = instValue.Instance;
            inst.Fields[member] = assigner.Execute(vm);
            return new NilValue();
        }
    }

    /// <summary>
    /// Represents a bound method in the AST.
    /// </summary>
    /// <remarks>
    /// A bound method is used for calling methods with a dot-call syntax like: <c>x.f()</c>
    /// </remarks>
    public class BoundMethod : IAST
    {
        /// <summary>
        /// The instance that will be passed to the method.
        /// </summary>
        public IAST Receiver { get; private set; }

        /// <summary>
        /// Name of the method to call.
        /// </summary>
        public string Method { get; private set; }

        public BoundMethod(IAST receiver, string method)
        {
            Receiver = receiver;
            Method = method;
        }

        /// <summary>
        /// Should not be called! Always throws an exception when called.
        /// </summary>
        /// <exception cref="Exception">Shouldn't be called.</exception>
        public Value Execute(VM vm)
        {
            throw new Exception("Internal: Should not execute a BoundMethod directly.");
        }
    }

    /// <summary>
    /// Represents an if statement in the AST.
    /// </summary>
    public class IfStatement : IAST
    {
        IAST cond;
        Block then;
        IAST @else;

        public IfStatement(IAST cond, Block then, IAST @else)
        {
            this.cond = cond;
            this.then = then;
            this.@else = @else;
        }

        /// <summary>
        /// Executes <see cref="then"/> if <see cref="cond"/> returns a true <see cref="BooleanValue"/>.
        /// </summary>
        /// <param name="vm">The environment in which to execute <see cref="cond"/> and <see cref="then"/>.</param>
        /// <returns>True if <see cref="then"/> was executed. False if not.</returns>
        /// <exception cref="Exception">If <see cref="cond"/> didn't return a <see cref="BooleanValue"/>.</exception>
        private bool ExecuteThenBlock(VM vm)
        {
            Value condValue = cond.Execute(vm);
            Value.AssertType(Value.ValueType.Boolean, condValue,
                "Condition of 'if' statement must be 'Boolean' but was given '{0}'.", condValue.Type);
            if (condValue.Boolean)
            {
                then.Execute(vm);
                return true;
            }
            return false;
        }

        /// <summary>
        /// Executes <see cref="then"/> if <see cref="cond"/> returns a true <see cref="BooleanValue"/>.
        /// If not, <see cref="@else"/> is executed if it isn't null.
        /// </summary>
        /// <param name="vm">The environment is which to execute the if statement.</param>
        /// <returns>A <see cref="NilValue"/>.</returns>
        /// <exception cref="Exception">If <see cref="cond"/> doesn't return a <see cref="BooleanValue"/>.</exception>
        public Value Execute(VM vm)
        {
            if (!ExecuteThenBlock(vm) && @else != null)
            {
                @else.Execute(vm);
            }
            
            return new NilValue();
        }
    }

    /// <summary>
    /// Represents a while loop in the AST.
    /// </summary>
    public class WhileStatement : IAST
    {
        IAST cond;
        Block body;

        public WhileStatement(IAST cond, Block body)
        {
            this.cond = cond;
            this.body = body;
        }

        /// <summary>
        /// Keeps executing <see cref="body"/> while <see cref="cond"/> returns a true <see cref="BooleanValue"/>.
        /// </summary>
        /// <param name="vm">The environment in which to execute the while loop.</param>
        /// <returns>A <see cref="NilValue"/>.</returns>
        /// <exception cref="Exception">If cond doesn't return a <see cref="BooleanValue"/>.</exception>
        public Value Execute(VM vm)
        {
            while (true)
            {
                Value condValue = cond.Execute(vm);
                Value.AssertType(Value.ValueType.Boolean, condValue,
                    "Condition of 'while' statement must be 'Boolean' but was given '{0}'.", condValue.Type);

                if (!condValue.Boolean)
                    break;

                try
                {
                    body.Execute(vm);
                }
                catch (ContinueStatement.Signal)
                {
                    continue;
                }
                catch (BreakStatement.Signal)
                {
                    break;
                }
            }
            return new NilValue();
        }
    }

    /// <summary>
    /// Represents a for loop in the AST.
    /// </summary>
    public class ForStatement : IAST
    {
        string iter;
        string counter;
        IAST iterable;
        Block body;

        public ForStatement(string iter, string counter, IAST iterable, Block body)
        {
            this.iter = iter;
            this.counter = counter;
            this.iterable = iterable;
            this.body = body;
        }

        /// <summary>
        /// Executes <see cref="body"/> for each item in <see cref="iterable"/>.
        /// </summary>
        /// <param name="vm">The environment in which to execute the for loop.</param>
        /// <returns>A <see cref="NilValue"/>.</returns>
        /// <exception cref="Exception">
        /// If <see cref="iterable"/> returns something that
        /// can't be iterated over.
        /// </exception>
        public Value Execute(VM vm)
        {
            Value iterableValue = iterable.Execute(vm);
            switch (iterableValue.Type)
            {
                case Value.ValueType.List:
                    IterateOverList(vm, iterableValue.List);
                    break;

                case Value.ValueType.String:
                    IterateOverString(vm, iterableValue as StringValue);
                    break;

                case Value.ValueType.Range:
                    IterateOverRange(vm, iterableValue.Range);
                    break;

                default:
                    throw new Exception(string.Format("Cannot iterate over something of type '{0}'.", iterableValue.Type));
            }
            return new NilValue();
        }

        /// <summary>
        /// Iterates over a list of values.
        /// </summary>
        /// <param name="vm">The environment in which to execute the for loop's body.</param>
        /// <param name="list">List to iterate over.</param>
        private void IterateOverList(VM vm, List<Value> list)
        {
            for (int count = 0; count < list.Count; count++)
            {
                Value it = list[count];

                VM @new = new VM(vm, vm.Global);
                if (DoExecution(@new, it, count))
                {
                    break;
                }

                list[count] = @new.Variables[iter];
            }
        }

        /// <summary>
        /// Iterates over a string of characters.
        /// </summary>
        /// <param name="vm">The environment in which to execute the for loop's body.</param>
        /// <param name="str"><see cref="StringValue"/> to iterate over.</param>
        private void IterateOverString(VM vm, StringValue str)
        {
            char[] chars = str.String.ToCharArray();
            for (int count = 0; count < chars.Length; count++)
            {
                Value it = new CharValue(chars[count]);

                VM @new = new VM(vm, vm.Global);
                if (DoExecution(@new, it, count))
                {
                    break;
                }

                chars[count] = @new.Variables[iter].Char;
            }
            str.ReplaceString(new string(chars));
        }

        /// <summary>
        /// Iterates over a range of values.
        /// </summary>
        /// <param name="vm">The environment in which to execute the for loop's body.</param>
        /// <param name="range">The range to iterate over.</param>
        private void IterateOverRange(VM vm, RangeValue range)
        {
            Value it = range.Start;
            Value end = range.End;
            int count = 0;

            while (true)
            {
                switch (it.Type)
                {
                    case Value.ValueType.Number:
                        {
                            bool @break;
                            if (range.Inclusive)
                            {
                                @break = it.Number > end.Number;
                            }
                            else
                            {
                                @break = it.Number >= end.Number;
                            }

                            if (@break) return;
                            break;
                        }

                    case Value.ValueType.Char:
                        {
                            bool @break;
                            if (range.Inclusive)
                            {
                                @break = it.Char > end.Char;
                            }
                            else
                            {
                                @break = it.Char >= end.Char;
                            }

                            if (@break) return;
                            break;
                        }

                    default:
                        throw new Exception(string.Format("Internal Error: Range<{0}>.", it.Type));
                }

                VM @new = new VM(vm, vm.Global);
                if (DoExecution(@new, it, count))
                {
                    break;
                }

                count++;

                switch (it.Type)
                {
                    case Value.ValueType.Number:
                        it = new NumberValue(it.Number + 1);
                        break;

                    case Value.ValueType.Char:
                        it = new CharValue((char)(it.Char + 1));
                        break;

                    default:
                        throw new Exception(string.Format("Internal Error: Range<{0}>.", it.Type));
                }
            }
        }

        /// <summary>
        /// Executes a single iteration of the for loop.
        /// </summary>
        /// <param name="vm">The environment in which to execute the iteration.</param>
        /// <param name="it"><see cref="Value"/> of this iteration.</param>
        /// <param name="count">number of iterations already executed.</param>
        /// <returns>True if need to break loop. False if otherwise.</returns>
        private bool DoExecution(VM vm, Value it, int count)
        {
            vm.Variables.Add(iter, it);
            if (counter != null) vm.Variables.Add(counter, new NumberValue(count));

            try
            {
                body.Execute(vm);
            }
            catch (BreakStatement.Signal)
            {
                return true;
            }
            catch (ContinueStatement.Signal)
            {
            }

            return false;
        }
    }

    /// <summary>
    /// Represents a break statement in the AST.
    /// </summary>
    public class BreakStatement : IAST
    {
        public BreakStatement()
        {
        }

        /// <summary>
        /// Signal used to notify parent nodes that a break statement has been executed.
        /// </summary>
        public class Signal : Exception
        {
        }

        /// <summary>
        /// Throws a <see cref="Signal"/>.
        /// </summary>
        public Value Execute(VM vm)
        {
            throw new Signal();
        }
    }

    /// <summary>
    /// Represents a continue statement in the AST.
    /// </summary>
    public class ContinueStatement : IAST
    {
        public ContinueStatement()
        {
        }

        /// <summary>
        /// Signal used to notify parent nodes that a continue statement has been executed.
        /// </summary>
        public class Signal : Exception
        {
        }

        /// <summary>
        /// Throws a <see cref="Signal"/>.
        /// </summary>
        public Value Execute(VM vm)
        {
            throw new Signal();
        }
    }

    /// <summary>
    /// Represents a lambda expression or function / method in the AST.
    /// </summary>
    public class LambdaExpression : IAST
    {
        /// <summary>
        /// Names of the lambda's arguments.
        /// </summary>
        public List<string> Args { get; private set; }

        /// <summary>
        /// Body of the lambda.
        /// </summary>
        public Block Body { get; private set; }

        /// <summary>
        /// Name of the function / method.
        /// </summary>
        public string Id { get; private set; }

        private bool varargs;

        public LambdaExpression(List<string> args, Block body, bool varargs, string id = "<LAMBDA>")
        {
            Args = args;
            Body = body;
            this.varargs = varargs;
            Id = id;
        }

        /// <summary>
        /// Returns the <see cref="LambdaExpression"/> wrapped in a <see cref="LambdaValue"/>.
        /// </summary>
        /// <returns>A <see cref="LambdaValue"/>.</returns>
        public Value Execute(VM vm)
        {
            return new LambdaValue(this);
        }

        /// <summary>
        /// Check if lambda allows for a variable number of arguments or not.
        /// </summary>
        /// <returns>True if lambda is varargs. False if not.</returns>
        public bool IsVarargs()
        {
            return varargs;
        }
    }

    /// <summary>
    /// Represents a variable declaration in the AST.
    /// </summary>
    public class VariableDeclaration : IAST
    {
        protected string id;

        public VariableDeclaration(string id)
        {
            this.id = id;
        }

        /// <summary>
        /// Adds a <see cref="NilValue"/> in <see cref="VM.Variables"/> with <see cref="id"/>
        /// as its key.
        /// </summary>
        /// <param name="vm">The environment in which to add the new variable.</param>
        /// <returns>A <see cref="NilValue"/>.</returns>
        public virtual Value Execute(VM vm)
        {
            vm.Variables.Add(id, new NilValue());
            return new NilValue();
        }
    }

    /// <summary>
    /// Represents a variable instatiation in the AST.
    /// </summary>
    public class VariableInstantiation : VariableDeclaration
    {
        protected IAST initializer;

        public VariableInstantiation(string id, IAST initializer)
            : base(id)
        {
            this.initializer = initializer;
        }

        /// <summary>
        /// Adds a new variable in <see cref="VM.Variables"/> by executing <see cref="initializer"/>
        /// with 'id' as its key.
        /// </summary>
        /// <param name="vm">The environment in which to add the new variable.</param>
        /// <returns>A <see cref="NilValue"/>.</returns>
        public override Value Execute(VM vm)
        {
            Value value = initializer.Execute(vm);
            vm.Variables.Add(id, value);
            return new NilValue();
        }
    }

    /// <summary>
    /// Represents a constant instatiation in the AST.
    /// </summary>
    public class ConstantInstantiation : VariableInstantiation
    {
        public ConstantInstantiation(string id, IAST initializer)
            : base(id, initializer)
        {
        }

        /// <summary>
        /// Adds a new constant to <see cref="VM.Constants"/> by executing 'initializer' with
        /// 'id' as its key.
        /// </summary>
        /// <param name="vm">The environment in which to add the new constant.</param>
        /// <returns>A <see cref="NilValue"/>.</returns>
        public override Value Execute(VM vm)
        {
            Value value = initializer.Execute(vm);
            vm.Constants.Add(id, value);
            return new NilValue();
        }
    }

    /// <summary>
    /// Represents a class declaration in the AST.
    /// </summary>
    public class ClassDeclaration : IAST
    {
        string name;
        string superClass;
        List<LambdaExpression> methods;
        List<LambdaExpression> classMethods;

        public ClassDeclaration(string name, string superClass, List<LambdaExpression> methods, List<LambdaExpression> classMethods)
        {
            this.name = name;
            this.superClass = superClass;
            this.methods = methods;
            this.classMethods = classMethods;
        }

        /// <summary>
        /// Adds a new <see cref="ClassValue"/> to <see cref="VM.Constants"/>.
        /// </summary>
        /// <param name="vm">The environment in which to add the new class.</param>
        /// <returns>A <see cref="NilValue"/>.</returns>
        /// <exception cref="Exception">
        /// If there is an unresolved identifier for its super class.
        /// If super class identifier doesn't correspond to a <see cref="ClassValue"/>.
        /// </exception>
        public Value Execute(VM vm)
        {
            ClassValue super = null;
            if (superClass != null)
            {
                if (!vm.Constants.ContainsKey(superClass))
                {
                    throw new Exception(string.Format("Unresolved identifier '{0}'.", superClass));
                }

                Value superClassValue = vm.Constants[superClass];
                Value.AssertType(Value.ValueType.Class, superClassValue,
                    "Cannot inherit from something of type '{0}'.", superClassValue.Type);
                super = superClassValue.Class;
            }

            var @class = new ClassValue(name, super);

            if (super != null)
            {
                foreach (var method in super.Methods)
                {
                    string methodName = method.Key;
                    if (methodName == "<SUPER>") continue;
                    if (method.Key == "init") methodName = "<SUPER>";
                    @class.Methods[methodName] = method.Value;
                }

                foreach (var method in super.ClassMethods)
                {
                    @class.ClassMethods[method.Key] = method.Value;
                }
            }

            foreach (var method in methods)
            {
                string methodName = method.Id;
                LambdaValue methodVal = method.Execute(vm) as LambdaValue;
                @class.Methods[methodName] = methodVal;
            }

            foreach (var method in classMethods)
            {
                string methodName = method.Id;
                LambdaValue methodVal = method.Execute(vm) as LambdaValue;
                @class.ClassMethods[methodName] = methodVal;
            }

            vm.Constants.Add(name, @class);
            return new NilValue();
        }
    }

    /// <summary>
    /// Represents a member reference in the AST.
    /// </summary>
    public class MemberReference : IAST
    {
        /// <summary>
        /// Name of the member.
        /// </summary>
        public string Member { get; private set; }

        /// <summary>
        /// Instance that owns the member.
        /// </summary>
        public IAST Instance { get; private set; }

        public MemberReference(IAST instance, string member)
        {
            Member = member;
            Instance = instance;
        }

        /// <summary>
        /// Executes <see cref="Instance"/> and returns the member that corresponds to <see cref="Member"/>.
        /// </summary>
        /// <param name="vm">The environment in which to execute <see cref="Instance"/>.</param>
        /// <returns>The member of the instance.</returns>
        /// <exception cref="Exception">
        /// If <see cref="Instance"/> doesn't return a <see cref="InstanceValue"/>, <see cref="StringValue"/> or
        /// <see cref="ListValue"/>.
        /// </exception>
        public Value Execute(VM vm)
        {
            Value inst = Instance.Execute(vm);
            switch (inst.Type)
            {
                case Value.ValueType.Instance:
                    return MemberReferenceInstance(inst as InstanceValue);

                case Value.ValueType.String:
                    return MemberReferenceString(inst as StringValue);

                case Value.ValueType.List:
                    return MemberReferenceList(inst as ListValue);

                default:
                    throw new Exception(string.Format("Cannot refer to members of something of type '{0}'.", inst.Type));
            }
        }

        /// <summary>
        /// Returns the field that corresponds to <see cref="Member"/>.
        /// </summary>
        /// <param name="value">The <see cref="InstanceValue"/>.</param>
        /// <returns>The instance's field.</returns>
        private Value MemberReferenceInstance(InstanceValue value)
        {
            return value.Fields[Member];
        }

        /// <summary>
        /// Returns the member of the <see cref="StringValue"/> that corresponds to
        /// <see cref="Member"/>.
        /// </summary>
        /// <param name="value">The <see cref="StringValue"/>.</param>
        /// <returns>The <see cref="StringValue"/>'s member.</returns>
        /// <exception cref="Exception">If <see cref="Member"/> isn't a member of <see cref="StringValue"/>.</exception>
        private Value MemberReferenceString(StringValue value)
        {
            switch (Member)
            {
                case "length":
                    return new NumberValue(value.String.Length);

                default:
                    throw new Exception(string.Format("'{0}' is not a member of 'String'.", Member));
            }
        }

        /// <summary>
        /// Returns the member of the <see cref="ListValue"/> that corresponds to
        /// <see cref="Member"/>.
        /// </summary>
        /// <param name="value">The <see cref="ListValue"/>.</param>
        /// <returns>The <see cref="ListValue"/>'s member.</returns>
        /// <exception cref="Exception">If <see cref="Member"/> isn't a member of <see cref="ListValue"/>.</exception>
        private Value MemberReferenceList(ListValue value)
        {
            switch (Member)
            {
                case "capacity":
                    return new NumberValue(value.List.Capacity);

                case "length":
                    return new NumberValue(value.List.Count);

                default:
                    throw new Exception(string.Format("'{0}' is not a member of 'List'.", Member));
            }
        }
    }
}

// Unary.cs
using System;
namespace CustomProject
{
    /// <summary>
    /// Represents any unary operation in the AST.
    /// </summary>
    public abstract class Unary : IAST
    {
        /// <summary>
        /// The sub-expression of the <see cref="Unary"/> node.
        /// </summary>
        public IAST Expr { get; private set; }

        public Unary(IAST expr)
        {
            Expr = expr;
        }

        public abstract Value Execute(VM vm);
    }

    /// <summary>
    /// Represents a boolean not operation in the AST.
    /// </summary>
    public class Not : Unary
    {
        public Not(IAST expr)
            : base(expr)
        {
        }

        /// <summary>
        /// Executes sub-expression and returns its value notted.
        /// </summary>
        /// <param name="vm">The environment in which to execute the operation.</param>
        /// <returns>A <see cref="BooleanValue"/>.</returns>
        /// <exception cref="Exception">If sub-expression doesn't return a <see cref="BooleanValue"/>.</exception>
        public override Value Execute(VM vm)
        {
            Value a = Expr.Execute(vm);

            Value.AssertType(Value.ValueType.Boolean, a,
                "Operand of '!' expected to be 'Boolean' but was given '{0}'.", a.Type);

            return new BooleanValue(!a.Boolean);
        }
    }

    /// <summary>
    /// Represents a numeric negation operation in the AST.
    /// </summary>
    public class Negation : Unary
    {
        public Negation(IAST expr)
            : base(expr)
        {
        }

        /// <summary>
        /// Executes sub-expression and returns its negation.
        /// </summary>
        /// <param name="vm">The environment in which to execute the operation.</param>
        /// <returns>A <see cref="NumberValue"/>.</returns>
        /// <exception cref="Exception">If sub-expression doesn't return a <see cref="NumberValue"/>.</exception>
        public override Value Execute(VM vm)
        {
            Value a = Expr.Execute(vm);

            Value.AssertType(Value.ValueType.Number, a,
                "Argument to unary '-' expected to be 'Number' but was given '{0}'.", a.Type);

            float numA = a.Number;
            float numR = -numA;

            return new NumberValue(numR);
        }
    }

    /// <summary>
    /// Represents a return statement in the AST.
    /// </summary>
    public class ReturnStatement : Unary
    {
        public ReturnStatement(IAST expr)
            : base(expr)
        {
        }

        /// <summary>
        /// Signal used to notify parent nodes that a return statement has been executed.
        /// </summary>
        public class Signal : Exception
        {
            /// <summary>
            /// The <see cref="CustomProject.Value"/> being returned.
            /// </summary>
            public Value Value { get; private set; }

            public Signal(Value value)
            {
                Value = value;
            }
        }

        /// <summary>
        /// Throws a <see cref="Signal"/> with wrapped return <see cref="CustomProject.Value"/>.
        /// </summary>
        /// <param name="vm">The environment in which to execute the return statements sub-expression..</param>
        /// <returns></returns>
        public override Value Execute(VM vm)
        {
            if (Expr == null)
            {
                throw new Signal(new NilValue());
            }
            throw new Signal(Expr.Execute(vm));
        }
    }

    /// <summary>
    /// Represents a print statement in the AST.
    /// </summary>
    public class PrintStatement : Unary
    {
        public PrintStatement(IAST expr)
            : base(expr)
        {
        }

        /// <summary>
        /// Prints a <see cref="Value"/>'s string representation to the console.
        /// </summary>
        /// <param name="vm">The environment in which to execute the print statement.</param>
        /// <returns>A <see cref="NilValue"/>.</returns>
        public override Value Execute(VM vm)
        {
            Value value = Expr.Execute(vm);
            Console.WriteLine(value);
            return new NilValue();
        }
    }
}

// Binary.cs
using System;
using System.Collections.Generic;
using System.Text;

namespace CustomProject
{
    /// <summary>
    /// Represents any binary operation in the AST.
    /// </summary>
    public abstract class Binary : IAST
    {
        /// <summary>
        /// The left hand side sub-expression of the binary operator.
        /// </summary>
        public IAST Lhs { get; private set; }

        /// <summary>
        /// The right hand side sub-expression of the binary operator.
        /// </summary>
        public IAST Rhs { get; private set; }

        public Binary(IAST lhs, IAST rhs)
        {
            Lhs = lhs;
            Rhs = rhs;
        }

        public abstract Value Execute(VM vm);
    }

    /// <summary>
    /// Represents an equality operation in the AST.
    /// </summary>
    public class Equality : Binary
    {
        public Equality(IAST lhs, IAST rhs)
            : base(lhs, rhs)
        {
        }

        /// <summary>
        /// Executes sub-expressions and checks resulting <see cref="Value"/>s for equality.
        /// </summary>
        /// <param name="vm">The environment in which to execute the operation.</param>
        /// <returns>A <see cref="BooleanValue"/>. True if equal. False if not.</returns>
        public override Value Execute(VM vm)
        {
            Value a = Lhs.Execute(vm);
            Value b = Rhs.Execute(vm);
            bool equal = a.Equal(b);
            return new BooleanValue(equal);
        }
    }

    /// <summary>
    /// Represents a numeric addition operation in the AST.
    /// </summary>
    public class Addition : Binary
    {
        public Addition(IAST lhs, IAST rhs)
            : base(lhs, rhs)
        {
        }

        /// <summary>
        /// Executes sub-expressions and returns the sum of the resulting <see cref="NumberValue"/>s.
        /// </summary>
        /// <param name="vm">The environment in which to execute the operation.</param>
        /// <returns>A <see cref="NumberValue"/>. The sum of the sub-expressions.</returns>
        /// <exception cref="Exception">If either sub-expression doesn't return a <see cref="NumberValue"/>.</exception>
        public override Value Execute(VM vm)
        {
            Value a = Lhs.Execute(vm);
            Value b = Rhs.Execute(vm);

            Value.AssertType(Value.ValueType.Number, a,
                "First argument to '+' expected to be 'Number' but was given '{0}'.", a.Type);
            Value.AssertType(Value.ValueType.Number, b,
                "Second argument to '+' expected to be 'Number' but was given '{0}'.", b.Type);

            float numA = a.Number;
            float numB = b.Number;
            float numR = numA + numB;

            return new NumberValue(numR);
        }
    }

    /// <summary>
    /// Represents a numeric subtraction operation in the AST.
    /// </summary>
    public class Subtraction : Binary
    {
        public Subtraction(IAST lhs, IAST rhs)
            : base(lhs, rhs)
        {
        }

        /// <summary>
        /// Executes sub-expressions and returns the difference between the resulting <see cref="NumberValue"/>s.
        /// </summary>
        /// <param name="vm">The environment in which to execute the operation.</param>
        /// <returns>A <see cref="NumberValue"/>. The difference between the sub-expressions.</returns>
        /// <exception cref="Exception">If either sub-expression doesn't return a <see cref="NumberValue"/>.</exception>
        public override Value Execute(VM vm)
        {
            Value a = Lhs.Execute(vm);
            Value b = Rhs.Execute(vm);

            Value.AssertType(Value.ValueType.Number, a,
                "First argument to '-' expected to be 'Number' but was given '{0}'.", a.Type);
            Value.AssertType(Value.ValueType.Number, b,
                "Second argument to '-' expected to be 'Number' but was given '{0}'.", b.Type);

            float numA = a.Number;
            float numB = b.Number;
            float numR = numA - numB;

            return new NumberValue(numR);
        }
    }

    /// <summary>
    /// Represents a numeric multiplication operation in the AST.
    /// </summary>
    public class Multiplication : Binary
    {
        public Multiplication(IAST lhs, IAST rhs)
            : base(lhs, rhs)
        {
        }

        /// <summary>
        /// Executes sub-expressions and returns the product of the resulting <see cref="NumberValue"/>s.
        /// </summary>
        /// <param name="vm">The environment in which to execute the operation.</param>
        /// <returns>A <see cref="NumberValue"/>. The product of the sub-expressions.</returns>
        /// <exception cref="Exception">If either sub-expression doesn't return a <see cref="NumberValue"/>.</exception>
        public override Value Execute(VM vm)
        {
            Value a = Lhs.Execute(vm);
            Value b = Rhs.Execute(vm);

            Value.AssertType(Value.ValueType.Number, a,
                "First argument to '*' expected to be 'Number' but was given '{0}'.", a.Type);
            Value.AssertType(Value.ValueType.Number, b,
                "Second argument to '*' expected to be 'Number' but was given '{0}'.", b.Type);

            float numA = a.Number;
            float numB = b.Number;
            float numR = numA * numB;

            return new NumberValue(numR);
        }
    }

    /// <summary>
    /// Represents a numeric division operation in the AST.
    /// </summary>
    public class Division : Binary
    {
        public Division(IAST lhs, IAST rhs)
            : base(lhs, rhs)
        {
        }

        /// <summary>
        /// Executes sub-expressions and returns the quotient of the resulting <see cref="NumberValue"/>s.
        /// </summary>
        /// <param name="vm">The environment in which to execute the operation.</param>
        /// <returns>A <see cref="NumberValue"/>. The quotient of the sub-expressions.</returns>
        /// <exception cref="Exception">If either sub-expression doesn't return a <see cref="NumberValue"/>.</exception>
        public override Value Execute(VM vm)
        {
            Value a = Lhs.Execute(vm);
            Value b = Rhs.Execute(vm);

            Value.AssertType(Value.ValueType.Number, a,
                "First argument to '/' expected to be 'Number' but was given '{0}'.", a.Type);
            Value.AssertType(Value.ValueType.Number, b,
                "Second argument to '/' expected to be 'Number' but was given '{0}'.", b.Type);

            float numA = a.Number;
            float numB = b.Number;
            float numR = numA / numB;

            return new NumberValue(numR);
        }
    }

    /// <summary>
    /// Represents a boolean or operation in the AST.
    /// </summary>
    public class Or : Binary
    {
        public Or(IAST lhs, IAST rhs)
            : base(lhs, rhs)
        {
        }

        /// <summary>
        /// Evaluates Lhs or Rhs with short-circuiting.
        /// </summary>
        /// <param name="vm">The environment in which to execute the operation.</param>
        /// <returns>A <see cref="BooleanValue"/>. True if either sub-expression is true. False if not.</returns>
        /// <exception cref="Exception">If either sub-experssion doesn't return a <see cref="BooleanValue"/>.</exception>
        public override Value Execute(VM vm)
        {
            Value a = Lhs.Execute(vm);
            Value.AssertType(Value.ValueType.Boolean, a,
                "First operand of 'or' expected to be 'Boolean' but was given '{0}'.", a.Type);
            bool boolA = a.Boolean;

            if (boolA)
            {
                return new BooleanValue(boolA);
            }


            Value b = Rhs.Execute(vm);
            Value.AssertType(Value.ValueType.Boolean, b,
                "Second operand of 'or' expected to be 'Boolean' but was given '{0}'.", b.Type);
            return new BooleanValue(b.Boolean);
        }
    }

    /// <summary>
    /// Represents a boolean and operation in the AST.
    /// </summary>
    public class And : Binary
    {
        public And(IAST lhs, IAST rhs)
            : base(lhs, rhs)
        {
        }

        /// <summary>
        /// Evaluates Lhs and Rhs with short-circuiting.
        /// </summary>
        /// <param name="vm">The environment in which to execute the operation.</param>
        /// <returns>A <see cref="BooleanValue"/>. True if both sub-expression are true. False if not.</returns>
        /// <exception cref="Exception">if either sub-expression doesn't return a <see cref="BooleanValue"/>.</exception>
        public override Value Execute(VM vm)
        {
            Value a = Lhs.Execute(vm);
            Value.AssertType(Value.ValueType.Boolean, a,
                "First operand of 'or' expected to be 'Boolean' but was given '{0}'.", a.Type);
            bool boolA = a.Boolean;

            if (!boolA)
            {
                return new BooleanValue(boolA);
            }


            Value b = Rhs.Execute(vm);
            Value.AssertType(Value.ValueType.Boolean, b,
                "Second operand of 'or' expected to be 'Boolean' but was given '{0}'.", b.Type);
            return new BooleanValue(b.Boolean);
        }
    }

    /// <summary>
    /// Represents a numeric less-than operation in the AST.
    /// </summary>
    public class LessThan : Binary
    {
        public LessThan(IAST lhs, IAST rhs)
            : base(lhs, rhs)
        {
        }

        /// <summary>
        /// Evaluates Lhs less-than Rhs and returns the result.
        /// </summary>
        /// <param name="vm">The environment in which to execute the operation.</param>
        /// <returns>A <see cref="BooleanValue"/>. True if Lhs less-than Rhs. False if not.</returns>
        /// <exception cref="Exception">If either sub-expression doesn't return a <see cref="NumberValue"/>.</exception>
        public override Value Execute(VM vm)
        {
            Value a = Lhs.Execute(vm);
            Value b = Rhs.Execute(vm);

            Value.AssertType(Value.ValueType.Number, a,
                "First operand of '<' expected to be 'Number' but was given '{0}'.", a.Type);
            Value.AssertType(Value.ValueType.Number, b,
                "Second operand of '<' expected to be 'Number' but was given '{0}'.", b.Type);

            float numA = a.Number;
            float numB = b.Number;

            return new BooleanValue(numA < numB);
        }
    }

    /// <summary>
    /// Represents a numeric greater-than operation in the AST.
    /// </summary>
    public class GreaterThan : Binary
    {
        public GreaterThan(IAST lhs, IAST rhs)
            : base(lhs, rhs)
        {
        }

        /// <summary>
        /// Evaluates Lhs > Rhs and returns the result.
        /// </summary>
        /// <param name="vm">The environment in which to execute the operation.</param>
        /// <returns>A <see cref="BooleanValue"/>. True if Lhs > Rhs. False if not.</returns>
        /// <exception cref="Exception">If either sub-expression doesn't return a <see cref="NumberValue"/>.</exception>
        public override Value Execute(VM vm)
        {
            Value a = Lhs.Execute(vm);
            Value b = Rhs.Execute(vm);

            Value.AssertType(Value.ValueType.Number, a,
                "First operand of '>' expected to be 'Number' but was given '{0}'.", a.Type);
            Value.AssertType(Value.ValueType.Number, b,
                "Second operand of '>' expected to be 'Number' but was given '{0}'.", b.Type);

            float numA = a.Number;
            float numB = b.Number;

            return new BooleanValue(numA > numB);
        }
    }

    /// <summary>
    /// Represents a List subscript operation in the AST.
    /// </summary>
    public class Subscript : Binary
    {
        public Subscript(IAST lhs, IAST rhs)
            : base(lhs, rhs)
        {
        }

        /// <summary>
        /// Returns the <see cref="Value"/> in the list gotten by Lhs at the index gotten by Rhs.
        /// </summary>
        /// <param name="vm">The environment in which to execute the operation.</param>
        /// <returns>The element in the list.</returns>
        /// <exception cref="Exception">If there was a type error.</exception>
        public override Value Execute(VM vm)
        {
            Value list = Lhs.Execute(vm);
            Value idx = Rhs.Execute(vm);

            Value.AssertType(Value.ValueType.List, list,
                "First operand to '[]' expected to be 'List' but was given '{0}'.", list.Type);
            Value.AssertType(Value.ValueType.Number, idx,
                "Second operand to '[]' expected to be 'Number' but was given '{0}'.", idx.Type);

            List<Value> values = list.List;
            float i = idx.Number;

            return values[(int)i];
        }
    }

    /// <summary>
    /// Represents an invocation in the AST.
    /// </summary>
    public class Invocation : Binary
    {
        public Invocation(IAST lhs, IAST rhs)
            : base(lhs, rhs)
        {
        }

        /// <summary>
        /// Invokes a value and returns the resulting <see cref="Value"/>.
        /// </summary>
        /// <param name="vm">The environment in which to execute the invocation.</param>
        /// <returns>A <see cref="BooleanValue"/>. True if Lhs less-than Rhs. False if not.</returns>
        /// <exception cref="Exception">If Lhs isn't invocable.</exception>
        public override Value Execute(VM vm)
        {
            if (Lhs is BoundMethod boundMethod)
            {
                return InvokeBoundMethod(vm, boundMethod);
            }

            Value callee = Lhs.Execute(vm);
            switch (callee.Type)
            {
                case Value.ValueType.Lambda:
                    return InvokeLambda(vm, callee as LambdaValue);

                case Value.ValueType.Class:
                    return InvokeClass(vm, callee as ClassValue);

                default:
                    throw new Exception(string.Format("Cannot invoke something of type '{0}'.", callee.Type));
            }
        }

        /// <summary>
        /// Calls a lambda with a given <see cref="VM"/>.
        /// </summary>
        /// <param name="lambda">The lambda to invoke.</param>
        /// <param name="vm">The VM to invoke the lambda with.</param>
        /// <returns>The lambda's return value.</returns>
        private Value CallWith(LambdaExpression lambda, VM vm)
        {
            Value ret;
            try
            {
                ret = lambda.Body.Execute(vm);
            }
            catch (ReturnStatement.Signal sig)
            {
                ret = sig.Value;
            }

            return ret;
        }

        /// <summary>
        /// Executes argument expressions of the invocation and puts them into '@new's Variables dictionary.
        /// </summary>
        /// <param name="vm">The environment in which to execute the arguments.</param>
        /// <param name="new">Where to put the arguments.</param>
        /// <param name="lambda">Lambda that will be invoked.</param>
        /// <param name="args">Arguments being passed to the lambda.</param>
        /// <exception cref="Exception">If there is an argument mismatch.</exception>
        private void SetArguments(VM vm, VM @new, LambdaExpression lambda, List<IAST> args)
        {
            bool isVarargs = lambda.IsVarargs();

            if (isVarargs)
            {
                if (args.Count < lambda.Args.Count - 1)
                {
                    throw new Exception(string.Format(
                        "Argument Mismatch! '{0}' takes at least {1} argument(s) but was given {2}.",
                        lambda.Id, lambda.Args.Count - 1, args.Count));
                }
            }
            else if (args.Count != lambda.Args.Count)
            {
                throw new Exception(string.Format(
                    "Argument Mistmatch! '{0}' takes {1} argument(s) but was given {2}.",
                    lambda.Id, lambda.Args.Count, args.Count));
            }

            if (isVarargs)
            {
                for (int i = 0; i < lambda.Args.Count - 1; i++)
                {
                    string argId = lambda.Args[i];
                    Value arg = args[i].Execute(vm);
                    @new.Variables.Add(argId, arg);
                }

                List<Value> varargs = new List<Value>();
                for (int i = lambda.Args.Count - 1; i < args.Count; i++)
                {
                    varargs.Add(args[i].Execute(vm));
                }

                string varargsId = lambda.Args[lambda.Args.Count - 1];
                @new.Variables.Add(varargsId, new ListValue(varargs));
            }
            else
            {
                for (int i = 0; i < lambda.Args.Count; i++)
                {
                    string argId = lambda.Args[i];
                    Value arg = args[i].Execute(vm);
                    @new.Variables.Add(argId, arg);
                }
            }
        }

        /// <summary>
        /// Invokes a given lambda.
        /// </summary>
        /// <param name="vm">The environment in which to invoke the lambda.</param>
        /// <param name="value">The lambda to invoke.</param>
        /// <returns>The lambda's return value.</returns>
        /// <exception cref="Exception">If Rhs doesn't return a <see cref="Block"/>.</exception>
        private Value InvokeLambda(VM vm, LambdaValue value)
        {
            LambdaExpression lambda = value.Lambda;
            Block args = Rhs as Block;
            if (args == null)
            {
                throw new Exception("Internal: 'rhs' of 'Invocation' was not a 'Block'.");
            }

            VM @new = new VM(null, vm.Global);
            @new.Constants.Add(lambda.Id, value); // For recursion
            SetArguments(vm, @new, lambda, args.Nodes);
            return CallWith(lambda, @new);
        }

        /// <summary>
        /// Invokes a given class.
        /// </summary>
        /// <param name="vm">The environment in which to invoke the class.</param>
        /// <param name="value">The class to invoke.</param>
        /// <returns>A new <see cref="InstanceValue"/> of the given class.</returns>
        /// <exception cref="Exception">
        /// If Rhs doesn't return a <see cref="Block"/> or
        /// if 'init()' doesn't return a <see cref="NilValue"/>.
        /// </exception>
        private Value InvokeClass(VM vm, ClassValue value)
        {
            var self = new InstanceValue(value);

            if (value.Methods.ContainsKey("init"))
            {
                var init = value.Methods["init"].Lambda;

                Block args = Rhs as Block;
                if (args == null)
                {
                    throw new Exception("Internal: 'rhs' of 'Invocation' was not a 'Block'.");
                }

                VM @new = new VM(null, vm.Global);
                @new.Constants.Add(value.Name, value);
                @new.Constants.Add("self", self);
                SetArguments(vm, @new, init, args.Nodes);

                try
                {
                    init.Body.Execute(@new);
                }
                catch (ReturnStatement.Signal sig)
                {
                    if (!sig.Value.IsNil())
                    {
                        throw new Exception("Cannot return a non-nil value from class initializer.");
                    }
                }
            }

            return self;
        }

        /// <summary>
        /// Invokes a method with a receiver as an implicit first argument.
        /// </summary>
        /// <param name="vm">The environment in which to invoke the method.</param>
        /// <param name="boundMethod">The bound method with the method's name and the receiver.</param>
        /// <returns>The method's return value.</returns>
        /// <exception cref="Exception">If type error.</exception>
        private Value InvokeBoundMethod(VM vm, BoundMethod boundMethod)
        {
            Value receiverValue = boundMethod.Receiver.Execute(vm);

            switch (receiverValue.Type)
            {
                case Value.ValueType.Instance:
                    return InvokeBoundMethodInstance(vm, receiverValue.Instance, boundMethod.Method);

                case Value.ValueType.List:
                    return InvokeBoundMethodList(vm, receiverValue.List, boundMethod.Method);

                case Value.ValueType.String:
                    return InvokeBoundMethodString(vm, receiverValue as StringValue, boundMethod.Method);

                case Value.ValueType.Class:
                    return InvokeBoundMethodClass(vm, receiverValue.Class, boundMethod.Method);

                default:
                    throw new Exception(string.Format("Cannot invoke something of type '{0}'.", receiverValue.Type));
            }
        }

        /// <summary>
        /// Invokes a method with an <see cref="InstanceValue"/> as the receiver.
        /// </summary>
        /// <param name="vm">The environment in which to invoke the method.</param>
        /// <param name="receiver">The receiver of the method.</param>
        /// <param name="methodName">The name of the method.</param>
        /// <returns>The method's return value.</returns>
        /// <exception cref="Exception">
        /// If the methodName isn't a method of the class or
        /// if Rhs isn't a <see cref="Block"/>.
        /// </exception>
        private Value InvokeBoundMethodInstance(VM vm, InstanceValue receiver, string methodName)
        {
            ClassValue @class = receiver.Class;

            if (!@class.Methods.ContainsKey(methodName))
            {
                throw new Exception(string.Format("'{0}' is not a method of '{1}'.", methodName, @class.Name));
            }

            var method = @class.Methods[methodName].Lambda;

            Block args = Rhs as Block;
            if (args == null)
            {
                throw new Exception("Internal: 'rhs' of 'Invocation' was not a 'Block'.");
            }

            VM @new = new VM(null, vm.Global);
            @new.Constants.Add("self", receiver);
            SetArguments(vm, @new, method, args.Nodes);
            return CallWith(method, @new);
        }

        /// <summary>
        /// Invokes a method with a <see cref="ListValue"/> as the receiver.
        /// </summary>
        /// <param name="vm">The environment in which to invoke the method.</param>
        /// <param name="list">The receiver of the method.</param>
        /// <param name="methodName">The name of the method.</param>
        /// <returns>A <see cref="NilValue"/>.</returns>
        /// <exception cref="Exception">
        /// If the methodName isn't a method of the class or
        /// if Rhs isn't a <see cref="Block"/> or
        /// if there was an argument mismatch.
        /// </exception>
        private Value InvokeBoundMethodList(VM vm, List<Value> list, string methodName)
        {
            switch (methodName)
            {
                case "add":
                    {
                        Block args = Rhs as Block;
                        if (args == null)
                        {
                            throw new Exception("Internal: 'rhs' of 'Invocation' was not a 'Block'.");
                        }

                        if (args.Nodes.Count != 1)
                        {
                            throw new Exception(string.Format("Argument Mistmatch! List.add takes 1 argument(s) but was given {0}.", args.Nodes.Count));
                        }

                        Value value = args.Nodes[0].Execute(vm);
                        list.Add(value);
                        break;
                    }
                case "insert":
                    {
                        Block args = Rhs as Block;
                        if (args == null)
                        {
                            throw new Exception("Internal: 'rhs' of 'Invocation' was not a 'Block'.");
                        }

                        if (args.Nodes.Count != 2)
                        {
                            throw new Exception(string.Format("Argument Mistmatch! List.push takes 2 argument(s) but was given {0}.", args.Nodes.Count));
                        }

                        Value idx = args.Nodes[0].Execute(vm);
                        Value.AssertType(Value.ValueType.Number, idx,
                            "Type mismatch! List.remove expects first argument to be 'Number' but was given '{0}'.", idx.Type);

                        Value value = args.Nodes[1].Execute(vm);

                        list.Insert((int)idx.Number, value);
                        break;
                    }
                case "find":
                    {
                        Block args = Rhs as Block;
                        if (args == null)
                        {
                            throw new Exception("Internal: 'rhs' of 'Invocation' was not a 'Block'.");
                        }

                        if (args.Nodes.Count != 1)
                        {
                            throw new Exception(string.Format("Argument Mistmatch! List.push takes 1 argument(s) but was given {0}.", args.Nodes.Count));
                        }

                        Value value = args.Nodes[0].Execute(vm);
                        return new NumberValue(list.FindIndex(v => v.Equal(value)));
                    }
                case "remove":
                    {
                        Block args = Rhs as Block;
                        if (args == null)
                        {
                            throw new Exception("Internal: 'rhs' of 'Invocation' was not a 'Block'.");
                        }

                        if (args.Nodes.Count != 1)
                        {
                            throw new Exception(string.Format("Argument Mistmatch! List.push takes 1 argument(s) but was given {0}.", args.Nodes.Count));
                        }

                        Value value = args.Nodes[0].Execute(vm);
                        Value.AssertType(Value.ValueType.Number, value,
                            "Type mismatch! List.remove expects first argument to be 'Number' but was given '{0}'.", value.Type);

                        list.RemoveAt((int)value.Number);
                        break;
                    }

                default:
                    throw new Exception(string.Format("'{0}' is not a method of 'List'.", methodName));
            }

            return new NilValue();
        }

        /// <summary>
        /// Invokes a method with a <see cref="StringValue"/> as the receiver.
        /// </summary>
        /// <param name="vm">The environment in which to invoke the method.</param>
        /// <param name="str">The receiver of the method.</param>
        /// <param name="methodName">The name of the method.</param>
        /// <returns>A <see cref="NilValue"/>.</returns>
        /// <exception cref="Exception">
        /// If the methodName isn't a method of the class or
        /// if Rhs isn't a <see cref="Block"/>.
        /// </exception>
        private Value InvokeBoundMethodString(VM vm, StringValue str, string methodName)
        {
            switch (methodName)
            {
                case "concat":
                    {
                        Block args = Rhs as Block;
                        if (args == null)
                        {
                            throw new Exception("Internal: 'rhs' of 'Invocation' was not a 'Block'.");
                        }

                        StringBuilder builder = new StringBuilder(str.String);
                        foreach (IAST expr in args.Nodes)
                        {
                            Value arg = expr.Execute(vm);
                            builder.Append(arg.ToString());
                        }

                        str.ReplaceString(builder.ToString());
                        break;
                    }

                default:
                    throw new Exception(string.Format("'{0}' is not a method of 'String'.", methodName));
            }

            return new NilValue();
        }

        /// <summary>
        /// Invokes a class method that doesn't take an instance as a receiver.
        /// </summary>
        /// <param name="vm">The environment in which to invoke the method.</param>
        /// <param name="value">The class the method belongs to.</param>
        /// <param name="methodName">The name of the method.</param>
        /// <returns>The method's return value.</returns>
        /// <exception cref="Exception">
        /// If the methodName isn't a method of the class or
        /// if Rhs isn't a <see cref="Block"/>.
        /// </exception>
        private Value InvokeBoundMethodClass(VM vm, ClassValue value, string methodName)
        {
            if (!value.ClassMethods.ContainsKey(methodName))
            {
                if (value.Methods.ContainsKey(methodName))
                {
                    throw new Exception(string.Format(
                        "'{0}' is not a class method of '{1}'. Requires an instance of the class.",
                        methodName,
                        value.Name));
                }
                else
                {
                    throw new Exception(string.Format("'{0}' is not a method of '{1}'.", methodName, value.Name));
                }
            }

            LambdaExpression method = value.ClassMethods[methodName].Lambda;

            Block args = Rhs as Block;
            if (args == null)
            {
                throw new Exception("Internal: 'rhs' of 'Invocation' was not a 'Block'.");
            }

            VM @new = new VM(null, vm.Global);
            SetArguments(vm, @new, method, args.Nodes);
            return CallWith(method, @new);
        }
    }

    /// <summary>
    /// Represents a range expression in the AST.
    /// </summary>
    public class RangeExpression : Binary
    {
        bool inclusive;

        public RangeExpression(IAST lhs, IAST rhs, bool inclusive)
            : base(lhs, rhs)
        {
            this.inclusive = inclusive;
        }

        /// <summary>
        /// Executes sub-expressions and returns a <see cref="RangeValue"/>.
        /// </summary>
        /// <param name="vm">The environment in which to execute the sub-expressions.</param>
        /// <returns>A <see cref="RangeValue"/>. A range between Lhs and Rhs.</returns>
        /// <exception cref="Exception">If type error.</exception>
        public override Value Execute(VM vm)
        {
            Value start = Lhs.Execute(vm);
            Value end = Rhs.Execute(vm);

            if (!Value.TypesMatch(start, end))
            {
                throw new Exception("start and end values of 'Range' must be the same type.");
            }

            if (start.Type != Value.ValueType.Number && start.Type != Value.ValueType.Char)
            {
                throw new Exception(string.Format("Cannot make a range over '{0}'.", start.Type));
            }

            if (end.Type != Value.ValueType.Number && end.Type != Value.ValueType.Char)
            {
                throw new Exception(string.Format("Cannot make a range over '{0}'.", end.Type));
            }

            return new RangeValue(start, end, inclusive);
        }
    }
}

// VM.cs
using System;
using System.Collections.Generic;

namespace CustomProject
{
    /// <summary>
    /// Responsible for executing the AST as well as storing all variables and constants
    /// generated during execution.
    /// </summary>
    public class VM
    {
        /// <summary>
        /// Used to access variables and constants in a parent scope.
        /// </summary>
        public VM Parent { get; private set; }

        /// <summary>
        /// Used to access the global scope of the program.
        /// </summary>
        public VM Global { get; private set; }

        /// <summary>
        /// Stores all variables in this scope.
        /// </summary>
        public Dictionary<string, Value> Variables { get; private set; }

        /// <summary>
        /// Stores all constants in this scope.
        /// </summary>
        public Dictionary<string, Value> Constants { get; private set; }

        public VM()
        {
            Variables = new Dictionary<string, Value>();
            Constants = new Dictionary<string, Value>();
        }

        /// <summary>
        /// Initializes the <see cref="VM"/> and Sets its environment.
        /// </summary>
        /// <param name="parent">The parent scope.</param>
        /// <param name="global">Global scope.</param>
        public VM(VM parent, VM global)
            : this()
        {
            SetEnvironment(parent, global);
        }

        /// <summary>
        /// Sets the <see cref="VM"/>'s environment.
        /// </summary>
        /// <param name="parent">The parent scope.</param>
        /// <param name="global">Global scope.</param>
        public void SetEnvironment(VM parent, VM global)
        {
            Parent = parent;
            Global = global;
        }

        /// <summary>
        /// Executes the given program.
        /// </summary>
        /// <param name="program">Program to be executed as an AST.</param>
        public void Execute(List<IAST> program)
        {
            foreach (IAST statement in program)
            {
                try
                {
                    statement.Execute(this);
                }
                catch (Exception e)
                {
                    Console.WriteLine(e.Message);
                    break;
                }
            }
        }
    }
}
