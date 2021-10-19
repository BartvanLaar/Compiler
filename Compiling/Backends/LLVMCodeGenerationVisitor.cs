using LLVMSharp;
using Parsing.AbstractSyntaxTree.Expressions;
using Parsing.AbstractSyntaxTree.Visitors;
using System.Diagnostics;

namespace Compiling.Backends
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

        //public void VisitAssignmentExpression(AssignmentExpression expression)
        //{
        //    //Visit(node.IdentificationExpression);
        //    //Visit(expression.ValueExpression);

        //    //doesnt an assignment have a lhs or rhs? how does this work :)
        //    //var rhsValue = _valueStack.Pop();
        //    var lhsValue = _valueStack.Pop();
        //    Debug.Assert(expression.IdentificationExpression.Token.HasValue);
        //    _namedValues.Add(expression.IdentificationExpression.Token.Value.Name, lhsValue);
        //}
        public void VisitBooleanExpression(BooleanExpression expression)
        {
            //todo: is this the right way to handle bools..?
            _valueStack.Push(LLVM.ConstReal(LLVM.Int64Type(), expression.Value ? 1 : 0));
            throw new NotImplementedException();
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
            //todo: shouldnt a character be converted to an int?
            _valueStack.Push(LLVM.ConstRealOfStringAndSize(LLVM.Int64Type(), expression.Value.ToString(),1));
        }

        public void VisitStringExpression(StringExpression expression)
        {
            _valueStack.Push(LLVM.ConstRealOfString(LLVM.Int64Type(), expression.Value));
        }

        public void VisitIdentifierExpression(IdentifierExpression expression)
        {
            if (!_namedValues.TryGetValue(expression.Identifier, out var value))
            {
                throw new ArgumentException($"Unknown variable name {expression.Identifier}");
            }

            _valueStack.Push(value);
        }

        public void VisitFunctionCallExpression(FunctionCallExpression expression)
        {
            var calleeFunction = LLVM.GetNamedFunction(_module, expression.FunctionName);

            if (calleeFunction.Pointer == IntPtr.Zero)
            {
                throw new Exception($"Unknown function referenced: {expression.FunctionName}");
            }

            var argumentCount = (uint)expression.Arguments.Length;
            if (LLVM.CountParams(calleeFunction) != argumentCount)
            {
                throw new Exception($"Incorrect # arguments passed to {expression.FunctionName}");
            }

            var argumentValues = new LLVMValueRef[argumentCount];
            for (int i = 0; i < argumentCount; ++i)
            {
                argumentValues[i] = _valueStack.Pop();
            }

            _valueStack.Push(LLVM.BuildCall(_builder, calleeFunction, argumentValues, "calltmp"));
        }

        public void VisitFunctionDefinitionExpression(FunctionDefinitionExpression expression)
        {
            Debug.Assert(expression?.Token != null);

            var argumentCount = (uint)expression.Arguments.Length;
            var arguments = new LLVMTypeRef[argumentCount];
            var expressionName = expression.Token.Value.Name;

            var function = LLVM.GetNamedFunction(_module, expressionName);
            if (function.Pointer != IntPtr.Zero)
            {
                if (LLVM.CountBasicBlocks(function) != 0)
                {
                    throw new Exception($"Redefinition of function :'{expressionName}'");
                }

                if (LLVM.CountParams(function) != argumentCount)
                {
                    //todo: we should support method overloading...
                    throw new Exception($"Redefinition of function :'{expressionName}' with a different number of arguments.");
                }

            }
            else
            {
                //todo: support values other than doubles? We do have access to the tokens and their types... so why not?
                for (int i = 0; i < argumentCount; ++i)
                {
                    arguments[i] = LLVM.DoubleType();
                }

                function = LLVM.AddFunction(_module, expressionName, LLVM.FunctionType(LLVM.DoubleType(), arguments, LLVMBoolFalse));
                LLVM.SetLinkage(function, LLVMLinkage.LLVMExternalLinkage);
            }

            for (int i = 0; i < argumentCount; ++i)
            {
                Debug.Assert(!string.IsNullOrWhiteSpace(expression?.Arguments[i].ValueToken.Name));
                var argumentName = expression.Arguments[i].ValueToken.Name;// todo: is this right?

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

        public void VisitWhileStatementExpression(WhileStatementExpression expression)
        {
            throw new NotImplementedException();
        }
    }
}
