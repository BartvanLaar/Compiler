using Lexing;
using Parsing;
using Parsing.AbstractSyntaxTree.Expressions;
using Parsing.AbstractSyntaxTree.Visitors;
using System.Diagnostics;

namespace Compiling.Backends
{
    internal class DotNetCodeInterpreter : IAbstractSyntaxTreeVisitor
    {
        private readonly Stack<(object? Value, Type? TypeInfo)> _valueStack = new();
        private readonly Dictionary<string, object?> _namedValues = new();
        private readonly Dictionary<string, ExpressionBase> _functions = new();
        private List<IScope> _scopes = new ();

        public IEnumerable<object?> Results => _valueStack.Select(x => x.Value);
        public string Name => "Dot Net Interpreter / simulator";
        public void Visit(ExpressionBase? expression) => AbstractSyntaxTreeVisitor.Visit(this, expression);

        public void VisitBinaryExpression(BinaryExpression expression)
        {
            Visit(expression.LeftHandSide);
            var lhs = _valueStack.Pop();
            Debug.Assert(lhs.Value != null);
            Debug.Assert(lhs.TypeInfo != null);
            dynamic lhsValue = Convert.ChangeType(lhs.Value, lhs.TypeInfo);
            lhsValue = expression.LeftHandSide.IsNegative ? -lhsValue : lhsValue;
            Visit(expression.RightHandSide);
            var rhs = _valueStack.Pop();
            Debug.Assert(rhs.Value != null);
            Debug.Assert(rhs.TypeInfo != null);
            dynamic rhsValue = Convert.ChangeType(rhs.Value, rhs.TypeInfo);
            rhsValue = expression.RightHandSide?.IsNegative == true ? -rhsValue : rhsValue;

            switch (expression.Token.TokenType)
            {
                case TokenType.Add:
                    {
                        _valueStack.Push((lhsValue + rhsValue, lhs.TypeInfo));
                        break;
                    }
                case TokenType.Subtract:
                    {
                        _valueStack.Push((lhsValue - rhsValue, lhs.TypeInfo));
                        break;
                    }
                case TokenType.Multiply:
                    {
                        _valueStack.Push((lhsValue * rhsValue, lhs.TypeInfo));
                        break;
                    }
                case TokenType.Divide:
                    {
                        _valueStack.Push((lhsValue / rhsValue, lhs.TypeInfo));
                        break;
                    }
                case TokenType.Assignment:
                    {
                        var expr = expression.LeftHandSide as IdentifierExpression;
                        Debug.Assert(expr is not null);
                        Debug.Assert(_namedValues.ContainsKey(expr.Identifier));
                        _namedValues[expr.Identifier] = lhsValue;
                        break;
                    }
                case TokenType.Equivalent: //todo: actually make this do a type compare? 
                    {
                        _valueStack.Push((lhsValue == rhsValue, typeof(bool)));
                        break;
                    }
                case TokenType.Equals:
                    {
                        _valueStack.Push((lhsValue == rhsValue, typeof(bool)));
                        break;
                    }
                case TokenType.NotEquivalent: //todo: actually make this do a type compare? 
                    {
                        _valueStack.Push(((lhsValue != rhsValue, typeof(bool))));
                        break;
                    }
                case TokenType.NotEquals:
                    {
                        _valueStack.Push((lhsValue != rhsValue, typeof(bool)));
                        break;
                    }
                case TokenType.GreaterThan:
                    {
                        _valueStack.Push((lhsValue > rhsValue, typeof(bool)));
                        break;
                    }
                case TokenType.GreaterThanEqual:
                    {
                        _valueStack.Push((lhsValue >= rhsValue, typeof(bool)));
                        break;
                    }
                case TokenType.LessThan:
                    {
                        _valueStack.Push((lhsValue < rhsValue, typeof(bool)));
                        break;
                    }
                case TokenType.LessThanEqual:
                    {
                        _valueStack.Push((lhsValue <= rhsValue, typeof(bool)));
                        break;
                    }
                case TokenType.BitwiseOr:
                    {
                        _valueStack.Push((lhsValue | rhsValue, typeof(int)));
                        break;
                    }
                case TokenType.BitwiseAnd:
                    {
                        _valueStack.Push((lhsValue & rhsValue, typeof(int)));
                        break;
                    }
                case TokenType.ConditionalOr:
                    {
                        _valueStack.Push((lhsValue || rhsValue, typeof(bool)));
                        break;
                    }
                case TokenType.ConditionalXOr:
                    {
                        _valueStack.Push((lhsValue ^ rhsValue, typeof(bool)));
                        break;
                    }
                case TokenType.ConditionalAnd:
                    {
                        _valueStack.Push((lhsValue && rhsValue, typeof(bool)));
                        break;
                    }
                case TokenType.BitShiftLeft:
                    {
                        _valueStack.Push((lhsValue << rhsValue, typeof(int)));
                        break;
                    }
                case TokenType.BitShiftRight:
                    {
                        _valueStack.Push((lhsValue >> rhsValue, typeof(int)));
                        break;
                    }
                default:
                    throw new ArgumentException($"invalid binary operator {expression.Token.TokenType}");
            }
        }

