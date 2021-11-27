using Lexing;
using Parsing.AbstractSyntaxTree.Expressions;
using Parsing.AbstractSyntaxTree.Visitors;
using System.Diagnostics;

namespace Compiling.Backends
{
    internal class TypeChecker : IAbstractSyntaxTreeVisitor
    {
        private class TypeCheckValue
        {
            public TypeCheckValue(Token valueToken, Token typeToken)
            {
                ValueToken = valueToken;
                TypeToken = typeToken;
            }
            public Token ValueToken { get; }
            public Token TypeToken { get; }
        }

        //todo: refactor entire compiler so this stuff is passed to the relevant places.
        // things like functions, userdefined types, public constants etc, should be found and registered before all other checks are done, to be able to call a function before its defined.
        private readonly Dictionary<string, FunctionDefinitionExpression> _functions = new();
        private readonly Dictionary<string, TypeCheckValue> _namedValues = new();
        private readonly Dictionary<string, Token> _userDefinedTypes = new();
        private readonly Stack<TypeCheckValue> _valueStack = new();
        private IReadOnlyDictionary<string, IScope> _scopes = new Dictionary<string, IScope>();

        public string Name => "Type checker";

        public void Visit(ExpressionBase? expression) => AbstractSyntaxTreeVisitor.Visit(this, expression);

        public void Initialize(IReadOnlyDictionary<string, IScope> scopes)
        {
            _scopes = scopes;
        }

        public void VisitBinaryExpression(BinaryExpression expression)
        {
            Visit(expression.LeftHandSide);
            var lhsValue = _valueStack.Pop();
            Visit(expression.RightHandSide);
            _valueStack.TryPop(out var rhsValue);

            if (rhsValue?.ValueToken == null)
            {
                return; // input = output
            }

            switch (expression.Token.TokenType)
            {
                case TokenType.Add:
                case TokenType.Subtract:
                case TokenType.Multiply:
                case TokenType.Modulo:
                case TokenType.Divide:
                case TokenType.GreaterThan:
                case TokenType.GreaterThanEqual:
                case TokenType.LessThan:
                case TokenType.LessThanEqual:
                    {
                        FixNumericalTypesAndValuesIfRequired(lhsValue, rhsValue);
                        if (lhsValue.TypeToken.TypeIndicator != rhsValue.TypeToken.TypeIndicator)
                        {
                            throw new Exception("rhs and lhs value are not of the same type.");// todo improve message.
                        }

                        break;
                    }
                case TokenType.Assignment:
                    {
                        //todo: refactor and infer type before calling this method...
                        FixNumericalTypesAndValuesIfRequired(lhsValue, rhsValue);
                        if (lhsValue.TypeToken.TypeIndicator != rhsValue.TypeToken.TypeIndicator)
                        {
                            throw new Exception("rhs and lhs value are not of the same type.");// todo improve message.
                        }
                        break;
                    }
                case TokenType.Equivalent: //todo: actually make this do a type compare? 
                case TokenType.NotEquivalent: //todo: actually make this do a type compare? 
                    {
                        //todo: refactor and infer type before calling this method...
                        FixNumericalTypesAndValuesIfRequired(lhsValue, rhsValue);
                        if (lhsValue.TypeToken.TypeIndicator != rhsValue.TypeToken.TypeIndicator)
                        {
                            throw new Exception("rhs and lhs value are not of the same type.");// todo improve message.
                        }

                        break;
                    }
                case TokenType.Equals:
                case TokenType.NotEquals:
                    {
                        //todo: refactor and infer type before calling this method...
                        FixNumericalTypesAndValuesIfRequired(lhsValue, rhsValue);
                        if (lhsValue.TypeToken.TypeIndicator != rhsValue.TypeToken.TypeIndicator)
                        {
                            throw new Exception("rhs and lhs value are not of the same type.");// todo improve message.
                        }

                        break;
                    }
                case TokenType.BitShiftLeft:
                case TokenType.BitShiftRight:
                case TokenType.BitwiseOr:
                case TokenType.BitwiseAnd:
                    {
                        if (lhsValue.TypeToken.TypeIndicator != TypeIndicator.Integer)
                        {
                            throw new Exception($"lhs value of {expression.Token.TokenType} must evaluate to an {TypeIndicator.Integer}.");// todo improve message.
                        }

                        if (rhsValue.TypeToken.TypeIndicator != TypeIndicator.Integer)
                        {
                            throw new Exception($"rhs value of{expression.Token.TokenType} must evaluate to an {TypeIndicator.Integer}.");// todo improve message.
                        }

                        break;
                    }
                case TokenType.ConditionalOr:
                case TokenType.ConditionalXOr:
                case TokenType.ConditionalAnd:
                    {

                        if (lhsValue.TypeToken.TypeIndicator != TypeIndicator.Boolean)
                        {
                            throw new Exception($"lhs value of {expression.Token.TokenType} must evaluate to a {TypeIndicator.Boolean}.");// todo improve message.

                        }

                        if (rhsValue.TypeToken.TypeIndicator != TypeIndicator.Boolean)
                        {
                            throw new Exception($"rhs value of{expression.Token.TokenType} must evaluate to a {TypeIndicator.Boolean}.");// todo improve message.
                        }

                        break;
                    }

                default:
                    throw new ArgumentException($"invalid binary operator {expression.Token.TokenType}");
            }

            _valueStack.Push(lhsValue); // lhsValue should be sufficient for type checking as lhs and rhs sides of binary operations should be equal now..
        }

