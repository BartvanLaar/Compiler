using LLVMSharp;
using Parser.AbstractSyntaxTree;
using Parser.AbstractSyntaxTree.Expressions;

namespace Parser.LLVMSupport
{
    internal sealed class CodeGenerationVisitor: ExpressionVisitor
    {
        private static readonly LLVMBool LLVMBoolFalse = new(0);

        private static readonly LLVMValueRef NullValue = new(IntPtr.Zero);

        private readonly LLVMModuleRef _module;

        private readonly LLVMBuilderRef _builder;

        private readonly Dictionary<string, LLVMValueRef> _namedValues = new();

        private readonly Stack<LLVMValueRef> _valueStack = new();

        public CodeGenerationVisitor(LLVMModuleRef module, LLVMBuilderRef builder)
        {
            _module = module;
            _builder = builder;
        }

        public Stack<LLVMValueRef> ResultStack => _valueStack;

        public void ClearResultStack()
        {
            _valueStack.Clear();
        }

        protected internal override ExpressionBase VisitIntegerExpression(IntegerExpression node)
        {
            _valueStack.Push(LLVM.ConstReal(LLVM.Int128Type(), node.Value));
            return node;
        }

        protected internal override ExpressionBase VisitDoubleExpression(DoubleExpression node)
        {
            _valueStack.Push(LLVM.ConstReal(LLVM.DoubleType(), node.Value));
            return node;
        }

        protected internal override ExpressionBase VisitFloatExpression(FloatExpression node)
        {
            _valueStack.Push(LLVM.ConstReal(LLVM.FloatType(), node.Value));
            return node;
        }

        protected internal override ExpressionBase VisitVariableEvaluationExpression(VariableEvaluationExpression node)
        {
            if(_namedValues.TryGetValue(node.VariableName, out var value))
            {
                _valueStack.Push(value);
            } 
            else
            {
                throw new ArgumentException($"Unknown variable name {node.VariableName}");
            }

            return node;
        }

        protected internal override ExpressionBase VisitBinaryExpression(BinaryExpression node)
        {
            Visit(node.LeftHandSide);
            Visit(node.RightHandSide);

            var rhsValue = _valueStack.Pop();
            var lhsValue = _valueStack.Pop();

            LLVMValueRef resultingValue;
            //todo: should we use BuildAdd instead of BuildFAdd when dealing with integers?
            switch (node.NodeExpressionType)
            {
                case ExpressionType.Add:
                    resultingValue = LLVM.BuildFAdd(_builder, lhsValue, rhsValue, "addtmp");
                    break;
                case ExpressionType.Subtract:
                    resultingValue = LLVM.BuildFSub(_builder, lhsValue, rhsValue, "subtmp");
                    break;
                case ExpressionType.Multiply:
                    resultingValue = LLVM.BuildFMul(_builder, lhsValue, rhsValue, "multmp");
                    break;
                case ExpressionType.Divide:
                    resultingValue = LLVM.BuildFDiv(_builder, lhsValue, rhsValue, "divtmp");
                    break;
                case ExpressionType.Equivalent: //todo: actually make this do a type compare? 
                    {
                        var floatCompare = LLVM.BuildFCmp(_builder, LLVMRealPredicate.LLVMRealUEQ, lhsValue, rhsValue, "cmptmp");
                        resultingValue = LLVM.BuildUIToFP(_builder, floatCompare, LLVM.DoubleType(), "booltmp");
                        break;
                    }
                case ExpressionType.Equals:
                    {
                        var floatCompare = LLVM.BuildFCmp(_builder, LLVMRealPredicate.LLVMRealUEQ, lhsValue, rhsValue, "cmptmp");
                        resultingValue = LLVM.BuildUIToFP(_builder, floatCompare, LLVM.DoubleType(), "booltmp");
                        break;
                    }
                case ExpressionType.GreaterThan:
                    {
                        var floatCompare = LLVM.BuildFCmp(_builder, LLVMRealPredicate.LLVMRealUGT, lhsValue, rhsValue, "cmptmp");
                        resultingValue = LLVM.BuildUIToFP(_builder, floatCompare, LLVM.DoubleType(), "booltmp");
                        break;
                    }
                case ExpressionType.GreaterThanEqual:
                    {
                        var floatCompare = LLVM.BuildFCmp(_builder, LLVMRealPredicate.LLVMRealUGE, lhsValue, rhsValue, "cmptmp");
                        resultingValue = LLVM.BuildUIToFP(_builder, floatCompare, LLVM.DoubleType(), "booltmp");
                        break;
                    }
                case ExpressionType.LessThan:
                    {
                        var floatCompare = LLVM.BuildFCmp(_builder, LLVMRealPredicate.LLVMRealULT, lhsValue, rhsValue, "cmptmp");
                        resultingValue = LLVM.BuildUIToFP(_builder, floatCompare, LLVM.DoubleType(), "booltmp");
                        break;
                    }
                case ExpressionType.LessThanEqual:
                    {
                        var floatCompare = LLVM.BuildFCmp(_builder, LLVMRealPredicate.LLVMRealULE, lhsValue, rhsValue, "cmptmp");
                        resultingValue = LLVM.BuildUIToFP(_builder, floatCompare, LLVM.DoubleType(), "booltmp");
                        break;
                    }
                default:
                    throw new ArgumentException("invalid binary operator");                    
            }

            _valueStack.Push(resultingValue);
            return node;
        }

    }
}
