using Lexing;
using LLVMSharp.Interop;
using Parsing.AbstractSyntaxTree.Expressions;
using Parsing.AbstractSyntaxTree.Visitors;
using System;
using System.Diagnostics;

namespace Compiling.Backends
{
    internal class LLVMCodeGenerator : IAbstractSyntaxTreeVisitor
    {
        private static readonly LLVMValueRef NullValue = new(IntPtr.Zero);

        private readonly LLVMModuleRef _module;

        private readonly LLVMBuilderRef _builder;
        private readonly LLVMExecutionEngineRef _executionEngine;
        private readonly LLVMPassManagerRef _passManager;
        private readonly Dictionary<string, LLVMValueRef> _valueAllocationPointers = new();
        private delegate double D_FUNCTION_PTR(); // temp?

        private readonly Stack<LLVMValueRef> _valueStack = new();

        //LLVM.LoadLibraryPermanently() // should i use this to load a c lib for printing to consoles?
        public LLVMCodeGenerator(LLVMModuleRef module, LLVMBuilderRef builder, LLVMExecutionEngineRef executionEngine, LLVMPassManagerRef passManager)
        {
            _module = module;
            _builder = builder;
            _executionEngine = executionEngine;
            _passManager = passManager;

            //var ptr = LLVMTypeRef.CreatePointer(LLVMTypeRef.Int8, 0);
            var funcType = LLVMTypeRef.CreateFunction(LLVMTypeRef.Void, new[] { LLVMTypeRef.Int64 }, true);

            var function = _module.AddFunction("printf", funcType);
            function.Linkage = LLVMLinkage.LLVMExternalLinkage;
            function.FunctionCallConv = (uint)LLVMCallConv.LLVMCCallConv;
            //var type = LLVMTypeRef.CreateFunction(LLVMTypeRef.Void, new[] { LLVMTypeRef.Int32, LLVMTypeRef.CreatePointer(LLVMTypeRef.Int8, 0) }, true);
            //var func = _module.AddFunction("printf", type);
            //func.Linkage = LLVMLinkage.LLVMDLLImportLinkage;

            // hack below.. Some global constant value needs to be set in order to use doubles or floats...
            // its either use this, or use clang for compilation from bc -> exe, but this takes more than 2 sec?! and secretly includes more than just the written code.
            //_builder.BuildCall(function, new[] { LLVMValueRef.CreateConstInt(LLVMTypeRef.Int32, 32, true) }, $"callPrintFTemp");
            var glob = _module.AddGlobal(LLVMTypeRef.Int1, "_fltused");
            glob.Initializer = LLVMValueRef.CreateConstInt(LLVMTypeRef.Int1, 1);
        }

        public string Name => "LLVM backend";

        public void Visit(ExpressionBase? expression) => AbstractSyntaxTreeVisitor.Visit(this, expression);

        public void VisitVariableDeclarationExpression(VariableDeclarationExpression expression)
        {
            Visit(expression.ValueExpression);
            var rhsValue = _valueStack.Pop();
            Debug.Assert(expression.Identifier is not null);
            if (_valueAllocationPointers.ContainsKey(expression.Identifier))
            {
                throw new ArgumentException($"Redeclaration of {expression.Identifier}! Scopes are not yet supported! Don't re-use variable names!");
            }
            var alloca = CreateEntryBlockAlloca(rhsValue.TypeOf, expression.Identifier);
            _builder.BuildStore(rhsValue, alloca);
            _valueAllocationPointers.Add(expression.Identifier, alloca);
        }


