using Parsing.AbstractSyntaxTree.Expressions;
using Parsing.AbstractSyntaxTree.Visitors;
using System.Diagnostics;

namespace Compiling.Backends
{
    internal class DotNetCodeInterpreter : IAbstractSyntaxTreeVisitor
    {
        private readonly Stack<object?> _valueStack = new();
        private readonly Dictionary<string, object?> _namedValues = new();
        public IEnumerable<object?> Results => _valueStack;
        public string Name => "Dot Net Interpreter / simulator";
        public void Visit(ExpressionBase? expression) => AbstractSyntaxTreeVisitor.Visit(this, expression);        

        public void VisitBinaryExpression(BinaryExpression expression)
        {
            Visit(expression.LeftHandSide);
            var lhsValue = Convert.ToDouble(_valueStack.Pop());
            Visit(expression.RightHandSide);
            var rhsValue = Convert.ToDouble(_valueStack.Pop());

            switch (expression.NodeExpressionType)
            {
                case ExpressionType.Add:
                    {
                         _valueStack.Push(lhsValue + rhsValue);
                        break;
                    }
                case ExpressionType.Subtract:
                    {
                         _valueStack.Push(lhsValue - rhsValue);
                        break;
                    }
                case ExpressionType.Multiply:
                    {
                         _valueStack.Push(lhsValue * rhsValue);
                        break;
                    }
                case ExpressionType.Divide:
                    {
                         _valueStack.Push(lhsValue / rhsValue);
                        break;
                    }
                case ExpressionType.Assignment:
                    {
                        var expr = expression.LeftHandSide as IdentifierExpression;
                        Debug.Assert(expr is not null);
                        Debug.Assert(_namedValues.ContainsKey(expr.Identifier));
                        _namedValues[expr.Identifier] = lhsValue;
                        break;
                    }
                case ExpressionType.Equivalent: //todo: actually make this do a type compare? 
                    {
                         _valueStack.Push(lhsValue == rhsValue);
                        break;
                    }
                case ExpressionType.Equals:
                    {
                         _valueStack.Push(lhsValue == rhsValue);
                        break;
                    }
                case ExpressionType.NotEquivalent: //todo: actually make this do a type compare? 
                    {
                         _valueStack.Push(lhsValue != rhsValue);
                        break;
                    }
                case ExpressionType.NotEquals:
                    {
                         _valueStack.Push(lhsValue != rhsValue);
                        break;
                    }
                case ExpressionType.GreaterThan:
                    {
                         _valueStack.Push(lhsValue > rhsValue);
                        break;
                    }
                case ExpressionType.GreaterThanEqual:
                    {
                         _valueStack.Push(lhsValue >= rhsValue);
                        break;
                    }
                case ExpressionType.LessThan:
                    {
                         _valueStack.Push(lhsValue < rhsValue);
                        break;
                    }
                case ExpressionType.LessThanEqual:
                    {
                         _valueStack.Push(lhsValue <= rhsValue);
                        break;
                    }
                case ExpressionType.LogicalOr:
                    {
                         _valueStack.Push((bool)Convert.ChangeType(lhsValue, typeof(bool)) | (bool)Convert.ChangeType(rhsValue, typeof(bool)));
                        break;
                    }
                case ExpressionType.LogicalXOr:
                    {
                         _valueStack.Push((bool)Convert.ChangeType(lhsValue, typeof(bool)) ^ (bool)Convert.ChangeType(rhsValue, typeof(bool)));
                        break;
                    }
                case ExpressionType.LogicalAnd:
                    {
                         _valueStack.Push((bool)Convert.ChangeType(lhsValue, typeof(bool)) & (bool)Convert.ChangeType(rhsValue, typeof(bool)));
                        break;
                    }
                case ExpressionType.ConditionalOr:
                    {
                         _valueStack.Push((bool)Convert.ChangeType(lhsValue, typeof(bool)) || (bool)Convert.ChangeType(rhsValue, typeof(bool)));
                        break;
                    }
                case ExpressionType.ConditionalAnd:
                    {
                         _valueStack.Push((bool)Convert.ChangeType(lhsValue, typeof(bool)) && (bool)Convert.ChangeType(rhsValue, typeof(bool)));
                        break;
                    }
                case ExpressionType.BitShiftLeft:
                    {
                         _valueStack.Push((int)lhsValue << (int)rhsValue);
                        break;
                    }
                case ExpressionType.BitShiftRight:
                    {
                         _valueStack.Push((int)lhsValue >> (int)rhsValue);
                        break;
                    }
                default:
                    throw new ArgumentException($"invalid binary operator {expression.NodeExpressionType}");
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

        public void VisitBooleanExpression(BooleanExpression expression)
        {
            _valueStack.Push(expression.Value);
        }

        public void VisitCharacterExpression(CharacterExpression expression)
        {
            _valueStack.Push(expression.Value);
        }

        public void VisitDoubleExpression(DoubleExpression expression)
        {
            _valueStack.Push(expression.Value);
        }

        public void VisitFloatExpression(FloatExpression expression)
        {
            _valueStack.Push(expression.Value);
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
            _valueStack.Push(value);
        }

        public void VisitIfStatementExpression(IfStatementExpression expression)
        {
            Visit(expression.IfCondition);
            Visit(expression.IfBody);
            Visit(expression.Else);
        }

        public void VisitIntegerExpression(IntegerExpression expression)
        {
            _valueStack.Push(expression.Value);
        }
        public void VisitStringExpression(StringExpression expression)
        {
            _valueStack.Push(expression.Value);
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
            Visit(expression.FunctionBody);
        }

        public void VisitWhileStatementExpression(WhileStatementExpression expression)
        {
            Visit(expression.WhileCondition);
            Visit(expression.DoBody);
        }

        public void VisitReturnExpression(ReturnExpression expression)
        {
            //todo: how do returns even work?
            Visit(expression.Expression);
        }
    }
}
