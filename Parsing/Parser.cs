﻿using Exceptions;
using Lexing;
using Parsing.AbstractSyntaxTree.Expressions;
using System.Diagnostics;

namespace Parsing
{
    public interface IParser
    {
        List<IScope> Parse();
    }

    public interface IScope
    {
        //todo: fixme!
        string Name { get; }
        List<NamespaceDefinitionExpression> Namespaces { get; }
        List<ClassDefinitionExpression> Classes { get; }
        List<FunctionDefinitionExpression> Functions { get; }
        List<VariableDeclarationExpression> Variables { get; }
        List<ExpressionBase> ExpressionTree { get; }
    }

    public class Scope : IScope
    {
        public Scope(string name)
        {
            Name = name;
        }

        public string Name { get; }
        public List<NamespaceDefinitionExpression> Namespaces { get; } = new();
        public List<ClassDefinitionExpression> Classes { get;  } = new();
        internal List<FunctionScope> FunctionsInternal { get;  } = new(); // outside doesnt need to know the functionScope, expression is enough..
#pragma warning disable CS8619 // Nullability of reference types in value doesn't match target type.
        public List<FunctionDefinitionExpression> Functions => FunctionsInternal.Select(x => x.Expression).ToList();
#pragma warning restore CS8619 // Nullability of reference types in value doesn't match target type.
        public List<VariableDeclarationExpression> Variables { get; } = new();
        public List<ExpressionBase> ExpressionTree { get; } = new();
    }

    internal class FunctionScope
    {
        // function scope seems to contain redundant information, but return type is useful when parsing the body to set the return type on return expressions.
        // perhaps its not the cleanest solution but its useful.
        public FunctionDefinitionExpression? Expression { get; set; }
        public Token? ReturnType { get; set; }
    }

    public sealed class Parser : IParser
    {
        private const string DIRECT_TEXT_INPUT = "Direct input";

        private readonly ILexer _lexer;
        private readonly string _file;
        private readonly List<Scope> _scopes;

        public Parser(ILexer lexer) : this(lexer, DIRECT_TEXT_INPUT) { }

        public Parser(ILexer lexer, string file)
        {
            _lexer = lexer;
            _file = file;
            _scopes = new();
        }

        public List<IScope> Parse()
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
            _scopes.Add(new Scope(_file));
            while (PeekToken().TokenType is not TokenType.EndOfFile)
            {
                var expr = ParseEntryLevelExpression();

                if (expr is ImportStatementExpression importExpr)
                {
                    // code below is ugly but works for now.. ~BvL, 16/11/2021
                    var text = File.ReadAllText(importExpr.Path);
                    var lexer = new Lexer(text);
                    var parser = new Parser(lexer, Path.GetFileNameWithoutExtension(importExpr.Path));
                    _scopes.AddRange(parser.Parse().Cast<Scope>());// ugly...
                }

                ConsumeExpression(expr);
            }

            return _scopes.Cast<IScope>().ToList();
        }

        // top level expressions should not reference a value or function call, this is handled by the default case.
        private ExpressionBase? ParseEntryLevelExpression()
        {
            switch (PeekToken().TokenType)
            {
                case TokenType.Namespace:
                    {
                        return ParseNameSpaceExpression();
                    }
                default:
                    {
                        return ParseTopLevelExpression();
                    }
            }
        }
        private ExpressionBase? ParseTopLevelExpression()
        {
            var peekedTokens = PeekTokens(2);
            switch (peekedTokens[0].TokenType)
            {
                case TokenType.EndOfFile:
                    {
                        return null;
                    }
#if DEBUG
                case TokenType.EndOfStatement:
                    {
                        throw new InvalidOperationException($"Dear developer, did you mean to call '{nameof(ParseExpression)}' or '{nameof(ParseExpressionResultingInValue)}'?");
                    }
#endif
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
                case TokenType.Export when (peekedTokens[1].TokenType is TokenType.FunctionDefinition):
                case TokenType.FunctionDefinition:
                    {
                        return ParseFunctionDefinitionExpression();
                    }
                case TokenType.Type when (peekedTokens[1].TokenType is TokenType.VariableIdentifier):
                    {
                        return ParseVariableDeclaration();
                    }
                case TokenType.Export when (peekedTokens[1].TokenType is TokenType.Class):
                case TokenType.Class:
                    {
                        return ParseClassExpression();
                    }
                case TokenType.ImportStatement:
                    {
                        return ParseImportStatementExpression();
                    }
                case TokenType.Return:
                    {
                        return ParseReturnStatementExpression();
                    }
                default:
                    {
                        return ParseExpression();
                    }
            }

        }