        public void VisitValueExpression(ValueExpression expression)
        {
            Debug.Assert(expression.Token is not null);
            switch (expression.Token.TypeIndicator)
            {
                case TypeIndicator.Boolean:
                    {
                        _valueStack.Push(LLVMValueRef.CreateConstInt(LLVMTypeRef.Int1, (ulong)((bool)expression.Value ? 1 : 0), false));
                        return;
                    }
                case TypeIndicator.Integer:
                    {
                        _valueStack.Push(LLVMValueRef.CreateConstInt(LLVMTypeRef.Int64, (ulong)Convert.ChangeType((int)expression.Value, typeof(ulong)), true));
                        return;
                    }

                case TypeIndicator.Double:
                    {
                        _valueStack.Push(LLVMValueRef.CreateConstReal(LLVMTypeRef.Double, (double)expression.Value));
                        return;
                    }

                case TypeIndicator.Float:
                    {
                        _valueStack.Push(LLVMValueRef.CreateConstReal(LLVMTypeRef.Float, (float)expression.Value));
                        return;
                    }

                case TypeIndicator.Character:
                    {
                        Debug.Assert(expression.Value is not null);
                        _valueStack.Push(LLVMValueRef.CreateConstInt(LLVMTypeRef.Int16, (ulong)((string)expression.Value).First(), false));
                        return;
                    }
                case TypeIndicator.String:
                    {
                        _valueStack.Push(LLVMValueRef.CreateConstRealOfStringAndSize(LLVMTypeRef.CreatePointer(LLVMTypeRef.Int16, 0), (string)expression.Value, (uint)((string)expression.Value).Length));
                        return;
                    }
                default: throw new Exception($"Visitted value {expression.Value} of type {expression.Token.TypeIndicator} which is not supported by the VisitValueExpression!");
            }
        }

        public void VisitIdentifierExpression(IdentifierExpression expression)
        {
            if (!_valueAllocationPointers.TryGetValue(expression.Identifier, out var alloca))
            {
                throw new ArgumentException($"Unknown variable name {expression.Identifier}");
            }
            _valueStack.Push(_builder.BuildLoad(alloca));
        }

        private static LLVMTypeRef GetReturnType(TypeIndicator typeIndicator)
        {
            return typeIndicator switch
            {
                TypeIndicator.Float => LLVMTypeRef.Float,
                TypeIndicator.Double => LLVMTypeRef.Double,
                TypeIndicator.Boolean => LLVMTypeRef.Int1,
                TypeIndicator.Integer => LLVMTypeRef.Int64,
                TypeIndicator.Character => LLVMTypeRef.Int16,
                TypeIndicator.String => LLVMTypeRef.Int16,// should be an array i think ?
                TypeIndicator.DateTime => throw new NotImplementedException(),
                TypeIndicator.Void => LLVMTypeRef.Void,
                _ => throw new InvalidOperationException($"TypeIndicator {typeIndicator} is not supported as a return type for LLVM."),
            };
        }