        public void VisitBodyExpression(BodyExpression expression)
        {
            foreach (var expr in expression.Body)
            {
                Visit(expr);
            }
        }

        public void VisitWhileStatementExpression(WhileStatementExpression expression)
        {
            Visit(expression.Condition);
            Visit(expression.Body);
        }

        public void VisitDoWhileStatementExpression(DoWhileStatementExpression expression)
        {
            Visit(expression.Body);
            Visit(expression.Condition);
        }

        public void VisitForStatementExpression(ForStatementExpression expression)
        {
            Visit(expression.VariableDeclaration);
            Visit(expression.Condition);
            Visit(expression.VariableIncreaseExpression);
            Visit(expression.Body);
        }

        public void VisitFunctionCallExpression(FunctionCallExpression expression)
        {
            var arguments = new List<TypeCheckValue>();
            foreach (var arg in expression.Arguments)
            {
                Visit(arg);
                arguments.Add(_valueStack.Pop());
            }
            //todo: refactor compiler so function names are already mangled and all user defined types, constants, etc are known before type checking.
            // the logic can be copied over.. i think...
            //todo add logic for userdefined types.
            //todo: functions imported from a library can't currently be mangled, fix? people should not be limited to function names...
            // or the stdlib.bs should be its seperate module..?
            expression.FunctionName = CreateMangledName(expression.FunctionName, arguments.Select(a => a.TypeToken));

            var hasFunc = _functions.TryGetValue(expression.FunctionName, out var funcExpr);
            if (!hasFunc)
            {
                var argString = string.Join(", ", arguments.Select(x => $"{x.TypeToken.Value} {x.ValueToken.Name}"));
                throw new Exception($"Function '{expression.FunctionName}' with argument types: ({argString}) does not exist.");// todo: improve message and make custom exception.
            }

            Debug.Assert(funcExpr != null); // stupid assert as hasFunc is checked above
            expression.TypeToken = funcExpr.ReturnTypeToken; // ugly?                                            
            _valueStack.Push(new TypeCheckValue(expression.Token, funcExpr.ReturnTypeToken));
        }

        public void VisitFunctionDefinitionExpression(FunctionDefinitionExpression expression)
        {
            //todo: make sure the arguments will be known before visting the body....
            foreach (var arg in expression.Arguments)
            {
                var val = new TypeCheckValue(arg.ValueToken, arg.TypeToken);
                _namedValues.Add(arg.ValueToken.Name, val);
            }

            Visit(expression.Body);

            foreach (var arg in expression.Arguments)
            {
                _namedValues.Remove(arg.ValueToken.Name);
            }
            //todo: refactor compiler so function names are already mangled and all user defined types, constants, etc are known before type checking.
            //expression.FunctionName = CreateMangledName(expression.FunctionName, expression.Arguments.Select(a => a.TypeToken));
            _functions.Add(expression.FunctionName, expression);
        }

        public void VisitIdentifierExpression(IdentifierExpression expression)
        {
            Debug.Assert(expression?.Token is not null);
            if (!_namedValues.TryGetValue(expression.Identifier, out var res))
            {
                throw new ArgumentException($"Unknown variable name {expression.Identifier}");
            }

            _valueStack.Push(res);
        }

        public void VisitIfStatementExpression(IfStatementExpression expression)
        {
            Visit(expression.IfCondition);
            Visit(expression.IfBody);
            Visit(expression.ElseBody);
        }

        public void VisitReturnExpression(ReturnExpression expression)
        {
            Visit(expression.ReturnExpr);
            var hasReturnExpressionResultToken = _valueStack.TryPop(out var returnExprStackValue);

            if (expression.FunctionReturnTypeIndicator is TypeIndicator.Void)
            {
                if (hasReturnExpressionResultToken)
                {
                    throw new Exception("void return func can't return a value"); // todo create custom exception for type error..
                }

                return;
            }

            Debug.Assert(returnExprStackValue is not null);
            Debug.Assert(expression.Token is not null);
            expression.Token.TypeIndicator = expression.FunctionReturnTypeIndicator;
            var lhsValue = new TypeCheckValue(expression.Token, expression.Token);
            FixNumericalTypesAndValuesIfRequired(lhsValue, returnExprStackValue);
            if (expression.FunctionReturnTypeIndicator != returnExprStackValue.TypeToken.TypeIndicator)
            {
                throw new Exception($"Function with return type '{expression.FunctionReturnTypeIndicator}' can't return a value of type '{returnExprStackValue.TypeToken.TypeIndicator}'");
            }

            return;
        }

