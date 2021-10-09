using Lexing;
using Parsing.AbstractSyntaxTree.Expressions;
using System.Diagnostics;

namespace Parsing
{
    public interface IParser
    {
        Queue<ExpressionBase> Parse();
    }

    public sealed class Parser : IParser
    {
        private readonly Lexer _lexer;
        private readonly Queue<ExpressionBase> _expressions;

        public Parser(Lexer lexer)
        {
            _lexer = lexer;
            _expressions = new();
        }

        public Queue<ExpressionBase> Parse()
        {
            var isFinished = false;
            while (!isFinished)
            {
                isFinished = ProcessToken();
            }

            return _expressions;
        }

        private bool ProcessToken()
        {
            var peekedTokens = PeekTokens(2);
            Debug.Assert(peekedTokens.Length == 2);

            switch (PeekToken().TokenType)
            {
                case TokenType.EndOfFile:
                    {
                        return true;
                    }
                case TokenType.EndOfStatement:
                    {
                        ConsumeToken();
                        return false;
                    }
                case TokenType.Identifier:
                    {
                        //todo: handle assignment variants?
                        if (peekedTokens[1].TokenType is TokenType.Add or TokenType.Subtract or TokenType.Multiply or TokenType.Divide)
                        {
                            ConsumeIdentifierExpression();
                            return false;
                        }

                        ConsumeAssignmentExpression(true);
                        return false;
                    }
                case TokenType.VariableDeclaration:
                    {
                        ConsumeAssignmentExpression(false);
                        return false;
                    }
                default:
                    {
                        //Shouldn't getting here throw an exception?
                        //ParseFunctionCallExpression(); 
                        var exp = ParseExpression();
                        ConsumeExpression(exp);
                        return false;
                    }
            }
        }

        private void ConsumeExpression(ExpressionBase? expression)
        {
            if (expression == null)
            {
                // on error resume next :)
                ConsumeToken();
            }
            else
            {
                _expressions.Enqueue(expression);
            }
        }

        private void ConsumeIdentifierExpression()
        {
            var expr = ParseExpression();
            ConsumeExpression(expr);
        }

        private void ConsumeAssignmentExpression(bool isReassignment)
        {
            var assignmentExpression = ParseAssignmentExpression(isReassignment);
            ConsumeExpression(assignmentExpression);
        }

        private void ConsumeFunctionCallExpression()
        {
            var functionExpression = ParseFunctionCallExpression();
            ConsumeExpression(functionExpression);
        }

        private ExpressionBase? ParsePrimary()
        {
            var currentTokenType = PeekToken().TokenType;

            return currentTokenType switch
            {
                TokenType.EndOfStatement => null,
                TokenType.EndOfFile => null,
                TokenType.AccoladesClose => null, // end of a (sub) expression
                TokenType.ParanthesesClose => null, // end of a sub expression
                TokenType.Identifier => ParseIdentifierExpression(),
                TokenType.Double => ParseDoubleExpression(),
                TokenType.Float => ParseFloatExpression(),
                TokenType.Integer => ParseIntegerExpression(),
                TokenType.String => ParseStringExpression(),
                TokenType.Character => ParseCharacterExpression(),
                TokenType.True => ParseBooleanExpression(),
                TokenType.False => ParseBooleanExpression(),
                TokenType.If => ParseIfStatementExpression(),
                _ => throw new InvalidOperationException($"Encountered an unkown token {currentTokenType}."),// todo: what to do here?                    
            };
        }

        private ExpressionBase ParseBooleanExpression()
        {
            if (PeekToken().TokenType is (TokenType.True or TokenType.False))
            {
                return new BooleanExpression(ConsumeToken());
            }

            throw new InvalidOperationException($"{nameof(ParseBooleanExpression)} can only be used with Tokens True or False, in all other cases use a binary expression");
        }

