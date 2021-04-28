using System;
using System.Collections.Generic;

namespace CustomProject
{
    public class Parser
    {
        private enum Precedence
        {
            None,
            Assignment, // =
            Or,         // ||
            And,        // &&
            Equality,   // == !=
            Term,       // + -
            Factor,     // * / %
            Unary,      // !
            Call,       // . () []
            Primary,
        }

        delegate void ParseFn(Parser p);

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

        private static ParseRule[] parseRules = new ParseRule[]
        {
            new ParseRule(Precedence.None, null, null), // EOF
            new ParseRule(Precedence.None, null, null), // Error
            new ParseRule(Precedence.None, null, null), // EndStatement

            new ParseRule(Precedence.None, LiteralParseFn, null), // LiteralNumber
            new ParseRule(Precedence.None, LiteralParseFn, null), // LiteralString

            new ParseRule(Precedence.None, IdentifierParseFn, null), // Identifier

            new ParseRule(Precedence.None, null, null), // KeywordVar
            new ParseRule(Precedence.None, null, null), // KeywordEnd
            new ParseRule(Precedence.None, null, null), // KeywordIf
            new ParseRule(Precedence.None, null, null), // KeywordElif
            new ParseRule(Precedence.None, null, null), // KeywordElse

            new ParseRule(Precedence.Term, null, BinaryParseFn), // OpPlus
            new ParseRule(Precedence.Assignment, null, AssigmentParseFn), // OpEqual
            new ParseRule(Precedence.Equality, null, BinaryParseFn), // OpDoubleEqual
        };

        private static ParseRule GetRule(Token.Kind kind)
        {
            return parseRules[(int)kind];
        }

        private List<Token> tokens;
        private int peekIndex;
        private IAST ast;

        private Token Previous { get => tokens[peekIndex - 1]; }
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
            tokens = new List<Token>();
            peekIndex = 0;
        }

        private void Advance()
        {
            if (peekIndex < tokens.Count)
            {
                peekIndex++;
            }
        }

        private bool Check(Token.Kind kind)
        {
            return Current.kind == kind || Current.kind == Token.Kind.EOF;
        }

        private bool CheckPrevious(Token.Kind kind)
        {
            return Previous.kind == kind || Current.kind == Token.Kind.EOF;
        }

        private bool Next(Token.Kind kind)
        {
            if (!Check(kind))
                return false;
            Advance();
            return true;
        }

        private void Expect(Token.Kind kind, string format, params object[] args)
        {
            Assert(Next(kind), format, args);
        }

        private void Assert(bool condition, string format, params object[] args)
        {
            if (!condition)
            {
                throw new Exception(string.Format(format, args));
            }
        }

        public List<IAST> Parse(List<Token> tokens)
        {
            this.tokens = tokens;
            List<IAST> program = new List<IAST>();

            bool error = false;

            while (!Check(Token.Kind.EOF))
            {
                try
                {
                    ParseDeclaration();
                    program.Add(ast);
                }
                catch (Exception e)
                {
                    error = true;
                    Console.WriteLine(e.Message);
                    while (!CheckPrevious(Token.Kind.EndStatement))
                    {
                        Advance();
                    }
                }
            }

            if (error)
            {
                return new List<IAST>();
            }
            return program;
        }

        private void ParseDeclaration()
        {
            if (Next(Token.Kind.KeywordVar))
            {
                ParseVariableInstantiation();
            }
            else
            {
                ParseStatement();
            }
            Expect(Token.Kind.EndStatement, "Unexpected expression(s) at the end of statement");
        }

        private void ParseStatement()
        {
            if (Next(Token.Kind.KeywordIf))
            {
                ParseIfStatement();
            }
            else
            {
                ParseExpression();
            }
        }

        private void ParseExpression()
        {
            ParsePrecedence(Precedence.Assignment);
        }

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

        private void ParseBlock()
        {
            Block block = new Block();
            while (!Check(Token.Kind.KeywordEnd))
            {
                ParseDeclaration();
                block.AddExpression(ast);
            }
            Expect(Token.Kind.KeywordEnd, "Expected 'end' keyword to finish block.");
            ast = block;
        }

        private void ParseVariableInstantiation()
        {
            Expect(Token.Kind.Identifier, "Expected an identifier after 'var' keyword.");
            string id = Previous.source;

            Expect(Token.Kind.OpEqual, "Expected '=' after identifer.");

            ParseExpression();
            IAST initilizer = ast;

            ast = new VariableInstantiation(id, initilizer);
        }

        private static void LiteralParseFn(Parser parser)
        {
            parser.ParseLiteral();
        }

        private void ParseLiteral()
        {
            ast = new Literal(Previous.value);
        }

        private static void IdentifierParseFn(Parser parser)
        {
            parser.ParseIdentifier();
        }

        private void ParseIdentifier()
        {
            ast = new Identifier(Previous.source);
        }

        private static void BinaryParseFn(Parser parser)
        {
            parser.ParseBinary();
        }

        private void ParseBinary()
        {
            Token.Kind op = Previous.kind;

            IAST lhs = ast;

            Precedence nextPrec = GetRule(Previous.kind).precedence + 1;
            ParsePrecedence(nextPrec);
            IAST rhs = ast;

            switch (op)
            {
                case Token.Kind.OpPlus:
                    ast = new Addition(lhs, rhs);
                    break;

                case Token.Kind.OpEqual:
                    throw new NotImplementedException();
                    break;

                case Token.Kind.OpDoubleEqual:
                    ast = new Equality(lhs, rhs);
                    break;
            }
        }

        private static void AssigmentParseFn(Parser parser)
        {
            parser.ParseAssignment();
        }

        private void ParseAssignment()
        {
            throw new NotImplementedException();
        }

        private void ParseIfStatement()
        {
            ParseExpression();
            IAST cond = ast;

            Expect(Token.Kind.EndStatement, "Body of 'if' statement must be on subsequent lines.");

            Block then = new Block();

            while (!Check(Token.Kind.KeywordEnd) &&
                !Check(Token.Kind.KeywordElif) &&
                !Check(Token.Kind.KeywordElse))
            {
                ParseDeclaration();
                then.AddExpression(ast);
            }

            IAST @else = null;
            if (Next(Token.Kind.KeywordElif))
            {
                ParseIfStatement();
                @else = ast;
            }
            else if (Next(Token.Kind.KeywordElse))
            {
                Expect(Token.Kind.EndStatement, "Body of 'else' block must be on subsequent lines.");
                ParseBlock();
                @else = ast;
            }
            else
            {
                Expect(Token.Kind.KeywordEnd, "Expected 'end' keyword after 'if' statement.");
            }

            ast = new IfStatement(cond, then, @else);
        }
    }
}
