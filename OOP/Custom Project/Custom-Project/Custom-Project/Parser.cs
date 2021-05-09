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

        private enum InstantiationKind
        {
            Variable,
            Constant,
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

        private static ParseRule GetRule(Token.Kind kind)
        {
            return parseRules[(int)kind];
        }

        private List<Token> tokens;
        private bool error;
        private int peekIndex;
        private int loopDepth;
        private int lambdaDepth;
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
        }

        private void Advance()
        {
            if (peekIndex < tokens.Count)
            {
                peekIndex++;
            }
        }

        private void HandleError(Exception err)
        {
            error = true;
            Console.WriteLine(err.Message);
            while (!CheckPrevious(Token.Kind.EndStatement) && !Check(Token.Kind.EOF))
            {
                Advance();
            }
        }

        private bool Check(Token.Kind kind)
        {
            return Current.kind == kind;
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
                Error(format, args);
            }
        }

        private void Error(string format, params object[] args)
        {
            throw new Exception(string.Format(format, args));
        }

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

        private void ParseBreakStatement()
        {
            ast = new BreakStatement();
        }

        private void ParseContinueStatement()
        {
            ast = new ContinueStatement();
        }

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

        private void ParsePrintStatement()
        {
            ParseExpression();
            ast = new PrintStatement(ast);
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

        private void ParseBlock(int indentation)
        {
            Block block = new Block();
            while (Current.indentation > indentation)
            {
                try
                {
                    ParseDeclaration();
                    block.AddExpression(ast);
                }
                catch (Exception e)
                {
                    HandleError(e);
                }
            }
            ast = block;
        }

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

        private static void GroupParseFn(Parser parser)
        {
            parser.ParseGroup();
        }

        private void ParseGroup()
        {
            ParseExpression();
            Expect(Token.Kind.DelimCloseParenthesis, "Expected ')' to terminate parenthesised expression.");
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

        private static void UnaryParseFn(Parser parser)
        {
            parser.ParseUnary();
        }

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

        private static void BinaryParseFn(Parser parser)
        {
            parser.ParseBinary();
        }

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

        private static void MemberReferenceParseFn(Parser parser)
        {
            parser.ParseMemberReference();
        }

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
                        args.AddExpression(ast);
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

        private static void AssigmentParseFn(Parser parser)
        {
            parser.ParseAssignment();
        }

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

        private void ParseAssignmentIdentifier(Identifier id)
        {
            ParseExpression();
            IAST assigner = ast;
            ast = new VariableAssignment(id.Id, assigner);
        }

        private void ParseAssignmentSubscript(Subscript sub)
        {
            IAST list = sub.Lhs;
            IAST subscript = sub.Rhs;

            ParseExpression();
            IAST assigner = ast;

            ast = new SubscriptAssignment(list, subscript, assigner);
        }

        private void ParseAssignmentMemberReference(MemberReference memRef)
        {
            IAST instance = memRef.Instance;
            string member = memRef.Member;

            ParseExpression();
            IAST assigner = ast;

            ast = new MemberReferenceAssignment(instance, member, assigner);
        }

        private static void ListParseFn(Parser parser)
        {
            parser.ParseList();
        }

        private void ParseList()
        {
            ListExpression list = new ListExpression();
            if (!Check(Token.Kind.DelimCloseBracket))
            {
                do
                {
                    ParseExpression();
                    list.AddExpression(ast);
                } while (Next(Token.Kind.Comma) && !Check(Token.Kind.EOF));
            }
            Expect(Token.Kind.DelimCloseBracket, "Expected ']' keyword to terminate List literal.");
            ast = list;
        }

        private static void InvokeParseFn(Parser parser)
        {
            parser.ParseInvocation();
        }

        private void ParseInvocation()
        {
            IAST lhs = ast;

            Block args = new Block();

            if (!Next(Token.Kind.DelimCloseParenthesis))
            {
                do
                {
                    ParseExpression();
                    args.AddExpression(ast);
                } while (Next(Token.Kind.Comma) && !Check(Token.Kind.EOF));
                Expect(Token.Kind.DelimCloseParenthesis, "Expected ')' after invocation.");
            }

            ast = new Invocation(lhs, args);
        }

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

        private void ParseFunctionDeclaration()
        {
            ParseFunctionDeclarationUnwrapped();
            var lambda = ast as LambdaExpression;
            ast = new ConstantInstantiation(lambda.Id, lambda);
        }

        private static void LambdaParseFn(Parser parser)
        {
            parser.ParseLambdaExpression();
        }

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
                body.AddExpression(ast);
            }

            ast = new LambdaExpression(args, body, varargs);
        }

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

        private void ParseSuperStatement()
        {
            SuperStatement super = new SuperStatement();

            Expect(Token.Kind.DelimOpenParenthesis, "Expected '(' after 'super' keyword.");

            if (!Check(Token.Kind.DelimCloseParenthesis))
            {
                do
                {
                    ParseExpression();
                    super.AddExpression(ast);
                } while (Next(Token.Kind.Comma) && !Check(Token.Kind.EOF));
            }

            Expect(Token.Kind.DelimCloseParenthesis, "Expected ')' to terminate 'super' statement.");

            ast = super;
        }

        private void ParseIfStatement()
        {
            ParseExpression();
            IAST cond = ast;

            Expect(Token.Kind.EndStatement, "Body of 'if' statement must be on subsequent lines.");

            int indent = Previous.indentation;

            //Block then = new Block();

            //while (!Check(Token.Kind.KeywordEnd) &&
            //    !Check(Token.Kind.KeywordElif) &&
            //    !Check(Token.Kind.KeywordElse) &&
            //    !Check(Token.Kind.EOF))
            //{
            //    try
            //    {
            //        ParseDeclaration();
            //        then.AddExpression(ast);
            //    }
            //    catch (Exception e)
            //    {
            //        HandleError(e);
            //    }
            //}

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
