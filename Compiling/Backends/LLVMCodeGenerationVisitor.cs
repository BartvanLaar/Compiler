using LLVMSharp;
using Parsing.AbstractSyntaxTree.Expressions;
using Parsing.AbstractSyntaxTree.Visitors;
using System.Diagnostics;

namespace Compiling.Backends.LLVMSupport
{
    internal class LLVMCodeGenerationVisitor : IByteCodeGeneratorListener
    {

        private static readonly LLVMBool LLVMBoolFalse = new(0);

        private static readonly LLVMValueRef NullValue = new(IntPtr.Zero);

        private readonly LLVMModuleRef _module;

        private readonly LLVMBuilderRef _builder;
        private readonly LLVMExecutionEngineRef _executionEngine;
        private readonly LLVMPassManagerRef _passManager;
        private readonly Dictionary<string, LLVMValueRef> _namedValues = new();
        private delegate double D_FUNCTION_PTR(); // temp?

        private readonly Stack<LLVMValueRef> _valueStack = new();

        public LLVMCodeGenerationVisitor(LLVMModuleRef module, LLVMBuilderRef builder, LLVMExecutionEngineRef executionEngine, LLVMPassManagerRef passManager)
        {
            _module = module;
            _builder = builder;
            _executionEngine = executionEngine;
            _passManager = passManager;
        }

        public Stack<LLVMValueRef> ResultStack => _valueStack;

        public void ClearResultStack()
        {
            _valueStack.Clear();
        }

        public void VisitAssignmentExpression(AssignmentExpression expression)
        {
            //Visit(node.IdentificationExpression);
            //Visit(expression.ValueExpression);

            //doesnt an assignment have a lhs or rhs? how does this work :)
            //var rhsValue = _valueStack.Pop();
            var lhsValue = _valueStack.Pop();
            Debug.Assert(expression.IdentificationExpression.Token.HasValue);
            _namedValues.Add(expression.IdentificationExpression.Token.Value.Name, lhsValue);
        }
        public void VisitIntegerExpression(IntegerExpression expression)
        {
            _valueStack.Push(LLVM.ConstReal(LLVM.Int64Type(), expression.Value));
        }

        public void VisitDoubleExpression(DoubleExpression expression)
        {
            _valueStack.Push(LLVM.ConstReal(LLVM.DoubleType(), expression.Value));
        }

        public void VisitFloatExpression(FloatExpression expression)
        {
            _valueStack.Push(LLVM.ConstReal(LLVM.FloatType(), expression.Value));
        }
        public void VisitCharacterExpression(CharacterExpression expression)
        {
            throw new NotImplementedException();
        }

        public void VisitStringExpression(StringExpression expression)
        {
            throw new NotImplementedException();
        }

        public void VisitFunctionCallExpression(FunctionCallExpression expression)
        {
            //_namedValues.Clear(); // this was cleared.. But how about the variable i previously added to an outer scope..?
            //todo: add scoping...

            var body = _valueStack.Pop();
            var protoTypeFunction = _valueStack.Pop();
            LLVM.PositionBuilderAtEnd(_builder, LLVM.AppendBasicBlock(protoTypeFunction, "entry"));

            //todo: below code seems fair... need to delete a fooked up function
            //try
            //{
            //    Visit(node.Body);
            //}
            //catch (Exception)
            //{
            //    LLVM.DeleteFunction(function);
            //    throw;
            //}

            // Finish off the function.
            LLVM.BuildRet(_builder, _valueStack.Pop());

            // Validate the generated code, checking for consistency.
            if (!LLVM.VerifyFunction(protoTypeFunction, LLVMVerifierFailureAction.LLVMPrintMessageAction))
            {
                LLVM.DeleteFunction(protoTypeFunction);
                throw new Exception("Encountered a bad function..?"); // clarify message?
            }

            _valueStack.Push(protoTypeFunction);
        }

        public void VisitIdentifierExpression(IdentifierExpression expression)
        {
            if (!_namedValues.TryGetValue(expression.Identifier, out var value))
            {
                throw new ArgumentException($"Unknown variable name {expression.Identifier}");
            }

            _valueStack.Push(value);
        }

        public void VisitMethodCallExpression(MethodCallExpression expression)
        {
            var calleeFunction = LLVM.GetNamedFunction(_module, expression.Callee);

            if (calleeFunction.Pointer == IntPtr.Zero)
            {
                throw new Exception($"Unknown function referenced: {expression.Callee}");
            }

            var argumentCount = (uint)expression.MethodArguments.Length;
            if (LLVM.CountParams(calleeFunction) != argumentCount)
            {
                throw new Exception($"Incorrect # arguments passed to {expression.Callee}");
            }

            var argumentValues = new LLVMValueRef[argumentCount];
            for (int i = 0; i < argumentCount; ++i)
            {
                argumentValues[i] = _valueStack.Pop();
            }

            _valueStack.Push(LLVM.BuildCall(_builder, calleeFunction, argumentValues, "calltmp"));
        }

        public void VisitPrototypeExpression(PrototypeExpression expression)
        {
            var argumentCount = (uint)expression.Arguments.Length;
            var arguments = new LLVMTypeRef[argumentCount];

            var function = LLVM.GetNamedFunction(_module, expression.Name);
            if (function.Pointer != IntPtr.Zero)
            {
                if (LLVM.CountBasicBlocks(function) != 0)
                {
                    throw new Exception($"Redefinition of function :'{expression.Name}'");
                }

                if (LLVM.CountParams(function) != argumentCount)
                {
                    //todo: we should support method overloading...
                    throw new Exception($"Redefinition of function :'{expression.Name}' with a different number of arguments.");
                }

            }
            else
            {
                //todo: support values other than doubles? We do have access to the tokens and their types... so why not?
                for (int i = 0; i < argumentCount; ++i)
                {
                    arguments[i] = LLVM.DoubleType();
                }

                function = LLVM.AddFunction(_module, expression.Name, LLVM.FunctionType(LLVM.DoubleType(), arguments, LLVMBoolFalse));
                LLVM.SetLinkage(function, LLVMLinkage.LLVMExternalLinkage);
            }

            for (int i = 0; i < argumentCount; ++i)
            {
                var argumentName = expression.ArgumentNames[i];

                LLVMValueRef param = LLVM.GetParam(function, (uint)i);
                LLVM.SetValueName(param, argumentName);

                _namedValues[argumentName] = param;
            }

            _valueStack.Push(function);
        }

        public void VisitBinaryExpression(BinaryExpression expression)
        {
            var rhsValue = _valueStack.Pop();
            var lhsValue = _valueStack.Pop();

            LLVMValueRef resultingValue;
            //todo: should we use BuildAdd instead of BuildFAdd when dealing with integers?
            switch (expression.NodeExpressionType)
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

        }

        public void VisitIfStatementExpression(IfStatementExpression expression)
        {
            throw new NotImplementedException();
        }

        public void VisitForStatementExpression(ForStatementExpression expression)
        {
            throw new NotImplementedException();
        }

        public void VisitBodyStatementExpression(BodyExpression expression)
        {
            throw new NotImplementedException();
        }
    }
}