        public void VisitBinaryExpression(BinaryExpression expression)
        {
            Visit(expression.LeftHandSide);
            var lhsValue = _valueStack.Pop();

            Visit(expression.RightHandSide);
            var hasRhsValue = _valueStack.TryPop(out var rhsValue);
            if (!hasRhsValue)
            {
                return;
            }

            var lhsAndRhsBothIntegers = rhsValue.TypeOf.Kind is LLVMTypeKind.LLVMIntegerTypeKind && lhsValue.TypeOf.Kind is LLVMTypeKind.LLVMIntegerTypeKind;

            switch (expression.NodeExpressionType)
            {
                case ExpressionType.Assignment:
                    {
                        //todo: refactor the code so all assignments end here.. e.g. += -= *=, etc...?
                        Debug.Assert(_valueAllocationPointers.ContainsKey(((IdentifierExpression)expression.LeftHandSide).Identifier));
                        // assert will fail in case of e.g. 50 += 5;
                        // this is ok though as thats not allowed and should be handled by type checking... But isn't for now..
                        var alloca = _valueAllocationPointers[((IdentifierExpression)expression.LeftHandSide).Identifier];
                        _builder.BuildStore(rhsValue, alloca);
                        break;
                    }
                case ExpressionType.Add:
                    {
                        if (lhsAndRhsBothIntegers)
                        {
                            _valueStack.Push(_builder.BuildAdd(lhsValue, rhsValue));
                        }
                        else
                        {
                            _valueStack.Push(_builder.BuildFAdd(lhsValue, rhsValue));
                        }
                        break;
                    }
                case ExpressionType.Subtract:
                    {
                        if (lhsAndRhsBothIntegers)
                        {
                            _valueStack.Push(_builder.BuildSub(lhsValue, rhsValue));
                        }
                        else
                        {
                            _valueStack.Push(_builder.BuildFSub(lhsValue, rhsValue));
                        }
                        break;
                    }
                case ExpressionType.Multiply:
                    {
                        if (lhsAndRhsBothIntegers)
                        {
                            _valueStack.Push(_builder.BuildMul(lhsValue, rhsValue));
                        }
                        else
                        {
                            _valueStack.Push(_builder.BuildFMul(lhsValue, rhsValue));
                        }
                        break;
                    }
                case ExpressionType.Divide:
                    {
                        if (lhsAndRhsBothIntegers)
                        {
                            _valueStack.Push(_builder.BuildSDiv(lhsValue, rhsValue));
                        }
                        else
                        {
                            _valueStack.Push(_builder.BuildFDiv(lhsValue, rhsValue));
                        }
                        break;
                    }
                case ExpressionType.Modulo:
                    {
                        if (lhsAndRhsBothIntegers)
                        {
                            _valueStack.Push(_builder.BuildSRem(lhsValue, rhsValue));
                        }
                        else
                        {
                            _valueStack.Push(_builder.BuildFRem(lhsValue, rhsValue));
                        }
                        break;
                    }
                case ExpressionType.Equivalent: //todo: actually make this do a type compare? 
                    {
                        if (lhsAndRhsBothIntegers)
                        {
                            _valueStack.Push(_builder.BuildICmp(LLVMIntPredicate.LLVMIntEQ, lhsValue, rhsValue));
                        }
                        else
                        {
                            _valueStack.Push(_builder.BuildFCmp(LLVMRealPredicate.LLVMRealUEQ, lhsValue, rhsValue));
                        }
                        break;
                    }
                case ExpressionType.Equals:
                    {
                        if (lhsAndRhsBothIntegers)
                        {
                            _valueStack.Push(_builder.BuildICmp(LLVMIntPredicate.LLVMIntEQ, lhsValue, rhsValue));
                        }
                        else
                        {
                            _valueStack.Push(_builder.BuildFCmp(LLVMRealPredicate.LLVMRealUEQ, lhsValue, rhsValue));
                        }
                        break;
                    }
                case ExpressionType.NotEquivalent:
                    {
                        if (lhsAndRhsBothIntegers)
                        {
                            _valueStack.Push(_builder.BuildICmp(LLVMIntPredicate.LLVMIntNE, lhsValue, rhsValue));
                        }
                        else
                        {
                            _valueStack.Push(_builder.BuildFCmp(LLVMRealPredicate.LLVMRealUNE, lhsValue, rhsValue));
                        }
                        break;
                    }
                case ExpressionType.NotEquals:
                    {
                        if (lhsAndRhsBothIntegers)
                        {
                            _valueStack.Push(_builder.BuildICmp(LLVMIntPredicate.LLVMIntNE, lhsValue, rhsValue));
                        }
                        else
                        {
                            _valueStack.Push(_builder.BuildFCmp(LLVMRealPredicate.LLVMRealUNE, lhsValue, rhsValue));
                        }
                        break;
                    }
                case ExpressionType.GreaterThan:
                    {
                        if (lhsAndRhsBothIntegers)
                        {
                            _valueStack.Push(_builder.BuildICmp(LLVMIntPredicate.LLVMIntSGT, lhsValue, rhsValue));
                        }
                        else
                        {
                            _valueStack.Push(_builder.BuildFCmp(LLVMRealPredicate.LLVMRealUGT, lhsValue, rhsValue));

                        }
                        break;
                    }
                case ExpressionType.GreaterThanEqual:
                    {
                        if (lhsAndRhsBothIntegers)
                        {
                            _valueStack.Push(_builder.BuildICmp(LLVMIntPredicate.LLVMIntSGE, lhsValue, rhsValue));
                        }
                        else
                        {
                            _valueStack.Push(_builder.BuildFCmp(LLVMRealPredicate.LLVMRealUGE, lhsValue, rhsValue));
                        }
                        break;
                    }
                case ExpressionType.LessThan:
                    {
                        if (lhsAndRhsBothIntegers)
                        {
                            _valueStack.Push(_builder.BuildICmp(LLVMIntPredicate.LLVMIntSLT, lhsValue, rhsValue));
                        }
                        else
                        {
                            _valueStack.Push(_builder.BuildFCmp(LLVMRealPredicate.LLVMRealULT, lhsValue, rhsValue));
                        }
                        break;
                    }
                case ExpressionType.LessThanEqual:
                    {
                        if (lhsAndRhsBothIntegers)
                        {
                            _valueStack.Push(_builder.BuildICmp(LLVMIntPredicate.LLVMIntSLE, lhsValue, rhsValue));
                        }
                        else
                        {
                            _valueStack.Push(_builder.BuildFCmp(LLVMRealPredicate.LLVMRealULE, lhsValue, rhsValue));
                        }
                        break;
                    }
                case ExpressionType.BitwiseOr:
                    {
                        _valueStack.Push(_builder.BuildOr(lhsValue, rhsValue));
                        break;
                    }
                case ExpressionType.BitwiseAnd:
                    {
                        _valueStack.Push(_builder.BuildAnd(lhsValue, rhsValue));
                        break;
                    }
                case ExpressionType.ConditionalOr:
                    {
                        _valueStack.Push(_builder.BuildOr(lhsValue, rhsValue));
                        break;
                    }
                case ExpressionType.ConditionalXOr:
                    {
                        _valueStack.Push(_builder.BuildOr(lhsValue, rhsValue));
                        break;
                    }
                case ExpressionType.ConditionalAnd:
                    {
                        _valueStack.Push(_builder.BuildAnd(lhsValue, rhsValue));
                        break;
                    }
                case ExpressionType.BitShiftLeft:
                    {
                        _valueStack.Push(_builder.BuildShl(lhsValue, rhsValue));
                        break;
                    }
                case ExpressionType.BitShiftRight:
                    {
                        _valueStack.Push(_builder.BuildLShr(lhsValue, rhsValue));
                        break;
                    }
                default:
                    {
                        throw new ArgumentException($"Invalid binary operator: '{expression.NodeExpressionType}'.");
                    }
            }


        }

