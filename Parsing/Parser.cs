﻿using Lexing;
using Parsing.AbstractSyntaxTree.Expressions;
using System.Diagnostics;

namespace Parsing
{
    public interface IParser
    {
        ExpressionBase[] Parse();
    }

    public sealed class Parser : IParser
    {
        private const string DIRECT_TEXT_INPUT = "Direct text input";

        private readonly ILexer _lexer;
        private readonly string _file;
        private readonly List<ExpressionBase> _expressions;
        private TypeIndicator? _currentFunctionReturnType = null; // not THREAD SAFE! May be visited at a later date... Perhaps files should be parsed one at a time?

        public Parser(ILexer lexer) : this(lexer, DIRECT_TEXT_INPUT) { }

        public Parser(ILexer lexer, string file)
        {
            _lexer = lexer;
            _file = file;
            _expressions = new();
        }

        public ExpressionBase[] Parse()
        {
            // how to handle multiple files.. 
            // Are we gonna added them to the same queue, or are we going to return a list of queues?
            // We should make the expression visitor(s) smart enough to not care about order..
            // e.g. by looping over it once to determine whether all used functions and global variables are present in the included files..
            // this means global varialbes and public functions should be stored somewhere so we can check that...
            // does an expression need a scope..? we need to know if the used function is present in one of the imported file(s).
            // but we also need to know which variant is picked, as no duplicates are allowed.
            // should we require import "stuff" as <NAME HERE> so we know which version they mean? or should it be optional in case of ambiguity
            // once that is valid, we need to do some type checking.
            // then the last step is code gen... which we can do already.. sort of...
            while (PeekToken().TokenType is not TokenType.EndOfFile)
            {
                var expr = ParseTopLevelExpression();

                if (expr is ImportStatementExpression)
                {
                    //todo: parse imported file before continuing?
                }

                ConsumeExpression(expr);
            }

            return _expressions.ToArray();
        }

        private ExpressionBase? ParseTopLevelExpression(bool shouldThrowErrorOnMissingSemiColon = true)
        {
            var peekedTokens = PeekTokens(2);
            switch (peekedTokens[0].TokenType)
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
                case TokenType.For:
                    {
                        return ParseForLoopStatementExpression();
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
                case TokenType.Type when (peekedTokens[1].TokenType is TokenType.Identifier):
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
                        var expr = ParseExpression();
                        if (PeekToken().TokenType is not TokenType.EndOfStatement)
                        {
                            if (shouldThrowErrorOnMissingSemiColon)
                            {
                                throw ParseError(PeekToken(), LexerConstants.END_OF_STATEMENT, "after expression statement");
                            }
                        }
                        else
                        {
                            ConsumeToken();
                        }
                        return expr;
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
                _expressions.Add(expression);
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
                //TokenType.AccoladesClose => null, // end of a (sub) expression
                TokenType.ParanthesesClose => null, // e.g. end of function call..
                TokenType.ParanthesesOpen => ParseParantheseOpen(),
                TokenType.FunctionName => ParseIdentifierExpression(), // kind of a hack, but a function name is also an identifier.
                TokenType.Identifier => ParseIdentifierExpression(),
                TokenType.Value => ParseValueExpression(),
                _ => throw new InvalidOperationException($"Encountered an unkown token {currentTokenType}."),// todo: what to do here?                    
            };
        }

        private ExpressionBase? ParseValueExpression()
        {
            var peekedToken = PeekToken();
            return peekedToken.TypeIndicator switch
            {
                TypeIndicator.None => throw new InvalidOperationException("TypeIndicator 'None' should never be used with a TokenType.Value."),
                TypeIndicator.Inferred => throw new InvalidOperationException($"{nameof(ParseValueExpression)} does not support inferring types. The calling method should handle this"),
                //case TypeIndicator.UserDefined: throw new InvalidOperationException($"{nameof(ParseValueExpression)} does not support user defined types. The calling method should handle this");
                TypeIndicator.Float => ParseFloatExpression(),
                TypeIndicator.Double => ParseDoubleExpression(),
                TypeIndicator.Boolean => ParseBooleanExpression(),
                TypeIndicator.Integer => ParseIntegerExpression(),
                TypeIndicator.Character => ParseCharacterExpression(),
                TypeIndicator.String => ParseStringExpression(),
                TypeIndicator.DateTime => throw new NotImplementedException(),
                TypeIndicator.Void => throw new InvalidOperationException("Should not get here in case of void type."),
                _ => throw new InvalidOperationException($"Encountered an unkown type indicator {peekedToken.TypeIndicator}."),
            };
        }

