using Lexing;
using LLVMSharp.Interop;
using Parsing.AbstractSyntaxTree.Expressions;
using Parsing.AbstractSyntaxTree.Visitors;
using System.Diagnostics;

namespace Compiling.Backends
{
    internal class LLVMCodeGenerationVisitor : IByteCodeGeneratorListener
    {
        private static readonly LLVMValueRef NullValue = new(IntPtr.Zero);

        private readonly LLVMModuleRef _module;

        private readonly LLVMBuilderRef _builder;
        private readonly LLVMExecutionEngineRef _executionEngine;
        private readonly LLVMPassManagerRef _passManager;
        private readonly Dictionary<string, LLVMValueRef> _namedValues = new();
        private delegate double D_FUNCTION_PTR(); // temp?

        private readonly Stack<LLVMValueRef> _valueStack = new();
        //LLVM.LoadLibraryPermanently() // should i use this to load a c lib for printing to consoles?
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

        public void VisitVariableDeclarationExpression(VariableDeclarationExpression expression)
        {
            var rhsValue = _valueStack.Pop();
            Debug.Assert(expression.IdentificationExpression.Token?.Name is not null);
            _namedValues.Add(expression.IdentificationExpression.Token.Name, rhsValue);
        }

        public void VisitBooleanExpression(BooleanExpression expression)
        {
            _valueStack.Push(LLVMValueRef.CreateConstReal(LLVMTypeRef.Int1, expression.Value ? 1 : 0));
        }

        public void VisitIntegerExpression(IntegerExpression expression)
        {
            Debug.Assert(expression.Token is not null);
            _valueStack.Push(LLVMValueRef.CreateConstInt(LLVMTypeRef.Int64, (ulong) expression.Value, true));

        }

        public void VisitDoubleExpression(DoubleExpression expression)
        {
            _valueStack.Push(LLVMValueRef.CreateConstReal(LLVMTypeRef.Double, expression.Value));
        }

        public void VisitFloatExpression(FloatExpression expression)
        {
            _valueStack.Push(LLVMValueRef.CreateConstReal(LLVMTypeRef.Float, expression.Value));
        }

        public void VisitCharacterExpression(CharacterExpression expression)
        {
            //todo: shouldnt a character be converted to an int?
            _valueStack.Push(LLVMValueRef.CreateConstInt(LLVMTypeRef.Int16, (ulong)expression.Value, false));
        }

        public void VisitStringExpression(StringExpression expression)
        {
            _valueStack.Push(LLVMValueRef.CreateConstRealOfStringAndSize(LLVMTypeRef.Int16, expression.Value));
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
            var calleeFunction = _module.GetNamedFunction(expression.FunctionName);

            if (calleeFunction.Handle == IntPtr.Zero)
            {
                throw new Exception($"Unknown function referenced: {expression.FunctionName}");
            }

            var argumentCount = (uint)expression.Arguments.Length;
            if (calleeFunction.ParamsCount != argumentCount)
            {
                throw new Exception($"Incorrect # arguments passed to {expression.FunctionName}");
            }

            var argumentValues = new LLVMValueRef[argumentCount];
            for (int i = 0; i < argumentCount; ++i)
            {
                argumentValues[i] = _valueStack.Pop();
            }

            var call = _builder.BuildCall(calleeFunction, argumentValues, "calltmp");
            _valueStack.Push(call);
        }

        public void VisitFunctionDefinitionExpression(FunctionDefinitionExpression expression)
        {
            Debug.Assert(expression?.Token is not null);

            var argumentCount = (uint)expression.Arguments.Length;
            var arguments = new LLVMTypeRef[argumentCount];
            var expressionName = expression.Token.Name;

            var function = _module.GetNamedFunction(expressionName);
            if (function.Handle != IntPtr.Zero)
            {
                if (function.BasicBlocksCount != 0)
                {
                    throw new Exception($"Redefinition of function :'{expressionName}'");
                }

                if (function.ParamsCount != argumentCount)
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
                    arguments[i] = GetReturnType(expression.Arguments[i].TypeToken.TokenType);
                }
                var retType = GetReturnType(expression.ReturnTypeToken.TokenType);
                function = _module.AddFunction(expressionName, LLVMTypeRef.CreateFunction(retType, arguments, false));
                function.Linkage = LLVMLinkage.LLVMExternalLinkage;
            }

