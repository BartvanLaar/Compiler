using Parser.AbstractSyntaxTree.Expressions;
using Parser.CodeLexer;
using System.Diagnostics;

namespace Parser
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
                        if (peekedTokens[1].TokenType is (TokenType.Add or TokenType.Subtract or TokenType.Multiply or TokenType.Divide))
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
            switch (currentTokenType)
            {
                case TokenType.EndOfStatement: return null;
                case TokenType.EndOfFile: return null;
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

            if (PeekToken().TokenType != TokenType.Identifier)
            {
                LogParseError(PeekToken(), "identifier", "assignment of variable");
                return null;
            }

            var leftHandSideIdentifierExpression = ParseIdentifierExpression(); // variable name...

            if (leftHandSideIdentifierExpression == null)
            {
                LogParseError(leftHandSideTok, "variable name", "assignment of variable");
                return null;
            }

            if (PeekToken().TokenType is not (TokenType.Assignment or TokenType.AddAssign or TokenType.SubtractAssign or TokenType.MultiplyAssign or TokenType.DivideAssign))
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

            return new AssignmentExpression(declarationTypeToken, leftHandSideIdentifierExpression, assignmentTok, valueExpression);
        }

        private ExpressionBase? ParseExpression()
        {
            var leftHandSide = ParsePrimary();
            const int DEFAULT_OPERATOR_PRECENDENCE = 0;
            return leftHandSide == null
                ? null
                : ParseBinaryOperatorRightHandSide(leftHandSide, DEFAULT_OPERATOR_PRECENDENCE);
        }

        private ExpressionBase? ParseBinaryOperatorRightHandSide(ExpressionBase leftHandSide, int previousExpressionPrecedence)
        {
            while (true)
            {
               
                // Peek instead of consume to determine precedence
                var currentTokenPrecendence = LexerConstants.OperatorPrecedence.Get(PeekToken());

                if (currentTokenPrecendence < previousExpressionPrecedence)
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

        private Token[] PeekTokens(int amount) => _lexer.PeekTokens(amount);
        private Token PeekToken() => _lexer.PeekToken();
        private Token ConsumeToken() => _lexer.ConsumeToken();
    }
}
