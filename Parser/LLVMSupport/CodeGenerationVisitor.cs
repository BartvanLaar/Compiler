using LLVMSharp;
using Parser.AbstractSyntaxTree;
using Parser.AbstractSyntaxTree.Expressions;

namespace Parser.LLVMSupport
{
    internal sealed class CodeGenerationVisitor : ExpressionVisitor
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
            _valueStack.Push(LLVM.ConstReal(LLVM.Int64Type(), node.Value));
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
            if (_namedValues.TryGetValue(node.VariableName, out var value))
            {
                _valueStack.Push(value);
            }
            else
            {
                throw new ArgumentException($"Unknown variable name {node.VariableName}");
            }

            return node;
        }

        protected internal override ExpressionBase? VisitAssignmentExpression(AssignmentExpression node)
        {
            //Visit(node.IdentificationExpression);
            Visit(node.ValueExpression);
            
            //var rhsValue = _valueStack.Pop();
            var lhsValue = _valueStack.Pop();
            _namedValues.Add(node.IdentificationExpression.Token.Value.Name, lhsValue);

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

        protected internal override ExpressionBase VisitMethodCallExpression(MethodCallExpression node)
        {
            var calleeFunction = LLVM.GetNamedFunction(_module, node.Callee);

            if (calleeFunction.Pointer == IntPtr.Zero)
            {
                throw new Exception($"Unknown function referenced: {node.Callee}");
            }

            var argumentCount = (uint)node.MethodArguments.Length;
            if (LLVM.CountParams(calleeFunction) != argumentCount)
            {
                throw new Exception($"Incorrect # arguments passed to {node.Callee}");
            }

            var argumentValues = new LLVMValueRef[argumentCount];
            for (int i = 0; i < argumentCount; ++i)
            {
                Visit(node.MethodArguments[i]);
                argumentValues[i] = _valueStack.Pop();
            }

            _valueStack.Push(LLVM.BuildCall(_builder, calleeFunction, argumentValues, "calltmp"));

            return node;
        }

        protected internal override ExpressionBase VisitPrototype(PrototypeExpression node)
        {
            var argumentCount = (uint)node.Arguments.Length;
            var arguments = new LLVMTypeRef[argumentCount];

            var function = LLVM.GetNamedFunction(_module, node.Name);
            if (function.Pointer != IntPtr.Zero)
            {
                if (LLVM.CountBasicBlocks(function) != 0)
                {
                    throw new Exception($"Redefinition of function :'{node.Name}'");
                }

                if (LLVM.CountParams(function) != argumentCount)
                {
                    //todo: we should support method overloading...
                    throw new Exception($"Redefinition of function :'{node.Name}' with a different number of arguments.");
                }

            }
            else
            {
                //todo: support values other than doubles? We do have access to the tokens and their types... so why not?
                for (int i = 0; i < argumentCount; ++i)
                {
                    arguments[i] = LLVM.DoubleType();
                }

                function = LLVM.AddFunction(_module, node.Name, LLVM.FunctionType(LLVM.DoubleType(), arguments, LLVMBoolFalse));
                LLVM.SetLinkage(function, LLVMLinkage.LLVMExternalLinkage);
            }

            for (int i = 0; i < argumentCount; ++i)
            {
                var argumentName = node.ArgumentNames[i];

                LLVMValueRef param = LLVM.GetParam(function, (uint)i);
                LLVM.SetValueName(param, argumentName);

                _namedValues[argumentName] = param;
            }

            _valueStack.Push(function);
            return node;
        }

        protected internal override ExpressionBase VisitFunctionCallExpression(FunctionCallExpression node)
        {
            //_namedValues.Clear(); // this was cleared.. But how about the variable i previously added to an outer scope..?
            //todo: add scoping...
            Visit(node.Prototype);

            var function = _valueStack.Pop();
            LLVM.PositionBuilderAtEnd(_builder, LLVM.AppendBasicBlock(function, "entry"));

            try
            {
                Visit(node.Body);
            }
            catch (Exception)
            {
                LLVM.DeleteFunction(function);
                throw;
            }
            
            // Finish off the function.
            LLVM.BuildRet(_builder, _valueStack.Pop());

            // Validate the generated code, checking for consistency.
            LLVM.VerifyFunction(function, LLVMVerifierFailureAction.LLVMPrintMessageAction);

            _valueStack.Push(function);
            return node;
        }

    }
}