        private ExpressionBase ParseReturnStatementExpression()
        {
            if (!_currentFunctionReturnType.HasValue)
            {
                throw ParseError(PeekToken(), "return statements are only allowed within a function");
            }

            return new ReturnExpression(ConsumeToken(), ParseTopLevelExpression(), _currentFunctionReturnType.Value);
        }

        private ExpressionBase? ParseParantheseOpen()
        {
            Debug.Assert(PeekToken().TokenType is TokenType.ParanthesesOpen);
            var paranOpenToken = ConsumeToken();

            var exp = ParseExpression();
            var peekedToken = PeekToken();
            if (peekedToken.TokenType is not TokenType.ParanthesesClose)
            {
                throw ParseError(peekedToken, $"matching closing paranthese '{LexerConstants.PARANTHESES_CLOSE}'", $"for opening paranthese at line: {paranOpenToken.Line} column: {paranOpenToken.Column}");
            }

            ConsumeToken();
            return exp;
        }

        private ExpressionBase ParseBooleanExpression()
        {
            return new BooleanExpression(ConsumeToken());
        }

        private FunctionDefinitionExpression ParseFunctionDefinitionExpression()
        {
            var isExtern = PeekToken().TokenType is TokenType.Extern;
            var isExport = PeekToken().TokenType is TokenType.Export;

            if (isExtern && isExport)
            {
                throw ParseError(PeekToken(), "Function definition can not be both extern and export at the same time");
            }

            if (isExtern || isExport)
            {
                ConsumeToken();   
            }

            if (PeekToken().TokenType is not TokenType.FunctionDefinition)
            {
                throw ParserError(PeekToken(), TokenType.FunctionDefinition, "after extern or export declaration");
            }

            ConsumeToken();
   ;
            if (PeekToken().TokenType is not TokenType.FunctionName)
            {
                throw ParserError(PeekToken(), TokenType.FunctionName, "after func definition");
            }

            var funcIdentifier = ConsumeToken();
   

            if (PeekToken().TokenType is not TokenType.ParanthesesOpen)
            {
                throw ParseError(PeekToken(), LexerConstants.PARANTHESES_OPEN, "after func identifier");
            }

            ConsumeToken();

            var parameters = new List<FunctionDefinitionArgument>();

            while (PeekToken().TokenType is not TokenType.ParanthesesClose or TokenType.EndOfFile)
            {
                var currentPeek = PeekToken();

                if (currentPeek.TokenType is TokenType.EndOfFile)
                {
                    throw ParseError(currentPeek, LexerConstants.PARANTHESES_CLOSE, "after func parameter body");
                }

                if (currentPeek.TokenType is not TokenType.ArgumentSeparator && parameters.Count >= 1)
                {
                    throw ParseError(PeekToken(), LexerConstants.ARGUMENT_SEPARATOR, "function definition");
                }

                if (currentPeek.TokenType is TokenType.ArgumentSeparator)
                {
                    ConsumeToken();
                }

                var typeIdentifier = PeekToken();
                if (typeIdentifier.TypeIndicator is TypeIndicator.Inferred)
                {
                    throw ParseError(typeIdentifier, "Inferred types are not allowed in function definitions.");
                }
                ConsumeToken();

                currentPeek = PeekToken();
                if (currentPeek.TokenType is not TokenType.Identifier)
                {
                    throw ParserError(currentPeek, TokenType.Identifier, "in func parameter body");
                }

                var variableName = ConsumeToken();

                parameters.Add(new FunctionDefinitionArgument(typeIdentifier, variableName));
            }

            var functionName = funcIdentifier.Name;
  
            if (PeekToken().TokenType is not TokenType.ParanthesesClose)
            {
                throw ParseError(PeekToken(), LexerConstants.PARANTHESES_CLOSE, "after func identifier");
            }

            ConsumeToken();

            if (PeekToken().TokenType is not TokenType.ReturnTypeIndicator)
            {
                throw ParseError(PeekToken(), LexerConstants.RETURN_TYPE_INDICATOR, "after func parameter body");
            }

            ConsumeToken(); // don't care about return type indicator

            if(PeekToken().TokenType is not TokenType.Type or TokenType.Identifier)
            {
                throw ParseError(PeekToken(), "Type", "after return type indicator");
            }

            var returnType = ConsumeToken();
            Debug.Assert(returnType is not null);
            _currentFunctionReturnType = returnType.TypeIndicator; // if none is present... assume void...

            if (isExtern)
            {
                if (PeekToken().TokenType is not TokenType.EndOfStatement)
                {
                    throw ParseError(PeekToken(), LexerConstants.END_OF_STATEMENT, $"after func type identifier in extern function definition");
                }

                ConsumeToken();

                Debug.Assert(!isExport);

                return new FunctionDefinitionExpression(functionName, funcIdentifier, parameters.ToArray(), returnType, null, true, false);
            }


            var body = ParseBodyExpression("function body");

            return new FunctionDefinitionExpression(functionName, funcIdentifier, parameters.ToArray(), returnType, body, isExtern, isExport);
        }