        private ClassDefinitionExpression ParseClassExpression()
        {
            Debug.Assert(PeekToken().TokenType is TokenType.Class or TokenType.Export);

            var isExport = PeekToken().TokenType is TokenType.Export;

            if (isExport)
            {
                ConsumeToken();
            }

            if (PeekToken().TokenType is not TokenType.Class)
            {
                throw ParseError(PeekToken(), TokenType.Class, "after export declaration on scope level of namespace.");
            }

            ConsumeToken();

            if (PeekToken().TokenType is not TokenType.VariableIdentifier)
            {
                throw ParseError(PeekToken(), "identifier", "at start of class declaration.");
            }

            var classIdentifierToken = ConsumeToken();

            var classBody = ParseBodyExpression(classIdentifierToken, "class token");

            var variableDeclarations = classBody.Body.Where(x => x.DISCRIMINATOR is ExpressionType.VariableDeclaration).Cast<VariableDeclarationExpression>().ToArray();
            var functionDefinitions = classBody.Body.Where(x => x.DISCRIMINATOR is ExpressionType.FunctionDefinition).Cast<FunctionDefinitionExpression>().ToArray();
            var classDefinitions = classBody.Body.Where(x => x.DISCRIMINATOR is ExpressionType.Class).Cast<ClassDefinitionExpression>().ToArray();

            var expectedTotalLength = variableDeclarations.Length + functionDefinitions.Length + classDefinitions.Length;
            if (classBody.Body.Length != expectedTotalLength)
            {
                throw ParseError(classIdentifierToken, "variable, function or class definitions", "after class definition.");
            }

            var expr = new ClassDefinitionExpression(classIdentifierToken, variableDeclarations, functionDefinitions, classDefinitions);
            _scopes.Last().Classes.Add(expr);

            return expr;
        }

        private NamespaceDefinitionExpression ParseNameSpaceExpression()
        {
            Debug.Assert(PeekToken().TokenType is TokenType.Namespace);
            ConsumeToken();

            if (PeekToken().TokenType is not TokenType.VariableIdentifier)
            {
                throw ParseError(PeekToken(), "identifier", "at start of namespace declaration.");
            }
            var namespaceIdentifier = ConsumeToken();

            var namespaceBody = ParseNamespaceBodyExpression(namespaceIdentifier, "namespace declaration");

            if (namespaceBody.Body.Any(x => x.DISCRIMINATOR is not (ExpressionType.Class or ExpressionType.Namespace)))
            {
                throw ParseError(namespaceIdentifier, "only classes and namespaces", "after namespace delcaration.");
            }

            var namespaces = namespaceBody?.Body?.Where(x => x.DISCRIMINATOR == ExpressionType.Namespace).Cast<NamespaceDefinitionExpression>()?.ToArray();
            var classes = namespaceBody?.Body?.Where(x => x.DISCRIMINATOR == ExpressionType.Class).Cast<ClassDefinitionExpression>()?.ToArray();
            var namespaceExp = new NamespaceDefinitionExpression(namespaceIdentifier, namespaces, classes); //todo: support dots in namespace.
            _scopes.Last().Namespaces.Add(namespaceExp);
            return namespaceExp;
        }