        public void VisitIfStatementExpression(IfStatementExpression expression)
        {
            Visit(expression.IfCondition);
            var condv = _valueStack.Pop();
            var parentFuncBlock = _builder.InsertBlock.Parent;
            var headerBB = parentFuncBlock.AppendBasicBlock("headerBB");
            var ifBodyBB = parentFuncBlock.AppendBasicBlock("if");
            var elseBB = parentFuncBlock.AppendBasicBlock("else");

            _builder.BuildBr(headerBB);
            _builder.PositionAtEnd(headerBB);

            _builder.BuildCondBr(condv, ifBodyBB, elseBB);

            // set insertion point to ifBody basic block before visiting...
            _builder.PositionAtEnd(ifBodyBB);
            Visit(expression.IfBody);
            ifBodyBB = _builder.InsertBlock;
            LLVMBasicBlockRef? mergeBB = null;
            var hasIfTerminator = ifBodyBB.Terminator != NullValue;
            if (!hasIfTerminator)
            {
                mergeBB = parentFuncBlock.AppendBasicBlock("afterIf");

                _builder.BuildBr(mergeBB.Value);
            }

            // set insertion point to elseBody basic block before visiting
            _builder.PositionAtEnd(elseBB);
            Visit(expression.ElseBody);
            elseBB = _builder.InsertBlock;
            var hasElseTerminator = elseBB.Terminator != NullValue;
            if (!hasElseTerminator)
            {
                mergeBB ??= parentFuncBlock.AppendBasicBlock("afterIf");
                _builder.BuildBr(mergeBB.Value);
            }

            if (mergeBB.HasValue)
            {
                _builder.PositionAtEnd(mergeBB.Value);
            }
        }

        private LLVMValueRef CreateEntryBlockAlloca(LLVMTypeRef typeRef, string variableName)
        {
            var currentPos = _builder.InsertBlock;

            var function = _builder.InsertBlock.Parent.EntryBasicBlock;
            //todo: make sure the alloca is added to the entry block of the function...? and make sure that this works
            _builder.PositionAtEnd(function);
            var alloca = _builder.BuildAlloca(typeRef, variableName);
            _builder.PositionAtEnd(currentPos);
            return alloca;
        }