        private ExpressionBase? ParseDoWhileStatementExpression()
        {
            Debug.Assert(PeekToken().TokenType is TokenType.Do);
            ConsumeToken();

            var doBody = ParseBodyExpression("while statement");

            if (PeekToken().TokenType is not TokenType.While)
            {
                throw ParseError(PeekToken(), LexerConstants.Keywords.WHILE, $"'{LexerConstants.Keywords.DO}' keyword");
            }
            var whileTok = ConsumeToken();
            var condition = ParseTopLevelExpression(false);
            if (condition == null)
            {
                throw ParseError(whileTok, "expression", "after while keyword.");
            }
            return new DoWhileStatementExpression(condition, doBody);
        }

        private WhileStatementExpression ParseWhileStatementExpression()
        {
            Debug.Assert(PeekToken().TokenType is TokenType.While);
            var whileTok = ConsumeToken();
            var conditionalExpression = ParseTopLevelExpression(false);
            if (conditionalExpression == null)
            {
                throw ParseError(whileTok, "expression", "after while keyword.");
            }

            if (PeekToken().TokenType is TokenType.Do)
            {
                ConsumeToken();
            }

            //todo: handle multiple { and } signs indented inside ?
            //todo: Or should we disallow such weird scopes.
            var bodyExpr = ParseBodyExpression("after while statement body");

            return new WhileStatementExpression(conditionalExpression, bodyExpr);
        }

