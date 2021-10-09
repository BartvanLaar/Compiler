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
                    resultingValue = (double)lhsValue + (double)rhsValue;
                    break;
                case ExpressionType.Subtract:
                    resultingValue = (double)lhsValue - (double)rhsValue;
                    break;
                case ExpressionType.Multiply:
                    resultingValue = (double)lhsValue * (double)rhsValue;
                    break;
                case ExpressionType.Divide:
                    resultingValue = (double)lhsValue / (double)rhsValue;
                    break;
                case ExpressionType.Equivalent: //todo: actually make this do a type compare? 
                    {
                        resultingValue = (double)lhsValue == (double)rhsValue;
                        break;
                    }
                case ExpressionType.Equals:
                    {
                        resultingValue = (double)lhsValue == (double)rhsValue;
                        break;
                    }
                case ExpressionType.GreaterThan:
                    {
                        resultingValue = (double)lhsValue > (double)rhsValue;
                        break;
                    }
                case ExpressionType.GreaterThanEqual:
                    {
                        resultingValue = (double)lhsValue >= (double)rhsValue;
                        break;
                    }
                case ExpressionType.LessThan:
                    {
                        resultingValue = (double)lhsValue < (double)rhsValue;
                        break;
                    }
                case ExpressionType.LessThanEqual:
                    {
                        resultingValue = (double)lhsValue <= (double)rhsValue;
                        break;
                    }
                default:
                    throw new ArgumentException("invalid binary operator");
            }
            _valueStack.Push(resultingValue);
        }

        public void VisitBodyStatementExpression(BodyExpression expression)
        {
            throw new NotImplementedException();
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

        public void VisitFunctionCallExpression(FunctionCallExpression expression)
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

        public void VisitMethodCallExpression(MethodCallExpression expression)
        {
            throw new NotImplementedException();
        }

        public void VisitPrototypeExpression(PrototypeExpression expression)
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