        private BodyExpression ParseNamespaceBodyExpression(Token parentToken, string bodyName)
        {
            if (PeekToken().TokenType is not TokenType.AccoladesOpen)
            {
                throw ParseError(PeekToken(), $"Expected opening '{ LexerConstants.ACCOLADES_OPEN}'", $"at start of {bodyName} body");
            }

            ConsumeToken();

            var body = new List<ExpressionBase>();
            while (PeekToken().TokenType is not TokenType.AccoladesClose)
            {
                if (PeekToken().TokenType is TokenType.EndOfFile)
                {
                    throw ParseError(PeekToken(), $"Expected matching closing '{ LexerConstants.ACCOLADES_CLOSE}'", $"after {bodyName} body");
                }

                var expression = ParseEntryLevelExpression(); // different between parse body and this is what level of expressions are allowed.
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
            return new BodyExpression(parentToken, body);
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
                _scopes.Last().ExpressionTree.Add(expression);
            }
        }

        private ValueExpressionBase? ParseExpressionResultingInValue()
        {
            var peekedTokens = PeekTokens(2);
            var currentTokenType = peekedTokens[0].TokenType;

            return currentTokenType switch
            {
                TokenType.EndOfStatement => null,
                TokenType.EndOfFile => null,
                TokenType.ParanthesesClose => null, // e.g. end of function call..
                TokenType.FunctionIdentifier => ParseFunctionCallExpression(), // kind of a hack, but a function name is also an identifier.
                TokenType.VariableIdentifier when (peekedTokens[1].TokenType is TokenType.Dot) => ParseMemberAccessExpression(),
                TokenType.VariableIdentifier => ParseIdentifierExpression(),
                TokenType.Value => ParseValueExpression(),
                TokenType.Add => ParseValueExpression(), // e.g. var x = +1;
                TokenType.Subtract => ParseNegativeValueExpression(), // e.g. var x = -1;
                TokenType.ParanthesesOpen => ParseParantheseOpen(),
                _ => throw new InvalidOperationException($"Encountered an unkown token {currentTokenType}."),// todo: what to do here?                    
            };
        }

        private ValueExpressionBase? ParseValueExpression()
        {
            var peekedToken = PeekToken();
            return peekedToken.TypeIndicator switch
            {
                TypeIndicator.None => throw new InvalidOperationException("TypeIndicator 'None' should never be used with a TokenType.Value."),
                TypeIndicator.Inferred => throw new InvalidOperationException($"{nameof(ParseValueExpression)} does not support inferring types. The calling method should handle this"),
                TypeIndicator.Float => ParseValueExpressionInternal(),
                TypeIndicator.Double => ParseValueExpressionInternal(),
                TypeIndicator.Boolean => ParseValueExpressionInternal(),
                TypeIndicator.Integer => ParseValueExpressionInternal(),
                TypeIndicator.Character => ParseValueExpressionInternal(),
                TypeIndicator.String => ParseValueExpressionInternal(),
                TypeIndicator.DateTime => throw new NotImplementedException(),
                TypeIndicator.Void => throw new InvalidOperationException("Should not get here in case of void type."),
                _ => throw new InvalidOperationException($"Encountered an unkown type indicator {peekedToken.TypeIndicator}."),
            };
        }

        private ValueExpressionBase ParseMemberAccessExpression()
        {
            Debug.Assert(PeekToken().TokenType is TokenType.VariableIdentifier);

            var parent = ConsumeToken();
            Debug.Assert(PeekToken().TokenType is TokenType.Dot, $"{nameof(ParseExpressionResultingInValue)}, should check whether a dot is present! Else its just a normal variable, not an member access...");
            var dot = ConsumeToken();
            if (PeekToken().TokenType is not TokenType.VariableIdentifier)
            {
                throw ParseError(PeekToken(), TokenType.VariableIdentifier, "after a dot indicating a member access expression.");
            }

            var memberAccess = ConsumeToken();

            return new MemberAccessExpression(parent, memberAccess);
        }
        private ExpressionBase ParseReturnStatementExpression()
        {
            try
            {
                var returnType = _scopes.Last().FunctionsInternal.Last().ReturnType;
                Debug.Assert(returnType is not null);
                var returnTypeIndicator = returnType.TypeIndicator;
                return new ReturnExpression(ConsumeToken(), ParseExpression(), returnTypeIndicator);
            }
            catch
            {
                throw ParseError(PeekToken(), "return statements are only allowed within a function.");
            }

        }

