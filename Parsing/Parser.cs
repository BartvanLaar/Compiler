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
                case TokenType.If: ConsumeIfStatementExpression(); return false;
                case TokenType.Do:
                    ConsumeDoWhileExpression(); return false;
                case TokenType.While:
                    ConsumeWhileStatementExpression(); return false;

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

        private void ConsumeDoWhileExpression()
        {
            var expr = ParseDoWhileStatementExpression();
            ConsumeExpression(expr);
        }

        private void ConsumeWhileStatementExpression()
        {
            var expr = ParseWhileStatementExpression();
            ConsumeExpression(expr);
        }


        private void ConsumeIfStatementExpression()
        {
            var expr = ParseIfStatementExpression();
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
                //TokenType.ParanthesesClose => null, // end of a sub expression
                TokenType.ParanthesesOpen => ParseParanthese(),
                TokenType.Identifier => ParseIdentifierExpression(),
                TokenType.Double => ParseDoubleExpression(),
                TokenType.Float => ParseFloatExpression(),
                TokenType.Integer => ParseIntegerExpression(),
                TokenType.String => ParseStringExpression(),
                TokenType.Character => ParseCharacterExpression(),
                TokenType.True => ParseBooleanExpression(),
                TokenType.False => ParseBooleanExpression(),
                _ => throw new InvalidOperationException($"Encountered an unkown token {currentTokenType}."),// todo: what to do here?                    
            };
        }

        private ExpressionBase? ParseParanthese()
        {
            Debug.Assert(PeekToken().TokenType is TokenType.ParanthesesOpen);
            ConsumeToken();

            var exp = ParseExpression();
            if (PeekToken().TokenType is not TokenType.ParanthesesClose)
            {
                ThrowParseError(PeekToken(), $"Expected matching closing paranthese '{LexerConstants.PARANTHESES_CLOSE}'");
                return null;
            }

            ConsumeToken();
            return exp;
        }

        private ExpressionBase ParseBooleanExpression()
        {
            if (PeekToken().TokenType is (TokenType.True or TokenType.False))
            {
                return new BooleanExpression(ConsumeToken());
            }

            throw new InvalidOperationException($"{nameof(ParseBooleanExpression)} can only be used with Tokens True or False, in all other cases use a binary expression");
        }

        private ExpressionBase? ParseDoWhileStatementExpression()
        {
            Debug.Assert(PeekToken().TokenType is TokenType.Do);
            ConsumeToken();

            if (PeekToken().TokenType is not TokenType.AccoladesOpen)
            {
                ThrowParseError(PeekToken(), LexerConstants.ACCOLADES_OPEN, $"'{LexerConstants.KeyWords.DO}' keyword");
                return null;
            }

            ConsumeToken();

            //todo: handle multiple { and } signs indented inside ?
            //todo: Or should we disallow such weird scopes.
            var body = new List<ExpressionBase> { };
            while (PeekToken().TokenType is not TokenType.AccoladesClose)
            {
                var expression = ParseExpression();
                Debug.Assert(expression != null);
                body.Add(expression);
            }

            Debug.Assert(PeekToken().TokenType is TokenType.AccoladesClose);
            ConsumeToken();

            if (PeekToken().TokenType is not TokenType.While)
            {
                ThrowParseError(PeekToken(), LexerConstants.KeyWords.WHILE, $"'{LexerConstants.KeyWords.DO}' keyword");
                return null;
            }
            var prevTok = ConsumeToken();
            var condition = ParseExpression();
            if (condition == null)
            {
                ThrowParseError("Expected an expression", prevTok);
                return null;
            }
            return new WhileStatementExpression(condition, new BodyExpression(body));
        }

        private ExpressionBase ParseWhileStatementExpression()
        {
            Debug.Assert(PeekToken().TokenType is TokenType.While);
            var whileTok = ConsumeToken();
            var conditionalExpression = ParseExpression();
            if (conditionalExpression == null)
            {
                ThrowParseError("Expected an expression", whileTok);
                return null;
            }

            var hasDo = PeekToken().TokenType is TokenType.Do;
            if (PeekToken().TokenType is TokenType.Do)
            {
                ConsumeToken();
            }

            if (PeekToken().TokenType is not TokenType.AccoladesOpen)
            {
                var keyword = hasDo ? LexerConstants.KeyWords.DO : LexerConstants.KeyWords.WHILE;
                ThrowParseError(PeekToken(), LexerConstants.ACCOLADES_OPEN, $"'{keyword}' keyword");
                return null;
            }
            ConsumeToken();

            //todo: handle multiple { and } signs indented inside ?
            //todo: Or should we disallow such weird scopes.
            var body = new List<ExpressionBase> { };
            while (PeekToken().TokenType is not TokenType.AccoladesClose)
            {
                var expression = ParseExpression();
                Debug.Assert(expression != null);
                body.Add(expression);
            }

            Debug.Assert(PeekToken().TokenType is TokenType.AccoladesClose);
            ConsumeToken();

            return new WhileStatementExpression(conditionalExpression, new BodyExpression(body));
        }

        private ExpressionBase? ParseIfStatementExpression()
        {
            //todo: @fix, a lot of these debug.asserts should actually be normal checks and throw parser errors accordingly...
            var ifToken = ConsumeToken();
            Debug.Assert(ifToken.TokenType == TokenType.If);
            var conditionalExpression = ParseExpression();
            if (conditionalExpression == null)
            {
                ThrowParseError("Expected an expression", ifToken);
                return null;
            }

            if (PeekToken().TokenType is not TokenType.AccoladesOpen)
            {
                ThrowParseError(PeekToken(), LexerConstants.ACCOLADES_OPEN, $"'{LexerConstants.KeyWords.DO}' keyword");
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

            Debug.Assert(PeekToken().TokenType is TokenType.Else);
            ConsumeToken();

            var isIfStatementNext = PeekToken().TokenType is TokenType.If;
            ExpressionBase? elseExpression;
            if (isIfStatementNext)
            {
                elseExpression = ParseIfStatementExpression();

            }
            else
            {
                Debug.Assert(PeekToken().TokenType == TokenType.AccoladesOpen);
                ConsumeToken();
                elseExpression = ParseExpression();

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

            if (PeekToken().TokenType is not TokenType.Identifier)
            {
                ThrowParseError(PeekToken(), "assignment of variable");
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

            var assignmentTok = ConsumeToken();
            var valueExpression = ParseExpression();
            if (valueExpression == null)
            {
                ThrowParseError(assignmentTok, "value expression", "assignment of variable");
                return null;
            }

            return new AssignmentExpression(declarationTypeToken, leftHandSideIdentifierExpression, assignmentTok, valueExpression);
        }

        private ExpressionBase? ParseExpression(int minTokenPrecedence = LexerConstants.OperatorPrecedence.DEFAULT_OPERATOR_PRECEDENCE)
        {
            var lhs = ParsePrimary();

            if (lhs == null)
            {
                return null;
            }

            while (true)
            {
                // Peek instead of consume to determine precedence
                var isBinaryOperator = LexerConstants.OperatorPrecedence.Get(PeekToken(), out var binaryOperatorPrecedence);
                if (!isBinaryOperator || binaryOperatorPrecedence < minTokenPrecedence)
                {
                    break;
                }

                Debug.Assert(isBinaryOperator);
                var operatorToken = ConsumeToken();

                var nextMinOperatorPrecedence = LexerConstants.OperatorPrecedence.IsLeftAssociated(operatorToken) ? binaryOperatorPrecedence + 1 : binaryOperatorPrecedence;

                var rhs = ParseExpression(nextMinOperatorPrecedence);

                lhs = new BinaryExpression(operatorToken, lhs, rhs);
            }

            return lhs;
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

        private static void ThrowParseError(Token token, string expectedInOrAt)
        {
            ThrowParseError(token, token.ToString(), expectedInOrAt);
        }

        private static void ThrowParseError(Token token, string expectedCharacter, string exptectedInOrAt)
        {
            ThrowParseError($"Expected '{expectedCharacter}' in {exptectedInOrAt}", token);
        }

        private static void ThrowParseError(string message, Token token)
        {
            Console.WriteLine($"{message}, at line: {token.Line}, column: {token.Column}.");
        }

        private Token[] PeekTokens(int amount) => _lexer.PeekTokens(amount);
        private Token PeekToken() => _lexer.PeekToken();
        private Token ConsumeToken() => _lexer.ConsumeToken();
    }
}