        private static void FixNumericalTypesAndValuesIfRequired(TypeCheckValue lhs, TypeCheckValue rhs)// todo:rename
        {
            var lhsType = lhs.TypeToken;
            var rhsType = rhs.TypeToken;
            //todo: refactor and infer type before calling this method...
            var isTypeInferred = lhsType.TypeIndicator is TypeIndicator.Inferred || rhsType.TypeIndicator is TypeIndicator.Inferred;
            if (lhsType.TypeIndicator == rhsType.TypeIndicator)
            {
                if (isTypeInferred) // not sure if this is actually valid... get rid of it if required -BvL 13-11-2021.
                {
                    throw new Exception("Unable to infer type, both lhs and rhs are type inferred.");
                }

                return;
            }

            var supportedTypesOrdered = new (TypeIndicator TypeIndicator, Type TypeInfo)[] { (TypeIndicator.Double, typeof(double)), (TypeIndicator.Float, typeof(float)), (TypeIndicator.Integer, typeof(long)) };

            if (!isTypeInferred && !supportedTypesOrdered.Any(t => t.TypeIndicator == lhsType.TypeIndicator || t.TypeIndicator == rhsType.TypeIndicator))
            {
                return; // can only fix these types, so if lhs and rhs are not both of the above typeIndicator then return.
            }

            foreach (var typeData in supportedTypesOrdered)// don't filter list as its ordered..
            {
                if (lhsType.TypeIndicator == typeData.TypeIndicator || rhsType.TypeIndicator == typeData.TypeIndicator)
                {
                    lhsType.TypeIndicator = typeData.TypeIndicator;
                    rhsType.TypeIndicator = typeData.TypeIndicator;
                    lhs.ValueToken.TypeIndicator = typeData.TypeIndicator;
                    rhs.ValueToken.TypeIndicator = typeData.TypeIndicator;

                    if (lhs.TypeToken.TokenType is TokenType.Value)
                    {
                        lhs.ValueToken.Value = Convert.ChangeType(lhs.ValueToken.Value, typeData.TypeInfo);
                    }

                    if (rhs.TypeToken.TokenType is TokenType.Value)
                    {
                        rhs.ValueToken.Value = Convert.ChangeType(rhs.ValueToken.Value, typeData.TypeInfo);
                    }

                    return;
                }
            }
        }

        public void VisitVariableDeclarationExpression(VariableDeclarationExpression expression)
        {
            if (_namedValues.ContainsKey(expression.Identifier))
            {
                throw new ArgumentException($"Redeclaration of {expression.Identifier}! Scopes are not yet supported! Don't re-use variable names!");
            }


            //var rhsValue = _valueStack.Pop();

            var lhsValue = new TypeCheckValue(expression.IdentifierToken, expression.DeclarationTypeToken);
            var rhsValue = new TypeCheckValue(expression.ValueExpression.TypeToken, expression.ValueExpression.TypeToken);
            FixNumericalTypesAndValuesIfRequired(lhsValue, rhsValue);
            Visit(expression.ValueExpression);

            // kind of hacky, but visiting the expression above can cause the lhs and rhs to change...
            // we abuse this for function calls as visiting a function call will expose its return type...
            lhsValue = new TypeCheckValue(expression.IdentifierToken, expression.DeclarationTypeToken);
            rhsValue = new TypeCheckValue(expression.ValueExpression.TypeToken, expression.ValueExpression.TypeToken);
            FixNumericalTypesAndValuesIfRequired(lhsValue, rhsValue);

            if (lhsValue.TypeToken.TypeIndicator != rhsValue.TypeToken.TypeIndicator)
            {
                throw new Exception("rhs and lhs value are not of the same type.");// todo improve message.
            }


            Debug.Assert(expression.Identifier is not null);

            _namedValues.Add(expression.Identifier, rhsValue);
        }

        public void VisitValueExpression(ValueExpression expression)
        {
            Debug.Assert(expression?.Token is not null);
            if (expression.Token.TypeIndicator is TypeIndicator.Character && expression.Token?.ValueAsString?.Length > 1)
            {
                throw new Exception($"Character can only have a length of one but was {expression.Token?.ValueAsString?.Length}"); // todo: Custom exception type.
            }
            Debug.Assert(expression?.Token is not null);
            _valueStack.Push(new TypeCheckValue(expression.ValueToken, expression.TypeToken));
        }
        public void VisitNamespaceExpression(NamespaceDefinitionExpression expression) { }
        public void VisitClassExpression(ClassDefinitionExpression expression) { }
        public void VisitImportExpression(ImportStatementExpression expression) { }

        //todo: cleanup! DUPLICATE CODE: A COPY exists in the crawler!!
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
    }
}
