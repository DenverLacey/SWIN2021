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
        /// the declartion.
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