        private ExpressionBase? ParseIfStatementExpression()
        {
            var ifToken = ConsumeToken();
            Debug.Assert(ifToken.TokenType == TokenType.If);
            var conditionalExpression = ParseExpression();
            Debug.Assert(conditionalExpression != null);

            if (ThrowParseError(TokenType.AccoladesOpen, "expected opening accolades"))
            {
                return null;
            }

            ConsumeToken();

            //todo: handle multiple { and } signs indented inside ?
            //todo: Or should we disallow such weird scopes.
            var body = new List<ExpressionBase> { };
            while (PeekToken().TokenType != TokenType.AccoladesClose)
            {
                var expression = ParseExpression();
                Debug.Assert(expression != null);
                body.Add(expression);
            }
            Debug.Assert(PeekToken().TokenType is TokenType.AccoladesClose);
            ConsumeToken();

            if (PeekToken().TokenType is not TokenType.Else)
            {
                return new IfStatementExpression(conditionalExpression, new BodyExpression(body), null);
            }

            // Consume else...
            Debug.Assert(PeekToken().TokenType is TokenType.Else);
            ConsumeToken();
            // Consume opening accolade
            var isIfStatementNext = PeekToken().TokenType is TokenType.If;
            if (!isIfStatementNext)
            {
                Debug.Assert(PeekToken().TokenType == TokenType.AccoladesOpen);
                ConsumeToken();
            }

            var elseExpression = isIfStatementNext ? ParseIfStatementExpression() : ParseExpression();

            // Consume closing accolade
            if (!isIfStatementNext)
            {
                Debug.Assert(PeekToken().TokenType == TokenType.AccoladesClose);
                ConsumeToken();
            }
            return new IfStatementExpression(conditionalExpression, new BodyExpression(body), elseExpression);
        }

        private ExpressionBase? ParseAssignmentExpression(bool isReassignment)
        {
            // todo: Check if the value thats being assigned a new value actually Exists in scope?
            var declarationTypeToken = isReassignment ? new Token() { TokenType = TokenType.ReAssignment } : ConsumeToken();

            var leftHandSideTok = PeekToken();

            if (ThrowParseError(TokenType.Identifier, "assignment of variable"))
            {
                return null;
            }

            var leftHandSideIdentifierExpression = ParseIdentifierExpression(); // variable name...

            if (leftHandSideIdentifierExpression == null)
            {
                ThrowParseError(leftHandSideTok, "variable name", "assignment of variable");
                return null;
            }

            if (PeekToken().TokenType is not (TokenType.Assignment or TokenType.AddAssign or TokenType.SubtractAssign or TokenType.MultiplyAssign or TokenType.DivideAssign))
            {
                ThrowParseError(PeekToken(), LexerConstants.ASSIGN_OPERATOR, "assignment of variable");
                return null;
            }

            var assignmentTok = ConsumeToken(); // consume assign operator
            var valueExpression = ParseExpression();
            if (valueExpression == null)
            {
                ThrowParseError(assignmentTok, "value expression", "assignment of variable");
                return null;
            }

            return new AssignmentExpression(declarationTypeToken, leftHandSideIdentifierExpression, assignmentTok, valueExpression);
        }

        private ExpressionBase? ParseExpression()
        {
            // Below code fixes the test but is not the solution, as just consuming it is not enough, it has to have priority because of ( sign.
            ConsumeParantheses();

            var leftHandSide = ParsePrimary();
            const int DEFAULT_OPERATOR_PRECENDENCE = 0;
            return leftHandSide == null
                ? null
                : ParseBinaryOperatorRightHandSide(leftHandSide, DEFAULT_OPERATOR_PRECENDENCE);
        }

        private (bool WasOpeningParanthese, int OpeningAmount, bool WasClosingParanthese, int ClosingAmount) ConsumeParantheses()
        {
            var currentPeek = PeekToken();
            var isOpen = currentPeek.TokenType is TokenType.ParanthesesOpen;
            var isClose = currentPeek.TokenType is TokenType.ParanthesesClose;
            var openCounter = 0;
            var closedCounter = 0;
            //todo: I don't think we need to know about all the starting layers... Could be wrong..
            //Order is important here.. First do openParantheses then the closing ones
            while (PeekToken().TokenType is TokenType.ParanthesesOpen)
            {
                ConsumeToken();
                openCounter++;
            }

            // Get rid of ending scopes.
            while (PeekToken().TokenType is TokenType.ParanthesesClose)
            {
                if (openCounter > 0)
                {
                    ThrowParseError(PeekToken(), "a statement instead of a closing paranthese", "binary operation.");
                    //Todo: Should we consume this token and check further..?
                }
                ConsumeToken();
                closedCounter++;
            }

            return (isOpen, openCounter, isClose, closedCounter);
        }