        private BodyExpression ParseBodyExpression(string bodyName)
        {
            if (PeekToken().TokenType is not TokenType.AccoladesOpen)
            {
                throw ParseError(PeekToken(), $"Expected opening '{ LexerConstants.ACCOLADES_OPEN}'", $"at start of {bodyName} body");
            }

            ConsumeToken();

            var body = new List<ExpressionBase> { };
            while (PeekToken().TokenType is not TokenType.AccoladesClose)
            {
                if (PeekToken().TokenType is TokenType.EndOfFile)
                {
                    throw ParseError(PeekToken(), $"Expected matching closing '{ LexerConstants.ACCOLADES_CLOSE}'", $"after {bodyName} body");
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
            return new BodyExpression(body);
        }

        private IfStatementExpression ParseIfStatementExpression()
        {
            var ifToken = ConsumeToken();
            Debug.Assert(ifToken.TokenType == TokenType.If);
            var conditionalExpression = ParseTopLevelExpression(false);
            if (conditionalExpression == null)
            {
                throw ParseError(ifToken, "Expected an expression but got none.");
            }

            //todo: handle multiple { and } signs indented inside ?
            //todo: Or should we disallow such weird scopes.
            var ifBody = ParseBodyExpression("if statement");

            if (PeekToken().TokenType is not TokenType.Else)
            {
                return new IfStatementExpression(conditionalExpression, ifBody, null);
            }

            ConsumeToken();

            var isIfStatementNext = PeekToken().TokenType is TokenType.If; // encountered else if..
            ExpressionBase? elseExpression;
            if (isIfStatementNext)
            {
                elseExpression = ParseIfStatementExpression();
            }
            else
            {
                elseExpression = ParseBodyExpression("else statement body");
            }

            return new IfStatementExpression(conditionalExpression, ifBody, elseExpression);
        }

        private VariableDeclarationExpression? ParseVariableDeclaration()
        {
            var declarationTypeToken = ConsumeToken();

            var leftHandSideTok = PeekToken();

            if (PeekToken().TokenType is not TokenType.Identifier)
            {
                throw ParserError(leftHandSideTok, TokenType.Identifier, "before assignment of variable");
            }

            var leftHandSideIdentifierExpression = ConsumeToken(); // variable name...

            if (leftHandSideIdentifierExpression == null)
            {
                throw ParseError(leftHandSideTok, "variable name", "before assignment of variable");
            }

            if (PeekToken().TokenType is not (TokenType.Assignment or TokenType.AddAssign or TokenType.SubtractAssign or TokenType.MultiplyAssign or TokenType.DivideAssign))
            {
                throw ParseError(PeekToken(), LexerConstants.ASSIGN_OPERATOR, "at assignment of variable");
            }

            var assignmentTok = ConsumeToken();
            var valueExpression = ParseTopLevelExpression();
            if (valueExpression == null)
            {
                throw ParseError(assignmentTok, "value expression", "after assignment of variable");
            }

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

        private FunctionCallExpression ParseFunctionCallExpression()
        {
            if (PeekToken().TokenType != TokenType.FunctionName)
            {
                // Expected a function name...
                throw ParseError(PeekToken(), "a function name", "function call");
            }

            var functionToken = ConsumeToken();

            if (PeekToken().TokenType != TokenType.ParanthesesOpen)
            {
                throw ParseError(PeekToken(), LexerConstants.PARANTHESES_OPEN, "function call");
            }

            ConsumeToken();

            var functionArguments = new List<ExpressionBase>();

            while (PeekToken().TokenType is not TokenType.ParanthesesClose)
            {
                if (PeekToken().TokenType is not TokenType.ArgumentSeparator && functionArguments.Count >= 1)
                {
                    throw ParseError(PeekToken(), LexerConstants.ARGUMENT_SEPARATOR, "function call");
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
                    throw ParseError(PeekToken(), $"Passed illegal expression '{PeekToken()}' to function: '{functionToken}' ");
                }

                functionArguments.Add(argumentExpression);
            }

            if (PeekToken().TokenType is not TokenType.ParanthesesClose)
            {
                throw ParseError(PeekToken(), LexerConstants.PARANTHESES_CLOSE, "function call");
            }

            ConsumeToken();
            return new FunctionCallExpression(functionToken.Name, functionArguments.ToArray());
        }

        private ExpressionBase ParseImportStatementExpression()
        {
            Debug.Assert(PeekToken().TokenType is TokenType.ImportStatement);
            var importTok = ConsumeToken();
            var expr = ParseExpression();
            if (expr == null || expr.Token is null || expr.Token.TypeIndicator is not TypeIndicator.String)
            {
                throw ParseError(importTok, "string expression representing a file path", "after import statement");
            }

            Token? alias = null;
            if (PeekToken().TokenType is TokenType.As)
            {
                ConsumeToken();

                if (PeekToken().TokenType is not TokenType.Identifier)
                {
                    throw ParserError(expr.Token, TokenType.Identifier, "after specifying 'as' when creating an alias for an import.");
                }
                alias = ConsumeToken();
            }

            if (PeekToken().TokenType is not TokenType.EndOfStatement)
            {
                throw ParseError(expr.Token, LexerConstants.END_OF_STATEMENT, "after specifying the file path of an import statement.");
            }

            if (string.IsNullOrWhiteSpace(expr.Token.ValueAsString))
            {
                throw ParseError(importTok, "Expected non empty string expression representing a file path after import statement.");
            }

            if (!File.Exists(expr.Token.ValueAsString))
            {
                throw ParseError(importTok, "File specified in import statement could not be found.");
            }

            ConsumeToken();

            return new ImportStatementExpression(expr.Token.ValueAsString, alias?.ValueAsString, _file);
        }

        private ExpressionBase? ParseForLoopStatementExpression()
        {
            Debug.Assert(PeekToken().TokenType is TokenType.For);
            ConsumeToken();

            if (PeekToken().TokenType is not TokenType.ParanthesesOpen)
            {
                throw ParseError(PeekToken(), LexerConstants.PARANTHESES_OPEN, "at start of 'for loop' header");
            }

            ConsumeToken();

            var variableDeclExp = ParseVariableDeclaration();
            if (variableDeclExp is null)
            {
                throw ParseError(PeekToken(), "variable declaration", "inside, but at the start of, the 'for loop' header");
            }

            var conditionExpr = ParseTopLevelExpression();
            if (conditionExpr is null)
            {
                throw ParseError(PeekToken(), "conditional expression", "inside, but in the center of, the 'for loop' header");
            }

            var variableIncreaseExpression = ParseTopLevelExpression(false);
            if (variableIncreaseExpression is null)
            {
                throw ParseError(PeekToken(), "variable modifier expression", "inside, but at the end of, the 'for loop' header");
            }

            if (PeekToken().TokenType is not TokenType.ParanthesesClose)
            {
                throw ParseError(PeekToken(), LexerConstants.PARANTHESES_CLOSE, "at end of 'for loop' header");
            }

            ConsumeToken();

            var forBody = ParseBodyExpression("for statement");
            if (forBody is null)
            {
                throw ParseError(PeekToken(), "body of for loop", "after the 'for loop' header");
            }

            return new ForStatementExpression(variableDeclExp, conditionExpr, variableIncreaseExpression, forBody);
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
            //todo: fix.. This should be caught sooner, e.g. at type checking...
            if (PeekToken().ValueAsString?.Length > 1)
            {
                ParseError(PeekToken(), "Character with length of 1", "Character initializer");
            }

            //todo: how to handle nullables?
            return new CharacterExpression(ConsumeToken());
        }

        private ParseException ParserError(TokenType expectedTokenType, string expectedInOrAt)
        {
            return ParseError(PeekToken(), expectedTokenType.ToString(), expectedInOrAt);
        }

        private ParseException ParserError(Token token, TokenType expectedTokenType, string expectedInOrAt)
        {
            return ParseError(token, expectedTokenType.ToString(), expectedInOrAt);
        }

        private ParseException ParseError(Token token, string expectedCharacter, string exptectedInOrAt)
        {
            return ParseError(token, $"Expected '{expectedCharacter}' {exptectedInOrAt}");
        }

        private ParseException ParserError(string message)
        {
            return ParseError(PeekToken(), message);
        }

        private ParseException ParseError(Token token, string message)
        {
            if (!message.EndsWith("!") && !message.EndsWith("."))
            {
                message += ".";
            }
            return new ParseException(token, message, _file);
        }

        private Token[] PeekTokens(int amount) => _lexer.PeekTokens(amount);
        private Token PeekToken() => _lexer.PeekToken();
        private Token[] ConsumeTokens(int amount) => _lexer.ConsumeTokens(amount);
        private Token ConsumeToken() => _lexer.ConsumeToken();
    }
}