        public void VisitBodyExpression(BodyExpression expression)
        {
            foreach (var expr in expression.Body)
            {
                Visit(expr);
            }
        }

        public void VisitVariableDeclarationExpression(VariableDeclarationExpression expression)
        {
            Visit(expression.ValueExpression);
            var valueRhs = _valueStack.Pop();
            var nameLhs = (string)Convert.ChangeType(expression.Identifier, typeof(string));

            if (!_namedValues.ContainsKey(nameLhs))
            {
                // error using not previously instantiated variable.
                return;
            }
            //todo: handle scopes?
            _namedValues[nameLhs] = valueRhs;
        }

        public void VisitValueExpression(ValueExpression expression)
        {
            _valueStack.Push((expression.Value, expression.Value.GetType()));
        }

        public void VisitForStatementExpression(ForStatementExpression expression)
        {
            throw new NotImplementedException();
        }

        public void VisitIdentifierExpression(IdentifierExpression expression)
        {
            //todo, handle double identifiers in same or different scope?
            if (!_namedValues.TryGetValue(expression.Identifier, out var value))
            {
                throw new ArgumentException($"Unknown variable name {expression.Identifier}");
            }
            _valueStack.Push((value, value?.GetType()));
        }

        public void VisitIfStatementExpression(IfStatementExpression expression)
        {
            Visit(expression.IfCondition);
            Visit(expression.IfBody);
            Visit(expression.ElseBody);
        }

        public void VisitFunctionCallExpression(FunctionCallExpression expression)
        {
            foreach (var expr in expression.Arguments)
            {
                Visit(expr);
            }
        }

        public void VisitFunctionDefinitionExpression(FunctionDefinitionExpression expression)
        {
            _functions.Add(expression.FunctionName, expression);
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

        public void VisitReturnExpression(ReturnExpression expression)
        {
            //todo: how do returns even work?
            Visit(expression.ReturnExpr);
        }

        public void Initialize(List<IScope> scopes)
        {
            _scopes = scopes;
        }

        public void VisitContextExpression(ContextDefinitionExpression expression)
        {
            foreach (var context in expression.Contexts)
            {
                Visit(context);
            }

            foreach (var @enum in expression.Enums)
            {
                Visit(@enum);
            }

            foreach (var @class in expression.Classes)
            {
                Visit(@class);
            }
        }

        public void VisitClassExpression(ClassDefinitionExpression expression)
        {
            foreach (var @enum in expression.Enums)
            {
                Visit(@enum);
            }

            foreach (var @class in expression.Classes)
            {
                Visit(@class);
            }

            foreach(var variable in expression.Variables)
            {
                Visit(variable);
            }

            foreach(var function in expression.Functions)
            {
                Visit(function);
            }
        }

        public void VisitImportExpression(ImportStatementExpression expression)
        {
            
        }

        public void VisitMemberAccessExpression(MemberAccessExpression expression)
        {
            throw new NotImplementedException();
        }

        public void VisitEnumExpression(EnumDefinitionExpression expression)
        {
            throw new NotImplementedException();
        }

        public void VisitForeachStatementExpression(ForeachStatementExpression expression)
        {
            throw new NotImplementedException();
        }

        public void VisitSwitchStatementExpression(SwitchStatementExpression expression)
        {
            throw new NotImplementedException();
        }

        public void VisitObjectInstantiationExpression(ObjectInstantiationExpression expression)
        {
            throw new NotImplementedException();
        }
    }
}
