
using Parser.AbstractSyntaxTree;
using Parser.AbstractSyntaxTree.Expressions;
using Parser.CodeLexer;

namespace Parser
{
    internal interface IParser
    {
        void Parse();
    }

    internal sealed class Parser : IParser
    {
        private readonly Lexer _lexer;
        private readonly ParserListener _parserListener;
        public Parser(Lexer lexer, IParserListener parserListener)
        {
            _lexer = lexer;
            _parserListener = new ParserListener(parserListener);
        }

        public void Parse()
        {
            MainLoop();
        }

        private void MainLoop()
        {
            while (true)
            {
                switch (PeekToken().TokenType)
                {
                    case TokenType.EndOfFile: return;
                    case TokenType.EndOfStatement: ConsumeToken(); break;
                    case TokenType.Identifier: HandleAssignmentExpression(true); break;
                    case TokenType.VariableDeclaration: HandleAssignmentExpression(false); break;
                    default: HandleTopLevelExpression(); break;
                }
            }
        }

        private void HandleAssignmentExpression(bool isReassignment)
        {
            _parserListener.EnterRule(nameof(HandleAssignmentExpression));

            var assigntmentExpression = ParseAssignmentExpression(isReassignment);
            _parserListener.ExitRule(assigntmentExpression);

            if (assigntmentExpression != null)
            {
                _parserListener.Listen();
            }
            else // on error resume next :)
            {
                ConsumeToken();
            }
        }

        private void HandleTopLevelExpression()
        {
            _parserListener.EnterRule(nameof(HandleTopLevelExpression));
            var functionExpression = ParseTopLevelExpression();

            _parserListener.ExitRule(functionExpression);

            if (functionExpression != null)
            {
                _parserListener.Listen();
            }
            else // on error resume next :)
            {
                ConsumeToken();
            }
        }

        private ExpressionBase? ParsePrimary()
        {
            var currentTokenType = PeekToken().TokenType;
            switch (currentTokenType)
            {
                case TokenType.Identifier: return ParseIdentifierExpression();
                case TokenType.Double: return ParseDoubleExpression();
                case TokenType.Float: return ParseFloatExpression();
                case TokenType.Integer: return ParseIntegerExpression();
                case TokenType.String: return ParseStringExpression();
                case TokenType.Character: return ParseCharacterExpression();
                default: throw new InvalidOperationException($"Encountered an unkown token {currentTokenType}.");// todo: what to do here?                    
            }
        }

        private ExpressionBase? ParseAssignmentExpression(bool isReassignment)
        {
            // todo: Check if the value thats being assigned a new value actually Exists in scope?
            var declarationTypeToken = isReassignment ? new Token() { TokenType = TokenType.ReAssignment } : ConsumeToken();

            var leftHandSideTok = PeekToken();
            var leftHandSide = ParsePrimary(); // variable name...

            if (leftHandSide == null)
            {
                LogParseError(leftHandSideTok, "variable name", "assignment of variable");
            }

            if (PeekToken().TokenType != TokenType.Assignment)
            {
                LogParseError(PeekToken(), LexerConstants.ASSIGN_OPERATOR, "assignment of variable");
                return null;
            }

            var assignmentTok = ConsumeToken(); // consume assign operator
            var valueExpression = ParseExpression();
            if (valueExpression == null)
            {
                LogParseError(assignmentTok, "value expression", "assignment of variable");
                return null;
            }

            return new AssignmentExpression(declarationTypeToken, assignmentTok, valueExpression);
        }

        private ExpressionBase? ParseExpression()
        {
            var leftHandSide = ParsePrimary();
            const int DEFAULT_OPERATOR_PRECENDENCE = 0;
            return leftHandSide == null
                ? null
                : ParseBinaryOperatorRightHandSide(leftHandSide, DEFAULT_OPERATOR_PRECENDENCE);
        }

