﻿using Lexing;
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
                TokenType.Identifier => ParseIdentifierExpression(),
                TokenType.Double => ParseDoubleExpression(),
                TokenType.Float => ParseFloatExpression(),
                TokenType.Integer => ParseIntegerExpression(),
                TokenType.String => ParseStringExpression(),
                TokenType.Character => ParseCharacterExpression(),
                _ => throw new InvalidOperationException($"Encountered an unkown token {currentTokenType}."),// todo: what to do here?                    
            };
        }

        private ExpressionBase? ParseAssignmentExpression(bool isReassignment)
        {
            // todo: Check if the value thats being assigned a new value actually Exists in scope?
            var declarationTypeToken = isReassignment ? new Token() { TokenType = TokenType.ReAssignment } : ConsumeToken();

            var leftHandSideTok = PeekToken();

            if (PeekToken().TokenType != TokenType.Identifier)
            {
                ThrowParseError(PeekToken(), "identifier", "assignment of variable");
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
            var temp = ConsumeParantheses();

            var leftHandSide = ParsePrimary();
            const int DEFAULT_OPERATOR_PRECENDENCE = 0;
            return leftHandSide == null
                ? null
                : ParseBinaryOperatorRightHandSide(leftHandSide, DEFAULT_OPERATOR_PRECENDENCE);
        }

        private (bool WasOpeningParanthese, bool WasClosingParanthese) ConsumeParantheses()
        {
            var currentPeek = PeekToken();
            var isOpen = currentPeek.TokenType is TokenType.ParanthesesOpen;
            var isClose = currentPeek.TokenType is TokenType.ParanthesesClose;
            if (isOpen) //todo: I don't think we need to know about all the starting layers... Could be wrong..
            {
                while (PeekToken().TokenType is TokenType.ParanthesesOpen)
                {
                    ConsumeToken();
                }
            }

            if (isClose) // Get rid of ending scopes.
            {
                while (PeekToken().TokenType is TokenType.ParanthesesClose)
                {
                    ConsumeToken();
                }
            }

            if (isOpen && PeekToken().TokenType is TokenType.ParanthesesClose)
            {
                ThrowParseError(PeekToken(), "a statement instead of a closing paranthese", "binary operation.");
                //Todo: Should we consume this token and check further..?
            }

            return (isOpen, isClose);
        }

        private ExpressionBase? ParseBinaryOperatorRightHandSide(ExpressionBase leftHandSide, int previousExpressionPrecedence)
        {
            while (true)
            {

                // Peek instead of consume to determine precedence
                var currentTokenPrecendence = LexerConstants.OperatorPrecedence.Get(PeekToken());

                if (currentTokenPrecendence < previousExpressionPrecedence )
                {
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
                    var nextTokPrec = LexerConstants.OperatorPrecedence.Get(PeekToken());
                    if (nextTokPrec > currentTokenPrecendence)
                    {
                        return ParseBinaryOperatorRightHandSide(new BinaryExpression(binaryOperatorToken, leftHandSide, rightHandSide), nextTokPrec);

                    }
                    else
                    {
                        return new BinaryExpression(binaryOperatorToken, leftHandSide, rightHandSide);
                    }

                }


                var currentPeek = PeekToken();

                var nextTokenPrecedence = LexerConstants.OperatorPrecedence.Get(currentPeek);

                var nextTokenIsMoreImportantThanCurrent = nextTokenPrecedence > currentTokenPrecendence;
                // The deeper the scope the better ;)
                var isStartOfNewMathScopeAndThusMoreImportantThanCurrent = availableParanthesesBeforeParsingRightHandSide.WasOpeningParanthese;
                if (nextTokenIsMoreImportantThanCurrent || isStartOfNewMathScopeAndThusMoreImportantThanCurrent)
                {
                    int prevTokPrecToUse = currentTokenPrecendence - 1;
                    if (nextTokenPrecedence >= currentTokenPrecendence)
                    {
                        prevTokPrecToUse = nextTokenPrecedence;
                    }
                    rightHandSide = ParseBinaryOperatorRightHandSide(rightHandSide, prevTokPrecToUse);
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

        private static void ThrowParseError(Token token, string expectedCharacter, string exptectedInOrAt)
        {
            Console.WriteLine($"Expected '{expectedCharacter}' in {exptectedInOrAt} at line: {token.Line}, column: {token.Column}.");
        }

        private Token[] PeekTokens(int amount) => _lexer.PeekTokens(amount);
        private Token PeekToken() => _lexer.PeekToken();
        private Token ConsumeToken() => _lexer.ConsumeToken();
    }
}
