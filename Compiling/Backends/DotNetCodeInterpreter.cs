using Parsing.AbstractSyntaxTree.Expressions;
using Parsing.AbstractSyntaxTree.Visitors;

namespace Compiling.Backends
{
    internal class DotNetCodeInterpreter : IAbstractSyntaxTreeVisitor
    {
        private readonly Stack<object?> _valueStack = new Stack<object?>();
        private readonly Dictionary<string, object?> _namedValues = new Dictionary<string, object?>();
        public IEnumerable<object?> Results => _valueStack;
        public string Name => "Dot Net Interpreter / simulator";
        public void Visit(ExpressionBase? expression) => AbstractSyntaxTreeVisitor.Visit(this, expression);        

        public void VisitBinaryExpression(BinaryExpression expression)
        {
            Visit(expression.LeftHandSide);
            var lhsValue = Convert.ToDouble(_valueStack.Pop());
            Visit(expression.RightHandSide);
            var rhsValue = Convert.ToDouble(_valueStack.Pop());

            object resultingValue;
            //todo: should we use BuildAdd instead of BuildFAdd when dealing with integers?
            switch (expression.NodeExpressionType)
            {
                case ExpressionType.Add:
                    {
                        resultingValue = lhsValue + rhsValue;
                        break;
                    }
                case ExpressionType.Subtract:
                    {
                        resultingValue = lhsValue - rhsValue;
                        break;
                    }
                case ExpressionType.Multiply:
                    {
                        resultingValue = lhsValue * rhsValue;
                        break;
                    }
                case ExpressionType.Divide:
                    {
                        resultingValue = lhsValue / rhsValue;
                        break;
                    }
                case ExpressionType.Assignment:
                    {
                        // what to do here?
                        resultingValue = lhsValue;
                        break;
                    }
                case ExpressionType.Equivalent: //todo: actually make this do a type compare? 
                    {
                        resultingValue = lhsValue == rhsValue;
                        break;
                    }
                case ExpressionType.Equals:
                    {
                        resultingValue = lhsValue == rhsValue;
                        break;
                    }
                case ExpressionType.NotEquivalent: //todo: actually make this do a type compare? 
                    {
                        resultingValue = lhsValue != rhsValue;
                        break;
                    }
                case ExpressionType.NotEquals:
                    {
                        resultingValue = lhsValue != rhsValue;
                        break;
                    }
                case ExpressionType.GreaterThan:
                    {
                        resultingValue = lhsValue > rhsValue;
                        break;
                    }
                case ExpressionType.GreaterThanEqual:
                    {
                        resultingValue = lhsValue >= rhsValue;
                        break;
                    }
                case ExpressionType.LessThan:
                    {
                        resultingValue = lhsValue < rhsValue;
                        break;
                    }
                case ExpressionType.LessThanEqual:
                    {
                        resultingValue = lhsValue <= rhsValue;
                        break;
                    }
                case ExpressionType.LogicalOr:
                    {
                        resultingValue = (bool)Convert.ChangeType(lhsValue, typeof(bool)) | (bool)Convert.ChangeType(rhsValue, typeof(bool));
                        break;
                    }
                case ExpressionType.LogicalXOr:
                    {
                        resultingValue = (bool)Convert.ChangeType(lhsValue, typeof(bool)) ^ (bool)Convert.ChangeType(rhsValue, typeof(bool));
                        break;
                    }
                case ExpressionType.LogicalAnd:
                    {
                        resultingValue = (bool)Convert.ChangeType(lhsValue, typeof(bool)) & (bool)Convert.ChangeType(rhsValue, typeof(bool));
                        break;
                    }
                case ExpressionType.ConditionalOr:
                    {
                        resultingValue = (bool)Convert.ChangeType(lhsValue, typeof(bool)) || (bool)Convert.ChangeType(rhsValue, typeof(bool));
                        break;
                    }
                case ExpressionType.ConditionalAnd:
                    {
                        resultingValue = (bool)Convert.ChangeType(lhsValue, typeof(bool)) && (bool)Convert.ChangeType(rhsValue, typeof(bool));
                        break;
                    }
                case ExpressionType.BitShiftLeft:
                    {
                        resultingValue = (int)lhsValue << (int)rhsValue;
                        break;
                    }
                case ExpressionType.BitShiftRight:
                    {
                        resultingValue = (int)lhsValue >> (int)rhsValue;
                        break;
                    }
                default:
                    throw new ArgumentException($"invalid binary operator {expression.NodeExpressionType}");
            }
            _valueStack.Push(resultingValue);
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