        private ExpressionBase? ParseBinaryOperatorRightHandSide(ExpressionBase leftHandSide, int expressionPrecedence)
        {
            while (true)
            {
                // Peek instead of consume to determine precedence
                var currentTokenPrecendence = LexerConstants.OperatorPrecedence.Get(_lexer.PeekToken());

                if (currentTokenPrecendence < expressionPrecedence)
                {
                    return leftHandSide;
                }

                var binaryOperatorToken = ConsumeToken();// binary operator needs to be consumed before we can parse the right hand side.
                var rightHandSide = ParsePrimary();

                if (rightHandSide == null)
                {
                    return null;
                }

                var nextTokenPrecedence = LexerConstants.OperatorPrecedence.Get(PeekToken());

                if (currentTokenPrecendence < nextTokenPrecedence)
                {
                    rightHandSide = ParseBinaryOperatorRightHandSide(rightHandSide, currentTokenPrecendence + 1);
                    if (rightHandSide == null)
                    {
                        return null;
                    }
                }

                leftHandSide = new BinaryExpression(binaryOperatorToken, leftHandSide, rightHandSide);
            }
        }

        private FunctionCallExpression? ParseTopLevelExpression()
        {
            var expression = ParseExpression();
            if (expression == null)
            {
                return null;
            }

            //little bit hacky.. But it's an anonymous proto...
            var prototype = new PrototypeExpression(new Token() { Name = string.Empty }, Array.Empty<Token>());
            return new FunctionCallExpression(prototype, expression);
        }

        private PrototypeExpression? ParsePrototype()
        {
            if (PeekToken().TokenType != TokenType.Identifier)
            {
                // Expected a function name...
                LogParseError(PeekToken(), "a function name", "prototype");
                return null;
            }

            var functionToken = ConsumeToken();

            if (PeekToken().TokenType != TokenType.ParanthesesOpen)
            {
                LogParseError(PeekToken(), LexerConstants.PARANTHESES_OPEN, "prototype");
                return null;
            }
            ConsumeToken();

            var argumentNames = new List<Token>();

            while (PeekToken().TokenType == TokenType.Identifier)
            {
                argumentNames.Add(ConsumeToken());
            }

            if (PeekToken().TokenType != TokenType.ParanthesesClose)
            {
                LogParseError(PeekToken(), LexerConstants.PARANTHESES_CLOSE, "prototype");
                return null;
            }

            ConsumeToken();

            return new PrototypeExpression(functionToken, argumentNames.ToArray());
        }

        private ExpressionBase ParseIdentifierExpression()
        {
            //todo: make smarter so it knows whether this is a function call? Or should the lexer handle this for me, as it can also peek forward...
            //todo: how to handle nullables?
            var result = new VariableEvaluationExpression(ConsumeToken());

            return result;
        }

        private ExpressionBase ParseFloatExpression()
        {
            //todo: how to handle nullables?
            var result = new FloatExpression(ConsumeToken());

            return result;
        }

        private ExpressionBase ParseDoubleExpression()
        {
            //todo: how to handle nullables?
            var result = new DoubleExpression(ConsumeToken());

            return result;
        }

        private ExpressionBase ParseIntegerExpression()
        {
            //todo: how to handle nullables?
            var result = new IntegerExpression(ConsumeToken());

            return result;
        }

        private ExpressionBase ParseStringExpression()
        {
            //todo: how to handle nullables?
            var result = new StringExpression(ConsumeToken());

            return result;
        }

        private ExpressionBase ParseCharacterExpression()
        {
            if (PeekToken().StringValue?.Length > 1)
            {
                LogParseError(PeekToken(), "Character with length of 1", "Character initializer");
            }

            //todo: how to handle nullables?
            //should string value be filled?
            var result = new CharacterExpression(ConsumeToken());

            return result;
        }

        private void LogParseError(Token token, string expectedCharacter, string exptectedInOrAt)
        {
            Console.WriteLine($"Expected '{expectedCharacter}' in {exptectedInOrAt} at line: {token.Line}, column: {token.Column}.");
        }

        private Token PeekToken() => _lexer.PeekToken();
        private Token ConsumeToken() => _lexer.ConsumeToken();
    }
}