        private ValueExpressionBase? ParseParantheseOpen()
        {
            Debug.Assert(PeekToken().TokenType is TokenType.ParanthesesOpen);
            var paranOpenToken = ConsumeToken();

            var exp = ParseExpression(false);
            if (exp == null)
            {
                throw ParseError(paranOpenToken, "Parantheses should contain an expression");
            }
            if (exp is not ValueExpressionBase vExp)
            {
                throw ParseError(exp.Token, "expression between parentheses should evaluate to a result");
            }

            var peekedToken = PeekToken();
            if (peekedToken.TokenType is not TokenType.ParanthesesClose)
            {
                throw ParseError(peekedToken, $"matching closing paranthese '{LexerConstants.PARANTHESES_CLOSE}'", $"for opening paranthese at line: {paranOpenToken.Line} column: {paranOpenToken.Column}");
            }

            ConsumeToken();

            return vExp;
        }

        private ValueExpressionBase ParseNegativeValueExpression()
        {
            var subtractTok = ConsumeToken();
            var expr = ParseExpressionResultingInValue();
            if (expr == null)
            {
                throw ParseError(subtractTok, "an expression after a subtract '-' symbol.");
            }

            expr.IsNegative = true;

            return expr;
        }

        private ValueExpressionBase ParseValueExpressionInternal()
        {
            var tok = ConsumeToken();
            return new ValueExpression(tok, tok);
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
                throw ParseError(PeekToken(), TokenType.FunctionDefinition, "after extern or export declaration");
            }

            ConsumeToken();

            if (PeekToken().TokenType is not TokenType.FunctionIdentifier)
            {
                throw ParseError(PeekToken(), TokenType.FunctionIdentifier, "after func definition");
            }

            var funcIdentifier = ConsumeToken();

            if (PeekToken().TokenType is not TokenType.ParanthesesOpen)
            {
                throw ParseError(PeekToken(), LexerConstants.PARANTHESES_OPEN, "after func identifier");
            }

            ConsumeToken();

            var parameters = new List<FunctionDefinitionArgument>();