        public void VisitForStatementExpression(ForStatementExpression expression)
        {
            // save (shadowed) outer scope variable pointer if it exists.          
            _valueAllocationPointers.TryGetValue(expression.VariableName, out var oldValueAlloca);
            Visit(expression.VariableDeclaration); // visit var decl outside loop scope.

            var function = _builder.InsertBlock.Parent;
            var loopHeaderBB = function.AppendBasicBlock("loopHeader");
            var loopBodyBB = function.AppendBasicBlock("loopBody");
            var afterLoopBB = function.AppendBasicBlock("afterLoop"); // aka mergeBB

            _builder.BuildBr(loopHeaderBB); // Insert an explicit fall through from the current block to the LoopHeaderBB.
            _builder.PositionAtEnd(loopHeaderBB); // Start insertion in LoopBB.
            Visit(expression.Condition);
            var endCondition = _valueStack.Pop();
            _builder.BuildCondBr(endCondition, loopBodyBB, afterLoopBB);

            _builder.PositionAtEnd(loopBodyBB); // Start insertion in LoopBodyBB.
            Visit(expression.Body);
            Visit(expression.VariableIncreaseExpression); // visiting increases the value allocated to the variable.

            _builder.BuildBr(loopHeaderBB);

            _builder.PositionAtEnd(afterLoopBB); // set insertion point to afterLoopBB

            // Restore the unshadowed variable.
            if (oldValueAlloca.Handle != IntPtr.Zero)
            {
                _valueAllocationPointers[expression.VariableName] = oldValueAlloca;
            }
            else
            {
                _valueAllocationPointers.Remove(expression.VariableName);
            }
        }

        public void VisitBodyExpression(BodyExpression expression)
        {
            // BodyExpression should probably have a function name attached and some more info of the parent e.g. token type?
            // if this body isn't valid then we should remove the function all together.. and throw an error.
            foreach (var expr in expression.Body)
            {
                Visit(expr);
            }
        }

        public void VisitWhileStatementExpression(WhileStatementExpression expression)
        {
            var function = _builder.InsertBlock.Parent;
            var loopHeaderBB = function.AppendBasicBlock("loopHeader");
            var loopBodyBB = function.AppendBasicBlock("loop");
            var afterLoopBB = function.AppendBasicBlock("afterLoop");

            _builder.BuildBr(loopHeaderBB); // Insert an explicit fall through from the current block to the LoopHeaderBB.
            _builder.PositionAtEnd(loopHeaderBB); // Start insertion in LoopHeaderBB.
            Visit(expression.Condition);
            var endCondition = _valueStack.Pop();
            _builder.BuildCondBr(endCondition, loopBodyBB, afterLoopBB);
            _builder.PositionAtEnd(loopBodyBB); // Start insertion in LoopBodyBB.

            Visit(expression.Body);

            _builder.BuildBr(loopHeaderBB); // go back to header and check condition

            _builder.PositionAtEnd(afterLoopBB); // set insertion point to after the loop...
        }

        public void VisitDoWhileStatementExpression(DoWhileStatementExpression expression)
        {
            var function = _builder.InsertBlock.Parent;
            var loopBodyBB = function.AppendBasicBlock("loop");
            var loopFooterBB = function.AppendBasicBlock("loopFooter");
            var afterLoopBB = function.AppendBasicBlock("afterLoop");

            _builder.BuildBr(loopBodyBB);
            _builder.PositionAtEnd(loopBodyBB);
            Visit(expression.Body);

            _builder.BuildBr(loopFooterBB);
            _builder.PositionAtEnd(loopFooterBB);
            Visit(expression.Condition);
            var endCondition = _valueStack.Pop();
            _builder.BuildCondBr(endCondition, loopBodyBB, afterLoopBB);
            _builder.PositionAtEnd(afterLoopBB);
        }

        public void VisitFunctionCallExpression(FunctionCallExpression expression)
        {
            var function = _module.GetNamedFunction(expression.FunctionName);

            if (function.Handle == IntPtr.Zero)
            {
                throw new Exception($"Unknown function referenced: {expression.FunctionName}");
            }

            var argumentCount = expression.Arguments.Length;
            if (function.ParamsCount != argumentCount)
            {
                throw new Exception($"Incorrect # arguments passed to {expression.FunctionName}");
            }
            var argumentValues = new List<LLVMValueRef>();

            for (var i = 0; i < expression.Arguments.Length; i++)
            {
                Visit(expression.Arguments[i]);
                argumentValues.Add(_valueStack.Pop());
            }

            var isVoidFunc = function.TypeOf.ReturnType.Kind == LLVMTypeKind.LLVMVoidTypeKind;
            var call = _builder.BuildCall(function, argumentValues.ToArray() , isVoidFunc ? string.Empty:$"callTemp");
            _valueStack.Push(call);
        }

