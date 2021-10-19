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
            while (PeekToken().TokenType is not TokenType.EndOfFile)
            {
                var expr = ParseTopLevelExpression();

                if (expr is ImportStatementExpression)
                {
                    //todo: parse imported file before continuing?
                }

                ConsumeExpression(expr);
            }

            return _expressions;
        }

        private ExpressionBase? ParseTopLevelExpression()
        {
            var peekedTokens = PeekTokens(2);
            Debug.Assert(peekedTokens.Length == 2);

            switch (PeekToken().TokenType)
            {
                case TokenType.EndOfFile:
                    {
                        return null;
                    }
                case TokenType.EndOfStatement:
                    {
                        // consumed by ConsumeExpression in main Parse loop.
                        return null;
                    }
                case TokenType.If:
                    {
                        return ParseIfStatementExpression();
                    }
                case TokenType.Do:
                    {
                        return ParseDoWhileStatementExpression();
                    }
                case TokenType.While:
                    {
                        return ParseWhileStatementExpression();
                    }
                case TokenType.Extern:
                case TokenType.Export:
                case TokenType.FunctionDefinition:
                    {
                        return ParseFunctionDefinitionExpression();
                    }
                // if we get here, it apparently wasn't a function definition, so it should be a function call.
                case TokenType.FunctionName:
                    {
                        return ParseFunctionCallExpression();
                    }
                case TokenType.VariableDeclaration:
                    {
                        return ParseVariableDeclaration();
                    }
                case TokenType.ImportStatement:
                    {
                        return ParseImportStatementExpression();
                    }
                case TokenType.ReturnStatement:
                    {
                        return ParseReturnStatementExpression();
                    }
                default:
                    {
                        return ParseExpression();
                    }
            }
        }

        //private void ConsumeIdentifierExpression()
        //{
        //    // @todo: @Cleanup: This whole method feels like a massive hack.
        //    var peekedTokens = PeekTokens(2);
        //    //todo: handle assignment variants?
        //    if (peekedTokens[1].TokenType is TokenType.Add or TokenType.Subtract or TokenType.Multiply or TokenType.Divide)
        //    {
        //        ConsumeExpression(ParseExpression());
        //        return;
        //    }

        //    //todo: should this handle ++ and --? but what if those are before this token?..
        //    //@CleanUp, have seen a similar list in lexer constants..?
        //    if (peekedTokens[1].TokenType is (TokenType.Assignment or TokenType.AddAssign or TokenType.SubtractAssign or TokenType.MultiplyAssign or TokenType.DivideAssign))
        //    {
        //        ConsumeExpression(ParseAssignmentExpression(true));
        //        return;
        //    }

        //    even more of a hack!! Fix@@
        //    ConsumeExpression(ParseIdentifierExpression());
        //}

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

        private ExpressionBase? ParsePrimary()
        {
            var peekedTokens = PeekTokens(2);
            var currentTokenType = peekedTokens[0].TokenType;

            return currentTokenType switch
            {
                TokenType.EndOfStatement => null,
                TokenType.EndOfFile => null,
                TokenType.AccoladesClose => null, // end of a (sub) expression
                TokenType.ParanthesesClose => null, // e.g. end of function call..
                TokenType.ParanthesesOpen => ParseParantheseOpen(),
                TokenType.FunctionName => ParseIdentifierExpression(), // kind of a hack, but a function name is also an identifier.
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

        private ExpressionBase ParseReturnStatementExpression()
        {
            Debug.Assert(PeekToken().TokenType is TokenType.ReturnStatement);
            var expr = new ReturnExpression(ConsumeToken(), ParseExpression());
            return expr;
        }

        private ExpressionBase? ParseParantheseOpen()
        {
            Debug.Assert(PeekToken().TokenType is TokenType.ParanthesesOpen);
            var paranOpenToken = ConsumeToken();

            var exp = ParseExpression();
            var peekedToken = PeekToken();
            if (peekedToken.TokenType is not TokenType.ParanthesesClose)
            {
                ThrowParseError($"Expected matching closing paranthese '{LexerConstants.PARANTHESES_CLOSE}' for opening paranthese at line: {paranOpenToken.Line} column: {paranOpenToken.Column}", peekedToken);
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

        private ExpressionBase? ParseFunctionDefinitionExpression()
        {
            var peekedToken = PeekToken();
            var isExtern = peekedToken.TokenType is TokenType.Extern;
            var isExport = peekedToken.TokenType is TokenType.Export;

            if (isExtern && isExport)
            {
                ThrowParseError("Function definition can not be both extern and export at the same time", peekedToken);
                return null;
            }

            if (isExtern || isExport)
            {
                ConsumeToken();
                peekedToken = PeekToken();
            }

            if (peekedToken.TokenType is not TokenType.FunctionDefinition)
            {
                ThrowParseError(peekedToken, TokenType.FunctionDefinition, "after extern or export declaration");
                return null;
            }

            ConsumeToken();
            peekedToken = PeekToken();
            if (peekedToken.TokenType is not TokenType.FunctionName)
            {
                ThrowParseError(peekedToken, TokenType.FunctionName, "after func definition");
                return null;
            }

            var funcIdentifier = ConsumeToken();
            peekedToken = PeekToken();

            if (peekedToken.TokenType is not TokenType.ParanthesesOpen)
            {
                ThrowParseError(peekedToken, LexerConstants.PARANTHESES_OPEN, "after func identifier");
                return null;
            }
            ConsumeToken();

            var parameters = new List<FunctionDefinitionArgument>();

            while (PeekToken().TokenType is not TokenType.ParanthesesClose)
            {
                var currentPeek = PeekToken();

                if (currentPeek.TokenType is TokenType.EndOfFile)
                {
                    ThrowParseError(currentPeek, LexerConstants.PARANTHESES_CLOSE, "after func parameter body");
                    return null;
                }

                if (currentPeek.TokenType is not TokenType.ArgumentSeparator && parameters.Count() >= 1)
                {
                    ThrowParseError(PeekToken(), LexerConstants.ARGUMENT_SEPARATOR, "function definition");
                    return null;
                }

                if (currentPeek.TokenType is TokenType.ArgumentSeparator)
                {
                    ConsumeToken();
                }

                var typeIdentifier = ConsumeToken();

                currentPeek = PeekToken();
                if (currentPeek.TokenType is not TokenType.Identifier)
                {
                    ThrowParseError(currentPeek, TokenType.Identifier, "in func parameter body");
                    return null;
                }

                var variableName = ConsumeToken();

                parameters.Add(new FunctionDefinitionArgument(typeIdentifier, variableName));
            }

            //todo: check resulting parameters if any... They either all should have a token, or none should. e.g. difference between function call and function definition.
            // However, we can not assume that it's a function call based on this, as functions are allowed to have zero parameters.
            peekedToken = PeekToken();
            if (peekedToken.TokenType is not TokenType.ParanthesesClose)
            {
                ThrowParseError(peekedToken, LexerConstants.PARANTHESES_CLOSE, "after func identifier");
                return null;
            }
            ConsumeToken();
            peekedToken = PeekToken();
            if (peekedToken.TokenType is not TokenType.ReturnTypeIndicator)
            {
                ThrowParseError(peekedToken, LexerConstants.RETURN_TYPE_INDICATOR, "after func parameter body");
            }

            ConsumeToken(); // don't care about return type indicator

            var returnType = ConsumeToken();
            //todo: check return type??
            peekedToken = PeekToken();

            if (isExtern)
            {
                if (peekedToken.TokenType is not TokenType.EndOfStatement)
                {
                    ThrowParseError(peekedToken, LexerConstants.END_OF_STATEMENT, $"after func type identifier in extern function definition");
                    return null;
                }
                ConsumeToken();

                Debug.Assert(isExtern);
                Debug.Assert(!isExport);

                return new FunctionDefinitionExpression(funcIdentifier, parameters.ToArray(), returnType, null, true, false);
            }

            if (peekedToken.TokenType is not TokenType.AccoladesOpen)
            {
                ThrowParseError(peekedToken, LexerConstants.ACCOLADES_OPEN, $"after func type identifier");
                return null;
            }

            //Consume accolade...
            ConsumeToken();
            peekedToken = PeekToken();

            //todo: handle multiple { and } signs indented inside ?
            //todo: Or should we disallow such weird scopes.
            var body = new List<ExpressionBase> { };
            while (PeekToken().TokenType is not TokenType.AccoladesClose)
            {
                if (PeekToken().TokenType is TokenType.EndOfFile)
                {
                    ThrowParseError(PeekToken(), LexerConstants.ACCOLADES_CLOSE, "after func body");
                    return null;
                }
               
                if(PeekToken().TokenType is TokenType.EndOfStatement)
                {
                    ConsumeToken();
                    continue;
                }

                var expression = ParseTopLevelExpression();
                if(expression == null)
                {
                    ConsumeToken();
                }
                else
                {
                    body.Add(expression);
                }
            }
            return new FunctionDefinitionExpression(funcIdentifier, parameters.ToArray(), returnType, new BodyExpression(body), isExtern, isExport);
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
                if (PeekToken().TokenType is TokenType.EndOfFile)
                {
                    ThrowParseError(PeekToken(), LexerConstants.ACCOLADES_CLOSE, "after while statement body");
                    return null;
                }

                var expression = ParseTopLevelExpression();
                if (expression == null)
                {
                    ConsumeToken();
                }
                else
                {
                    body.Add(expression);
                }
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

        private ExpressionBase? ParseWhileStatementExpression()
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
                if (PeekToken().TokenType is TokenType.EndOfFile)
                {
                    ThrowParseError(PeekToken(), LexerConstants.ACCOLADES_CLOSE, "after while statement body");
                    return null;
                }

                var expression = ParseTopLevelExpression();
                if (expression == null)
                {
                    ConsumeToken();
                }
                else
                {
                    body.Add(expression);
                }
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
                ThrowParseError(PeekToken(), LexerConstants.ACCOLADES_OPEN, $"'{LexerConstants.KeyWords.IF}' keyword");
                return null;
            }

            ConsumeToken();

            //todo: handle multiple { and } signs indented inside ?
            //todo: Or should we disallow such weird scopes.
            var body = new List<ExpressionBase> { };
            while (PeekToken().TokenType != TokenType.AccoladesClose)
            {
                if (PeekToken().TokenType is TokenType.EndOfFile)
                {
                    ThrowParseError(PeekToken(), LexerConstants.ACCOLADES_CLOSE, "after if statement body");
                    return null;
                }

                var expression = ParseTopLevelExpression();
                if (expression == null)
                {
                    ConsumeToken();
                }
                else
                {
                    body.Add(expression);
                }
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

        private ExpressionBase? ParseVariableDeclaration()
        {
            // todo: Check if the value thats being assigned a new value actually Exists in scope?
            var declarationTypeToken = ConsumeToken();

            var leftHandSideTok = PeekToken();

            if (leftHandSideTok.TokenType is not TokenType.Identifier)
            {
                ThrowParseError(leftHandSideTok, TokenType.Identifier, "assignment of variable");
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

            //todo: fix and rename..
            return new VariableDeclarationExpression(declarationTypeToken, leftHandSideIdentifierExpression, assignmentTok, valueExpression);
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
            if (PeekToken().TokenType != TokenType.FunctionName)
            {
                // Expected a function name...
                ThrowParseError(PeekToken(), "a function name", "function call");
                return null;
            }

            var functionToken = ConsumeToken();

            if (PeekToken().TokenType != TokenType.ParanthesesOpen)
            {
                ThrowParseError(PeekToken(), LexerConstants.PARANTHESES_OPEN, "function call");
                return null;
            }
            ConsumeToken();

            var functionArguments = new List<ExpressionBase>();

            while (PeekToken().TokenType is not TokenType.ParanthesesClose)
            {
                if (PeekToken().TokenType is not TokenType.ArgumentSeparator && functionArguments.Count >= 1)
                {
                    ThrowParseError(PeekToken(), LexerConstants.ARGUMENT_SEPARATOR, "function call");
                    return null;
                }

                if (PeekToken().TokenType is TokenType.ArgumentSeparator)
                {
                    ConsumeToken();
                }

                if (PeekToken().TokenType is TokenType.FunctionName)
                {
                    var funcCallExpr = ParseFunctionCallExpression();
                    Debug.Assert(funcCallExpr != null, "func call expression can't be null when the previous token is a function name!");
                    functionArguments.Add(funcCallExpr);
                    continue;
                }
                var argumentExpression = ParseExpression();

                if (argumentExpression == null)
                {
                    ThrowParseError($"Passed illegal expression '{PeekToken()}' to function: '{functionToken}' ", PeekToken());
                    return null;
                }

                functionArguments.Add(argumentExpression);
            }

            if (PeekToken().TokenType is not TokenType.ParanthesesClose)
            {
                ThrowParseError(PeekToken(), LexerConstants.PARANTHESES_CLOSE, "function call");
                return null;
            }

            ConsumeToken();

            return new FunctionCallExpression(functionToken, functionArguments.ToArray());
        }

        private ExpressionBase? ParseImportStatementExpression()
        {
            Debug.Assert(PeekToken().TokenType is TokenType.ImportStatement);
            var importTok = ConsumeToken();
            var expr = ParseExpression();
            if (expr == null || expr.Token.HasValue == false || expr.Token.Value.TokenType is not TokenType.String)
            {
                ThrowParseError("Expected string expression representing a file path after import statement", importTok);
                return null;
            }

            if (PeekToken().TokenType is not TokenType.EndOfStatement)
            {
                ThrowParseError("Expected ; after specifying the file path of an import statement", expr.Token.Value);
                return null;
            }

            if (string.IsNullOrWhiteSpace(expr.Token.Value.StringValue))
            {
                ThrowParseError("Expected non empty string expression representing a file path after import statement", importTok);
                return null;
            }
            if (!File.Exists(expr.Token.Value.StringValue))
            {
                ThrowParseError("File specified in import statement could not be found", importTok);
                return null;
            }


            ConsumeToken();

            return new ImportStatementExpression(expr.Token.Value);
        }

        private ExpressionBase? ParseIdentifierExpression()
        {
            return new IdentifierExpression(ConsumeToken());

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
            return new DoubleExpression(ConsumeToken());
        }

        private ExpressionBase ParseIntegerExpression()
        {
            //todo: how to handle nullables?
            return new IntegerExpression(ConsumeToken());
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

        private static void ThrowParseError(Token token, TokenType expectedTokenType, string expectedInOrAt)
        {
            ThrowParseError(token, expectedTokenType.ToString(), expectedInOrAt);
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
        private Token[] ConsumeTokens(int amount) => _lexer.ConsumeTokens(amount);
        private Token ConsumeToken() => _lexer.ConsumeToken();
    }
}