            while (PeekToken().TokenType is not (TokenType.ParanthesesClose or TokenType.EndOfFile))
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
                    throw ParseError(typeIdentifier, $"Type identifiers that indicate an inferred value (e.g. {LexerConstants.Keywords.VARIABLE_TYPE_INFERRED_1}, {LexerConstants.Keywords.VARIABLE_TYPE_INFERRED_1}) are not allowed in a function definition header/prototype.");
                }
                ConsumeToken();

                currentPeek = PeekToken();
                if (currentPeek.TokenType is not TokenType.VariableIdentifier)
                {
                    throw ParseError(currentPeek, TokenType.VariableIdentifier, "in func parameter body");
                }

                var variableToken = ConsumeToken();
                // kind of ugly, but makes it cleaner for the rest of the program...
                // type indicators are nothing more than an enum value anyway... (at least, for base types.. this is not the case for user defined types.)
                variableToken.TypeIndicator = typeIdentifier.TypeIndicator;
                parameters.Add(new FunctionDefinitionArgument(typeIdentifier, variableToken));
            }

            if (!isExtern)
            {
                funcIdentifier.Name = CreateMangledName(funcIdentifier.Name, parameters.Select(a => a.TypeToken));
            }

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

            if (PeekToken().TokenType is not (TokenType.Type or TokenType.VariableIdentifier))
            {
                throw ParseError(PeekToken(), "Type or Identifier", $"after return type indicator '{LexerConstants.RETURN_TYPE_INDICATOR}'");
            }

            var returnType = ConsumeToken();
            _scopes.Last().FunctionsInternal.Add(new FunctionScope() { ReturnType = returnType }); // add return info before parsing body...

            if (isExtern)
            {
                if (PeekToken().TokenType is not TokenType.EndOfStatement)
                {
                    throw ParseError(PeekToken(), LexerConstants.END_OF_STATEMENT, $"after func type identifier in extern function definition");
                }

                ConsumeToken();

                Debug.Assert(!isExport);
                var externExpr = new FunctionDefinitionExpression(funcIdentifier, parameters.ToArray(), returnType, null, true, false);
                _scopes.Last().FunctionsInternal.Last().Expression = externExpr;
                return externExpr;
            }

            var body = ParseBodyExpression(funcIdentifier, "function body");
            var expr = new FunctionDefinitionExpression(funcIdentifier, parameters.ToArray(), returnType, body, isExtern, isExport); ;
            _scopes.Last().FunctionsInternal.Last().Expression = expr;
            return expr;
        }

        private static string CreateMangledName(string baseName, IEnumerable<Token> typeTokens)
        {
            //todo: code below doesnt really work for user defined types as the name can have the same starting value....?
            var name = baseName;

            foreach (var token in typeTokens)
            {
                var typeName = token.TokenType is TokenType.VariableIdentifier ? token.Name : ConvertTypeIndicatorToString(token.TypeIndicator);
                name += $"<{typeName}>";
            }

            return name;
        }

        private static string ConvertTypeIndicatorToString(TypeIndicator typeIndicator)
        {
            return typeIndicator switch
            {
                TypeIndicator.Float => "float",
                TypeIndicator.Double => "double",
                TypeIndicator.Boolean => "bool",
                TypeIndicator.Integer => "int",
                TypeIndicator.Character => "char",
                TypeIndicator.String => "string",
                TypeIndicator.Inferred => throw new NotImplementedException(),
                TypeIndicator.DateTime => throw new NotImplementedException(),
                TypeIndicator.Void => throw new NotImplementedException(),
                TypeIndicator.None => throw new NotImplementedException(),
                _ => throw new NotImplementedException($"Exhaustive use of {nameof(ConvertTypeIndicatorToString)}."),
            };
        }

        private ExpressionBase? ParseDoWhileStatementExpression()
        {
            Debug.Assert(PeekToken().TokenType is TokenType.Do);
            var doToken = ConsumeToken();

            var doBody = ParseBodyExpression(doToken, "while statement");

            if (PeekToken().TokenType is not TokenType.While)
            {
                throw ParseError(PeekToken(), LexerConstants.Keywords.WHILE, $"'{LexerConstants.Keywords.DO}' keyword");
            }
            var whileTok = ConsumeToken();
            var condition = ParseExpression(false);
            if (condition == null)
            {
                throw ParseError(whileTok, "expression", "after while keyword.");
            }
            return new DoWhileStatementExpression(whileTok, condition, doBody);
        }

        private WhileStatementExpression ParseWhileStatementExpression()
        {
            Debug.Assert(PeekToken().TokenType is TokenType.While);
            var whileTok = ConsumeToken();
            var conditionalExpression = ParseExpression(false);
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
            var bodyExpr = ParseBodyExpression(whileTok, "after while statement body");

            return new WhileStatementExpression(whileTok, conditionalExpression, bodyExpr);
        }

        private BodyExpression ParseBodyExpression(Token parentToken, string bodyName)
        {
            if (PeekToken().TokenType is not TokenType.AccoladesOpen)
            {
                throw ParseError(PeekToken(), $"Expected opening '{ LexerConstants.ACCOLADES_OPEN}'", $"at start of {bodyName} body");
            }

            ConsumeToken();

            var body = new List<ExpressionBase>();
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
            return new BodyExpression(parentToken, body);
        }

        private IfStatementExpression ParseIfStatementExpression()
        {
            var ifToken = ConsumeToken();
            Debug.Assert(ifToken.TokenType == TokenType.If);
            var conditionalExpression = ParseExpression(false);
            if (conditionalExpression == null)
            {
                throw ParseError(ifToken, "Expected an expression but got none.");
            }

            var ifBody = ParseBodyExpression(ifToken, "if statement");

            if (PeekToken().TokenType is not TokenType.Else)
            {
                return new IfStatementExpression(ifToken, conditionalExpression, ifBody, null);
            }

            var elseTok = ConsumeToken();

            var isIfStatementNext = PeekToken().TokenType is TokenType.If; // encountered else if..
            ExpressionBase? elseExpression;
            if (isIfStatementNext)
            {
                elseExpression = ParseIfStatementExpression();
            }
            else
            {
                elseExpression = ParseBodyExpression(elseTok, "else statement body");
            }

            return new IfStatementExpression(ifToken, conditionalExpression, ifBody, elseExpression);
        }

        private VariableDeclarationExpression? ParseVariableDeclaration()
        {
            var declarationTypeToken = ConsumeToken();

            var leftHandSideTok = PeekToken();

            if (PeekToken().TokenType is not TokenType.VariableIdentifier)
            {
                throw ParseError(leftHandSideTok, TokenType.VariableIdentifier, "before assignment of variable");
            }

            var leftHandSideVariableIdentifierExpression = ConsumeToken();

            if (PeekToken().TokenType is not (TokenType.Assignment or TokenType.AddAssign or TokenType.SubtractAssign or TokenType.MultiplyAssign or TokenType.DivideAssign))
            {
                throw ParseError(PeekToken(), LexerConstants.ASSIGN_OPERATOR, "at assignment of variable");
            }

            var assignmentTok = ConsumeToken();
            var valueExpression = ParseExpression();
            if (valueExpression == null) // this is not the case? it's actiually usual optional.... for now keep it an error tho...
            {
                throw ParseError(assignmentTok, "value expression", "after assignment of variable");
            }
            return new VariableDeclarationExpression(declarationTypeToken, leftHandSideVariableIdentifierExpression, assignmentTok, valueExpression);
        }

        private ValueExpressionBase? ParseExpression(bool throwErrorOnMissingSemicolon = true)
        {
            var expr = ParseExpressionInternal();

            if (PeekToken().TokenType is not TokenType.EndOfStatement)
            {
                if (throwErrorOnMissingSemicolon)
                {
                    throw ParseError(PeekToken(), LexerConstants.END_OF_STATEMENT, "after expression statement");
                }
            }
            else
            {
                ConsumeToken(); // consume semicolon
            }
            return expr;
        }

        private ValueExpressionBase? ParseExpressionInternal(int minTokenPrecedence = LexerConstants.OperatorPrecedence.DEFAULT_OPERATOR_PRECEDENCE)
        {
            var lhs = ParseExpressionResultingInValue();

            if (lhs == null)
            {
                return null;
            }

            while (true)
            {
                // Peek instead of consume to determine precedence
                var operatorMetadata = LexerConstants.GetOperatorMetadata(PeekToken());
                var isBinaryOperator = operatorMetadata != null;
                if (!isBinaryOperator)
                {
                    break;
                }
                Debug.Assert(operatorMetadata is not null);

                var binaryOperatorPrecedence = operatorMetadata.Precedence;

                if (binaryOperatorPrecedence < minTokenPrecedence)
                {
                    break;
                }

                var operatorToken = ConsumeToken();

                var nextMinOperatorPrecedence = operatorMetadata.IsLeftAssociated ? binaryOperatorPrecedence + 1 : binaryOperatorPrecedence;

                var rhs = ParseExpressionInternal(nextMinOperatorPrecedence);

                if (operatorMetadata.IsCompoundAssignment)
                {
                    var tokCopy = new Token(operatorToken) { TokenType = operatorMetadata.DecompoundedToken };
                    operatorToken.TokenType = TokenType.Assignment;

                    rhs = new BinaryExpression(tokCopy, lhs, rhs);
                }

                lhs = new BinaryExpression(operatorToken, lhs, rhs);
            }

            return lhs;
        }

        private FunctionCallExpression ParseFunctionCallExpression()
        {
            if (PeekToken().TokenType != TokenType.FunctionIdentifier)
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

                var argumentExpression = ParseExpression(false);

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
            return new FunctionCallExpression(functionToken, functionArguments.ToArray());
        }

        private ExpressionBase ParseImportStatementExpression()
        {
            Debug.Assert(PeekToken().TokenType is TokenType.ImportStatement);
            var importTok = ConsumeToken();

            if (PeekToken().TokenType is not TokenType.Value || PeekToken().TypeIndicator is not TypeIndicator.String)
            {
                throw ParseError(importTok, "string expression representing a file path", "after import statement");
            }

            var expr = ParseValueExpression();
            if (expr == null || expr.Token is null || expr.Token.TypeIndicator is not TypeIndicator.String)
            {
                throw ParseError(importTok, "string expression representing a file path", "after import statement");
            }

            Token? alias = null;
            if (PeekToken().TokenType is TokenType.As)
            {
                ConsumeToken();

                if (PeekToken().TokenType is not TokenType.VariableIdentifier)
                {
                    throw ParseError(expr.Token, TokenType.VariableIdentifier, "after specifying 'as' when creating an alias for an import.");
                }
                alias = ConsumeToken();
            }

            if (PeekToken().TokenType is not TokenType.EndOfStatement)
            {
                throw ParseError(expr.Token, LexerConstants.END_OF_STATEMENT, "after specifying the file path of an import statement.");
            }

            if (string.IsNullOrWhiteSpace(expr.Token.ValueAsString))
            {
                throw ParseError(expr.Token, "Expected non empty string expression representing a file path after import statement.");
            }

            if (_file == Path.GetFileNameWithoutExtension(expr.Token.ValueAsString))
            {
                throw ParseError(expr.Token, "File '{expr.Token.ValueAsString}' imports itself, which shouldn't be done!.");
            }

            if (!File.Exists(expr.Token.ValueAsString))
            {
                throw ParseError(expr.Token, $"File specified in import statement '{expr.Token.ValueAsString}' could not be found.");
            }

            ConsumeToken();

            return new ImportStatementExpression(importTok, expr.Token.ValueAsString, alias?.ValueAsString, _file);
        }

        private ExpressionBase ParseForLoopStatementExpression()
        {
            Debug.Assert(PeekToken().TokenType is TokenType.For);
            var forToken = ConsumeToken();

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

            var conditionExpr = ParseExpression();
            if (conditionExpr is null)
            {
                throw ParseError(PeekToken(), "conditional expression", "inside, but in the center of, the 'for loop' header");
            }

            var variableIncreaseExpression = ParseExpression(false);
            if (variableIncreaseExpression is null)
            {
                throw ParseError(PeekToken(), "variable modifier expression", "inside, but at the end of, the 'for loop' header");
            }

            if (PeekToken().TokenType is not TokenType.ParanthesesClose)
            {
                throw ParseError(PeekToken(), LexerConstants.PARANTHESES_CLOSE, "at end of 'for loop' header");
            }

            ConsumeToken();

            var forBody = ParseBodyExpression(forToken, "for statement");
            if (forBody is null)
            {
                throw ParseError(PeekToken(), "body of for loop", "after the 'for loop' header");
            }

            return new ForStatementExpression(forToken, variableDeclExp, conditionExpr, variableIncreaseExpression, forBody);
        }

        private ValueExpressionBase ParseIdentifierExpression()
        {
            return new IdentifierExpression(ConsumeToken());
        }

        private SyntaxErrorException ParseError(TokenType expectedTokenType, string expectedInOrAt)
        {
            return ParseError(PeekToken(), expectedTokenType.ToString(), expectedInOrAt);
        }

        private SyntaxErrorException ParseError(Token token, TokenType expectedTokenType, string expectedInOrAt)
        {
            return ParseError(token, expectedTokenType.ToString(), expectedInOrAt);
        }

        private SyntaxErrorException ParseError(Token token, string expectedCharacter, string exptectedInOrAt)
        {
            return ParseError(token, $"Expected '{expectedCharacter}' {exptectedInOrAt}");
        }

        private SyntaxErrorException ParseError(string message)
        {
            return ParseError(PeekToken(), message);
        }

        private SyntaxErrorException ParseError(Token token, string message)
        {
            if (!message.EndsWith("!") && !message.EndsWith("."))
            {
                message += ".";
            }
            return new SyntaxErrorException(token.Line, token.Column, message, _file);
        }

        private Token[] PeekTokens(int amount) => _lexer.PeekTokens(amount);
        private Token PeekToken() => _lexer.PeekToken();
        private Token[] ConsumeTokens(int amount) => _lexer.ConsumeTokens(amount);
        private Token ConsumeToken() => _lexer.ConsumeToken();
    }
}