        public void VisitFunctionDefinitionExpression(FunctionDefinitionExpression expression)
        {
            var argumentCount = (uint)expression.Arguments.Length;
            var arguments = new LLVMTypeRef[argumentCount];
            var functionName = expression.FunctionName;

            var function = _module.GetNamedFunction(functionName);
            if (function.Handle != IntPtr.Zero)
            {
                if (function.BasicBlocksCount != 0)
                {
                    throw new Exception($"Redefinition of function :'{functionName}'");
                }

                if (function.ParamsCount != argumentCount)
                {
                    // function overloading should be dealth with on language level -> mangling function names
                    // see https://mapping-high-level-constructs-to-llvm-ir.readthedocs.io/en/latest/basic-constructs/functions.html#function-overloading
                    Debug.Assert(false, $"Redefinition of function :'{functionName}' with a different number of arguments.");
                }

                Debug.Assert(false, "Should not get here I think as a function always has a body declared when its not extern");
            }
            else
            {
                for (int i = 0; i < argumentCount; ++i)
                {
                    arguments[i] = GetReturnType(expression.Arguments[i].TypeToken.TypeIndicator);
                }
                var retType = GetReturnType(expression.ReturnTypeToken.TypeIndicator);
                function = _module.AddFunction(functionName, LLVMTypeRef.CreateFunction(retType, arguments, false));
                function.Linkage = LLVMLinkage.LLVMExternalLinkage;
            }

            var entryBB = function.AppendBasicBlock("entry");
            _builder.PositionAtEnd(entryBB);
            _valueAllocationPointers.Clear();

            for (int i = 0; i < argumentCount; ++i)
            {
                Debug.Assert(!string.IsNullOrWhiteSpace(expression?.Arguments[i].ValueToken.Name));
                var argumentName = expression.Arguments[i].ValueToken.Name;// todo: is this right?

                LLVMValueRef param = function.GetParam((uint)i);
                param.Name = argumentName;
                // is this requried for a func?
                var alloca = CreateEntryBlockAlloca(param.TypeOf, param.Name);
                _builder.BuildStore(param, alloca);
                _valueAllocationPointers[argumentName] = alloca;
            }

            if (expression.Body is not null)
            {
                try
                {
                    Visit(expression.Body);
                }
                catch (Exception)
                {
                    // should we remove the function if an error happens in the body?
                    throw;
                }
            }

            _valueAllocationPointers.Clear();

            if (_builder.InsertBlock.Terminator == NullValue)
            {
                if (expression.ReturnTypeToken.TypeIndicator == TypeIndicator.Void)
                {
                    _builder.BuildRetVoid();
                }
                else
                {
                    throw new Exception("Function does not return a value in all cases.");
                }
            }
            _valueStack.Clear();
            function.VerifyFunction(LLVMVerifierFailureAction.LLVMPrintMessageAction);
            _passManager.RunFunctionPassManager(function);
        }

        public void VisitReturnExpression(ReturnExpression expression)
        {
            Visit(expression.ReturnExpr);
            // kind of ugly.. But if it's an assignment expression, we need to visit the identExpr to put the result back on the stack.
            // Perhaps the code below is hiding a bug? Investigate further... ~BvL, 14-11-2021
            if (expression.ReturnExpr?.Token.TokenType is TokenType.Assignment)
            {
                Debug.Assert(expression.ReturnExpr is BinaryExpression bexp && bexp.LeftHandSide is IdentifierExpression);
                Visit(((BinaryExpression)expression.ReturnExpr).LeftHandSide);
            }

            if (_valueStack.Any())
            {
                _builder.BuildRet(_valueStack.Pop());
            }
            else
            {
                _builder.BuildRetVoid();
            }

        }

    }
}