        private ExpressionBase? ParseBinaryOperatorRightHandSide(ExpressionBase leftHandSide, int previousExpressionPrecedence)
        {
            while (true)
            {
                // Peek instead of consume to determine precedence
                var currentTokenPrecendence = LexerConstants.OperatorPrecedence.Get(PeekToken());

                if (previousExpressionPrecedence > currentTokenPrecendence)
                {
                    if (PeekToken().TokenType is TokenType.ParanthesesClose)
                    {
                        // if we return the left hand side, and thus making it a right hand side..
                        // Because it might leave some trailing closing parantheses..
                        // We should get rid of those, as we have processed everything of importance at this point.
                        ConsumeParantheses();
                    }

                    return leftHandSide;
                }

                // binary operator needs to be consumed before we can parse the right hand side.
                var binaryOperatorToken = ConsumeToken();
                var availableParanthesesBeforeParsingRightHandSide = ConsumeParantheses();

                var rightHandSide = ParsePrimary();

                if (rightHandSide == null)
                {
                    return null;
                }

                var availableParanthesesAfterParsingRightHandSide = ConsumeParantheses();
                if (availableParanthesesAfterParsingRightHandSide.WasClosingParanthese)
                {
                    if (availableParanthesesAfterParsingRightHandSide.ClosingAmount == 1)
                    {
                        return ParseBinaryOperatorRightHandSide(new BinaryExpression(binaryOperatorToken, leftHandSide, rightHandSide), currentTokenPrecendence);
                    }

                    return new BinaryExpression(binaryOperatorToken, leftHandSide, rightHandSide);
                }

                var currentPeek = PeekToken();
                var nextTokenPrecedence = LexerConstants.OperatorPrecedence.Get(currentPeek);

                var nextTokenIsMoreImportantThanCurrent = nextTokenPrecedence > currentTokenPrecendence;
                // The deeper the scope the better ;)
                var isStartOfNewMathScopeAndThusMoreImportantThanCurrent = availableParanthesesBeforeParsingRightHandSide.WasOpeningParanthese;
                if (nextTokenIsMoreImportantThanCurrent || isStartOfNewMathScopeAndThusMoreImportantThanCurrent)
                {
                    rightHandSide = ParseBinaryOperatorRightHandSide(rightHandSide, nextTokenPrecedence);
                    if (rightHandSide == null)
                    {
                        return null;
                    }
                }

                leftHandSide = new BinaryExpression(binaryOperatorToken, leftHandSide, rightHandSide);
            }
        }

        private FunctionCallExpression? ParseFunctionCallExpression()
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

        //what is a prototype?
        private PrototypeExpression? ParsePrototype()
        {
            if (PeekToken().TokenType != TokenType.Identifier)
            {
                // Expected a function name...
                ThrowParseError(PeekToken(), "a function name", "prototype");
                return null;
            }

            var functionToken = ConsumeToken();

            if (PeekToken().TokenType != TokenType.ParanthesesOpen)
            {
                ThrowParseError(PeekToken(), LexerConstants.PARANTHESES_OPEN, "prototype");
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
                ThrowParseError(PeekToken(), LexerConstants.PARANTHESES_CLOSE, "prototype");
                return null;
            }

            ConsumeToken();

            return new PrototypeExpression(functionToken, argumentNames.ToArray());
        }

        private ExpressionBase ParseIdentifierExpression()
        {
            //todo: make smarter so it knows whether this is a function call? Or should the lexer handle this for me, as it can also peek forward...
            //todo: how to handle nullables?
            var result = new IdentifierExpression(ConsumeToken());

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
                ThrowParseError(PeekToken(), "Character with length of 1", "Character initializer");
            }

            //todo: how to handle nullables?
            //should string value be filled?
            var result = new CharacterExpression(ConsumeToken());

            return result;
        }

        private bool ThrowParseError(TokenType expectedNextTokenType, string exptectedInOrAt)
        {
            return ThrowParseError(expectedNextTokenType, expectedNextTokenType.ToString(), exptectedInOrAt);
        }

        private bool ThrowParseError(TokenType expectedNextTokenType, string expectedCharacter, string exptectedInOrAt)
        {
            var token = PeekToken();
            if (token.TokenType != expectedNextTokenType)
            {
                Console.WriteLine($"Expected '{expectedCharacter}' in {exptectedInOrAt} at line: {token.Line}, column: {token.Column}.");
                return true;
            }

            return false;
        }

        private static void ThrowParseError(Token token, string expectedCharacter, string exptectedInOrAt)
        {
            Console.WriteLine($"Expected '{expectedCharacter}' in {exptectedInOrAt} at line: {token.Line}, column: {token.Column}.");
        }

        private Token[] PeekTokens(int amount) => _lexer.PeekTokens(amount);
        private Token PeekToken() => _lexer.PeekToken();
        private Token ConsumeToken() => _lexer.ConsumeToken();
    }
}
