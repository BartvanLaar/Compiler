using Parsing.AbstractSyntaxTree.Expressions;
using Parsing.AbstractSyntaxTree.Visitors;

namespace Compiling.Backends
{
    internal class DotNetCodeGenerationVisitor : IByteCodeGeneratorListener
    {
        private readonly Stack<object> _valueStack = new Stack<object>();
        public IEnumerable<object> Results => _valueStack;
        public void VisitAssignmentExpression(AssignmentExpression expression)
        {
            throw new NotImplementedException();
        }

        public void VisitBinaryExpression(BinaryExpression expression)
        {
            var rhsValue = Convert.ToDouble(_valueStack.Pop());
            var lhsValue = Convert.ToDouble(_valueStack.Pop());

            object resultingValue;
            //todo: should we use BuildAdd instead of BuildFAdd when dealing with integers?
            switch (expression.NodeExpressionType)
            {
                case ExpressionType.Add:
                    resultingValue = lhsValue + rhsValue;
                    break;
                case ExpressionType.Subtract:
                    resultingValue = lhsValue - rhsValue;
                    break;
                case ExpressionType.Multiply:
                    resultingValue = lhsValue * rhsValue;
                    break;
                case ExpressionType.Divide:
                    resultingValue = lhsValue / rhsValue;
                    break;
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
                case ExpressionType.Or:
                    {
                        resultingValue = (bool)Convert.ChangeType(lhsValue, typeof(bool)) | (bool)Convert.ChangeType(rhsValue, typeof(bool));
                        break;
                    }
                case ExpressionType.OrElse:
                    {
                        resultingValue = (bool)Convert.ChangeType(lhsValue, typeof(bool)) || (bool)Convert.ChangeType(rhsValue, typeof(bool));
                        break;
                    }
                case ExpressionType.And:
                    {
                        resultingValue = (bool)Convert.ChangeType(lhsValue, typeof(bool)) & (bool)Convert.ChangeType(rhsValue, typeof(bool));
                        break;
                    }
                case ExpressionType.AndAlso:
                    {
                        resultingValue = (bool)Convert.ChangeType(lhsValue, typeof(bool)) && (bool)Convert.ChangeType(rhsValue, typeof(bool));
                        break;
                    }
                default:
                    throw new ArgumentException($"invalid binary operator {expression.NodeExpressionType}");
            }
            _valueStack.Push(resultingValue);
        }

        public void VisitBodyStatementExpression(BodyExpression expression)
        {
            throw new NotImplementedException();
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

        public void VisitFunctionCallExpression(FunctionDefinitionExpression expression)
        {
            throw new NotImplementedException();
        }

        public void VisitIdentifierExpression(IdentifierExpression expression)
        {
            throw new NotImplementedException();
        }

        public void VisitIfStatementExpression(IfStatementExpression expression)
        {
            throw new NotImplementedException();
        }

        public void VisitIntegerExpression(IntegerExpression expression)
        {
            _valueStack.Push(expression.Value);
        }

        public void VisitFunctionCallExpression(FunctionCallExpression expression)
        {
            throw new NotImplementedException();
        }

        public void VisitFunctionDefinitionExpression(FunctionDefinitionExpression expression)
        {
            throw new NotImplementedException();
        }

        public void VisitStringExpression(StringExpression expression)
        {
            _valueStack.Push(expression.Value);
        }

        public void VisitWhileStatementExpression(WhileStatementExpression expression)
        {
            throw new NotImplementedException();
        }
    }
}