            for (int i = 0; i < argumentCount; ++i)
            {
                Debug.Assert(!string.IsNullOrWhiteSpace(expression?.Arguments[i].ValueToken.Name));
                var argumentName = expression.Arguments[i].ValueToken.Name;// todo: is this right?

                LLVMValueRef param = function.GetParam((uint)i);
                param.Name = argumentName;

                _namedValues[argumentName] = param;
            }
            //todo: what does the below statement Do??? have copy pasted it above the verify and suddenly more tests became green.. but the statuscodes returned where not as i expected
            _builder.PositionAtEnd(function.AppendBasicBlock("entry")); // this in combination with specifying /entry:Main causes an .exe to be able to be build.

            //todo: implement visit body and add it to the function? So actual code can be run
            if (expression.ReturnTypeToken.TokenType is TokenType.Void)
            {
                _builder.BuildRetVoid();
            }
            else
            {
                var retValue = _valueStack.Pop();
                _builder.BuildRet(retValue);
            }
            _builder.PositionAtEnd(function.AppendBasicBlock("entry")); // this in combination with specifying /entry:Main causes an .exe to be able to be build.

            function.VerifyFunction(LLVMVerifierFailureAction.LLVMPrintMessageAction);
            _valueStack.Push(function);
        }

        private static LLVMTypeRef GetReturnType(TokenType tokenType)
        {
            return tokenType switch
            {
                TokenType.Float => LLVMTypeRef.Float,
                TokenType.Double => LLVMTypeRef.Double,
                TokenType.Boolean => LLVMTypeRef.Int1,
                TokenType.Integer => LLVMTypeRef.Int64,
                TokenType.Character => LLVMTypeRef.Int16,
                TokenType.String => LLVMTypeRef.Int16,// should be an array i think ?
                TokenType.DateTime => throw new NotImplementedException(),
                TokenType.Void => LLVMTypeRef.Void,
                _ => throw new InvalidOperationException($"TokenType {tokenType} is not supported as a return type for LLVM."),
            };
        }

