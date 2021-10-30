﻿using Lexing;
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
            _valueStack.Push(LLVM.ConstReal(LLVM.Int1Type(), expression.Value ? 1 : 0));
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
            _valueStack.Push(LLVM.ConstReal(LLVM.Int16Type(), (int)expression.Value));
        }

        public void VisitStringExpression(StringExpression expression)
        {
            _valueStack.Push(LLVM.ConstRealOfStringAndSize(LLVM.Int16Type(), expression.Value, (uint)expression.Value.Length));
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

            var call = LLVM.BuildCall(_builder, calleeFunction, argumentValues, "calltmp");
            _valueStack.Push(call);
        }

        public void VisitFunctionDefinitionExpression(FunctionDefinitionExpression expression)
        {
            Debug.Assert(expression?.Token is not null);

            var argumentCount = (uint)expression.Arguments.Length;
            var arguments = new LLVMTypeRef[argumentCount];
            var expressionName = expression.Token.Name;

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
                    arguments[i] = GetReturnType(expression.Arguments[i].TypeToken.TokenType);
                }
                var retType = GetReturnType(expression.ReturnTypeToken.TokenType);
                function = LLVM.AddFunction(_module, expressionName, LLVM.FunctionType(retType, arguments, LLVMBoolFalse));
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
            LLVM.PositionBuilderAtEnd(_builder, LLVM.AppendBasicBlock(function, "entry")); // this in combination with specifying /entry:Main causes an .exe to be able to be build.

            //todo: implement visit body and add it to the function? So actual code can be run
            if (expression.ReturnTypeToken.TokenType is TokenType.Void)
            {
                LLVM.BuildRetVoid(_builder);
            }
            else
            {
                var retValue = _valueStack.Pop();
                // below code is required for integers... why does LLVM turn ints into FPS?
                var newVal = LLVM.BuildCast(_builder, LLVMOpcode.LLVMRet, retValue, GetReturnType(expression.ReturnTypeToken.TokenType), "tmpCast");
                LLVM.BuildRet(_builder, newVal); // todo: support return types...?
            }

            LLVM.VerifyFunction(function, LLVMVerifierFailureAction.LLVMPrintMessageAction);
            _valueStack.Push(function);
        }

        private static LLVMTypeRef GetReturnType(TokenType tokenType)
        {
            switch (tokenType)
            {
                case TokenType.Float:
                    return LLVM.FloatType();
                case TokenType.Double:
                    return LLVM.DoubleType();
                case TokenType.Boolean:
                    return LLVM.Int1Type();
                case TokenType.Integer:
                    return LLVM.Int64Type();
                case TokenType.Character:
                    return LLVM.Int16Type();
                case TokenType.String:
                    return LLVM.Int16Type(); // should be an array i think ?
                case TokenType.DateTime:
                    throw new NotImplementedException();
                case TokenType.Void:
                    return LLVM.VoidType();
                default:
                    throw new InvalidOperationException($"TokenType {tokenType} is not supported as a return type for LLVM.");

            }
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
                    {
                        resultingValue = LLVM.BuildFAdd(_builder, lhsValue, rhsValue, "addtmp");
                        break;
                    }
                case ExpressionType.Subtract:
                    {
                        resultingValue = LLVM.BuildFSub(_builder, lhsValue, rhsValue, "subtmp");
                        break;
                    }
                case ExpressionType.Multiply:
                    {
                        resultingValue = LLVM.BuildFMul(_builder, lhsValue, rhsValue, "multmp");
                        break;
                    }
                case ExpressionType.Divide:
                    {
                        resultingValue = LLVM.BuildFDiv(_builder, lhsValue, rhsValue, "divtmp");
                        break;
                    }
                case ExpressionType.Equivalent: //todo: actually make this do a type compare? 
                    {
                        resultingValue = LLVM.BuildFCmp(_builder, LLVMRealPredicate.LLVMRealUEQ, lhsValue, rhsValue, "cmptmp");
                        break;
                    }
                case ExpressionType.Equals:
                    {
                        resultingValue = LLVM.BuildFCmp(_builder, LLVMRealPredicate.LLVMRealUEQ, lhsValue, rhsValue, "cmptmp");
                        break;
                    }
                case ExpressionType.GreaterThan:
                    {
                        resultingValue = LLVM.BuildFCmp(_builder, LLVMRealPredicate.LLVMRealUGT, lhsValue, rhsValue, "cmptmp");
                        break;
                    }
                case ExpressionType.GreaterThanEqual:
                    {
                        resultingValue = LLVM.BuildFCmp(_builder, LLVMRealPredicate.LLVMRealUGE, lhsValue, rhsValue, "cmptmp");
                        break;
                    }
                case ExpressionType.LessThan:
                    {
                        resultingValue = LLVM.BuildFCmp(_builder, LLVMRealPredicate.LLVMRealULT, lhsValue, rhsValue, "cmptmp");
                        break;
                    }
                case ExpressionType.LessThanEqual:
                    {
                        resultingValue = LLVM.BuildFCmp(_builder, LLVMRealPredicate.LLVMRealULE, lhsValue, rhsValue, "cmptmp");
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