        public void VisitBinaryExpression(BinaryExpression expression)
        {
            var rhsValue = _valueStack.Pop();
            var lhsValue = _valueStack.Pop();
            var lhsAndRhsBothIntegers = rhsValue.TypeOf.Kind is LLVMTypeKind.LLVMIntegerTypeKind && lhsValue.TypeOf.Kind is LLVMTypeKind.LLVMIntegerTypeKind;
            LLVMValueRef resultingValue;
            //todo: handle unsigned ints doubles and floats?
            //todo: should we use BuildAdd instead of BuildFAdd when dealing with integers?
            switch (expression.NodeExpressionType)
            {
                case ExpressionType.Add:
                    {
                        if (lhsAndRhsBothIntegers)
                        {
                            resultingValue = _builder.BuildAdd(lhsValue, rhsValue, "addtmp");
                        }
                        else
                        {
                            resultingValue = _builder.BuildFAdd(lhsValue, rhsValue, "addtmp");
                        }
                        break;
                    }
                case ExpressionType.Subtract:
                    {
                        if (lhsAndRhsBothIntegers)
                        {
                            resultingValue = _builder.BuildSub(lhsValue, rhsValue, "addtmp");
                        }
                        else
                        {
                            resultingValue = _builder.BuildFSub(lhsValue, rhsValue, "subtmp");
                        }
                        break;
                    }
                case ExpressionType.Multiply:
                    {
                        if (lhsAndRhsBothIntegers)
                        {
                            resultingValue = _builder.BuildMul(lhsValue, rhsValue, "multmp");
                        }
                        else
                        {
                            resultingValue = _builder.BuildFMul(lhsValue, rhsValue, "multmp");
                        }
                        break;
                    }
                case ExpressionType.Divide:
                    {
                        if (lhsAndRhsBothIntegers)
                        {
                            resultingValue = _builder.BuildSDiv(lhsValue, rhsValue, "divtmp");
                        }
                        else
                        {
                            resultingValue = _builder.BuildFDiv(lhsValue, rhsValue, "divtmp");
                        }
                        break;
                    }
                case ExpressionType.Modulo:
                    {
                        if (lhsAndRhsBothIntegers)
                        {
                            resultingValue = _builder.BuildSRem(lhsValue, rhsValue, "modtmp");
                        }
                        else
                        {
                            resultingValue = _builder.BuildFRem(lhsValue, rhsValue, "modtmp");
                        }
                        break;
                    }
                case ExpressionType.Equivalent: //todo: actually make this do a type compare? 
                    {
                        if (lhsAndRhsBothIntegers)
                        {
                            resultingValue = _builder.BuildICmp(LLVMIntPredicate.LLVMIntEQ, lhsValue, rhsValue, "cmptmp");
                        }
                        else
                        {
                            resultingValue = _builder.BuildFCmp(LLVMRealPredicate.LLVMRealUEQ, lhsValue, rhsValue, "cmptmp");
                        }
                        break;
                    }
                case ExpressionType.Equals:
                    {
                        if (lhsAndRhsBothIntegers)
                        {
                            resultingValue = _builder.BuildICmp(LLVMIntPredicate.LLVMIntEQ, lhsValue, rhsValue, "cmptmp");
                        }
                        else
                        {
                            resultingValue = _builder.BuildFCmp(LLVMRealPredicate.LLVMRealUEQ, lhsValue, rhsValue, "cmptmp");
                        }
                        break;
                    }
                case ExpressionType.GreaterThan:
                    {
                        if (lhsAndRhsBothIntegers)
                        {
                            resultingValue = _builder.BuildICmp(LLVMIntPredicate.LLVMIntSGT, lhsValue, rhsValue, "cmptmp");
                        }
                        else
                        {
                            resultingValue = _builder.BuildFCmp(LLVMRealPredicate.LLVMRealUGT, lhsValue, rhsValue, "cmptmp");

                        }
                        break;
                    }
                case ExpressionType.GreaterThanEqual:
                    {
                        if (lhsAndRhsBothIntegers)
                        {
                            resultingValue = _builder.BuildICmp(LLVMIntPredicate.LLVMIntSGE, lhsValue, rhsValue, "cmptmp");
                        }
                        else
                        {
                            resultingValue = _builder.BuildFCmp(LLVMRealPredicate.LLVMRealUGE, lhsValue, rhsValue, "cmptmp");
                        }
                        break;
                    }
                case ExpressionType.LessThan:
                    {
                        if (lhsAndRhsBothIntegers)
                        {
                            resultingValue = _builder.BuildICmp(LLVMIntPredicate.LLVMIntSLT, lhsValue, rhsValue, "cmptmp");
                        }
                        else
                        {
                            resultingValue = _builder.BuildFCmp(LLVMRealPredicate.LLVMRealULT, lhsValue, rhsValue, "cmptmp");
                        }
                        break;
                    }
                case ExpressionType.LessThanEqual:
                    {
                        if (lhsAndRhsBothIntegers)
                        {
                            resultingValue = _builder.BuildICmp(LLVMIntPredicate.LLVMIntSLE, lhsValue, rhsValue, "cmptmp");
                        }
                        else
                        {
                            resultingValue = _builder.BuildFCmp(LLVMRealPredicate.LLVMRealULE, lhsValue, rhsValue, "cmptmp");
                        }
                        break;
                    }
                case ExpressionType.LogicalOr:
                    {
                        resultingValue = _builder.BuildOr(lhsValue, rhsValue, "LogicalOrTmp");
                        break;
                    }
                case ExpressionType.LogicalXOr:
                    {
                        resultingValue = _builder.BuildXor(lhsValue, rhsValue, "LogicalXOrTmp");
                        break;
                    }
                case ExpressionType.LogicalAnd:
                    {
                        resultingValue = _builder.BuildAnd(lhsValue, rhsValue, "LogicalAndTmp");
                        break;
                    }
                case ExpressionType.ConditionalOr:
                    {
                        resultingValue = _builder.BuildOr(lhsValue, rhsValue, "ConditionalOrTmp");
                        break;
                    }
                case ExpressionType.ConditionalAnd:
                    {
                        resultingValue = _builder.BuildAnd(lhsValue, rhsValue, "ConditionalOrTmp");
                        break;
                    }
                case ExpressionType.BitShiftLeft:
                    {
                        resultingValue = _builder.BuildShl(lhsValue, rhsValue, "BitShiftLeftTmp");
                        break;
                    }
                case ExpressionType.BitShiftRight:
                    {
                        resultingValue = _builder.BuildLShr(lhsValue, rhsValue, "BitShiftRightTmp");
                        break;
                    }
                default:
                    {
                        throw new ArgumentException("invalid binary operator");
                    }
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

        public void VisitBodyExpression(BodyExpression expression)
        {
            // BodyExpression should probably have a function name attached and some more info of the parent e.g. token type?
            // if this body isn't valid then we should remove the function all together.. and throw an error.
            // todo: do we need to do anything here?
        }

        public void VisitWhileStatementExpression(WhileStatementExpression expression)
        {
            throw new NotImplementedException();
        }

        public void VisitReturnExpression(ReturnExpression expression)
        {
            //todo: do we need to do something here?
        }
    }
}
