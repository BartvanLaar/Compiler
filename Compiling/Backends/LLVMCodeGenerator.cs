using Lexing;
using LLVMSharp.Interop;
using Parsing;
using Parsing.AbstractSyntaxTree.Expressions;
using Parsing.AbstractSyntaxTree.Visitors;
using System.Diagnostics;

namespace Compiling.Backends
{
    internal class LLVMCodeGenerator : IAbstractSyntaxTreeVisitor
    {
        private static readonly LLVMValueRef NullValue = new(IntPtr.Zero);
        private static readonly LLVMValueRef ZeroValue = LLVMValueRef.CreateConstInt(LLVMTypeRef.Int64, 0ul, true);
        private readonly LLVMModuleRef _module;

        private readonly LLVMBuilderRef _builder;
        private readonly LLVMExecutionEngineRef _executionEngine;
        private readonly LLVMPassManagerRef _passManager;
        private readonly Dictionary<string, LLVMValueRef> _valueAllocationPointers = new();
        private readonly Dictionary<string, string[]> _userDefinedTypes = new();
        private readonly Stack<LLVMValueRef> _valueStack = new();
        private List<IScope> _scopes = new ();

        //LLVM.LoadLibraryPermanently() // should i use this to load a c lib for printing to consoles?
        public LLVMCodeGenerator(LLVMModuleRef module, LLVMBuilderRef builder, LLVMExecutionEngineRef executionEngine, LLVMPassManagerRef passManager)
        {
            _module = module;
            _builder = builder;
            _executionEngine = executionEngine;
            _passManager = passManager;

            // hack below.. Some global constant value needs to be set in order to use doubles or floats...
            // its either use this, or use clang for compilation from bc -> exe, but this takes more than 2 sec?! and secretly includes more than just the written code.
            var glob = _module.AddGlobal(LLVMTypeRef.Int1, "_fltused");
            glob.IsGlobalConstant = true;
            glob.Initializer = LLVMValueRef.CreateConstInt(LLVMTypeRef.Int1, 1);

            var structPtr = LLVMTypeRef.CreatePointer(LLVMTypeRef.Int8, 0);
            var stringStruct = _module.Context.CreateNamedStruct("class.string");
            stringStruct.StructSetBody(new[] { structPtr, LLVMTypeRef.Int64 }, true);
            _userDefinedTypes.Add("class.string", new[] { "Ptr", "Length" });
            //var ptr = LLVMTypeRef.CreatePointer(LLVMTypeRef.Int8, 0);
            //var printf = LLVMTypeRef.CreateFunction(LLVMTypeRef.Int64,  new[] { ptr, ptr }, false);
            //_module.AddFunction("printf", printf);
        }

        public LLVMCodeGenerator()
        {
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
                        _valueStack.Push(LLVMValueRef.CreateConstInt(LLVMTypeRef.Int1, ((bool)expression.Value ? 1ul : 0ul), false));
                        return;
                    }
                case TypeIndicator.Integer:
                    {
                        var val = LLVMValueRef.CreateConstInt(LLVMTypeRef.Int64, (ulong)Convert.ChangeType(expression.Value, typeof(ulong)), true);
                        if (expression.IsNegative) // hacky but works... and is probably optimized out anyway...
                        {
                            var sub = _builder.BuildSub(ZeroValue, val); // BuildNSWSub? should we handle overflows?
                            _valueStack.Push(sub);
                            return;
                        }
                        _valueStack.Push(val);
                        return;
                    }
                case TypeIndicator.Double:
                    {
                        var value = expression.IsNegative ? -(double)expression.Value : (double)expression.Value;
                        _valueStack.Push(LLVMValueRef.CreateConstReal(LLVMTypeRef.Double, value));
                        return;
                    }
                case TypeIndicator.Float:
                    {
                        var value = expression.IsNegative ? -(float)expression.Value : (float)expression.Value;

                        _valueStack.Push(LLVMValueRef.CreateConstReal(LLVMTypeRef.Float, value));
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
                        LLVMValueRef stringValue = CreateStringValue((string)expression.Value);
                        _valueStack.Push(stringValue);
                        return;
                    }
                default: throw new Exception($"Visitted value {expression.Value} of type {expression.Token.TypeIndicator} which is not supported by the VisitValueExpression!");
            }
        }

        private LLVMValueRef CreateStringValue(string value)
        {
            //todo: learn how to access struct members...

            var strPtr = _builder.BuildGlobalStringPtr(value);
            strPtr.Name = "Ptr";
            var length = LLVMValueRef.CreateConstInt(LLVMTypeRef.Int64, (ulong)value.Length, true);
            length.Name = "Length";
            var strType = _module.GetTypeByName("class.string");
            var test = LLVMValueRef.CreateConstNamedStruct(strType, new[] { strPtr, length });
            return test;
        }

        public void VisitIdentifierExpression(IdentifierExpression expression)
        {
            if (!_valueAllocationPointers.TryGetValue(expression.Identifier, out var alloca))
            {
                throw new ArgumentException($"Unknown variable name {expression.Identifier}");
            }
            _valueStack.Push(_builder.BuildLoad(alloca));
        }

        public void VisitMemberAccessExpression(MemberAccessExpression expression)
        {
            var alloca = _valueAllocationPointers[expression.ParentToken.Name];
            var typeName = alloca.TypeOf.ElementType.StructName;
            var index = (ulong)Array.IndexOf(_userDefinedTypes[typeName], expression.MemberToken.Name);

            var memberPtr = _builder.BuildGEP(alloca, new[] { LLVMValueRef.CreateConstInt(LLVMTypeRef.Int32, index, true) }, "test");
            var load = _builder.BuildLoad(memberPtr);
            _valueStack.Push(load);
        }

        private LLVMTypeRef GetReturnType(TypeIndicator typeIndicator)
        {
            return typeIndicator switch
            {
                TypeIndicator.Float => LLVMTypeRef.Float,
                TypeIndicator.Double => LLVMTypeRef.Double,
                TypeIndicator.Boolean => LLVMTypeRef.Int1,
                TypeIndicator.Integer => LLVMTypeRef.Int64,
                TypeIndicator.Character => LLVMTypeRef.Int16,
                TypeIndicator.String => _module.GetTypeByName("class.string"),// should be an array i think ?
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

            switch (expression.Token.TokenType)
            {
                case TokenType.Assignment:
                    {
                        //todo: refactor the code so all assignments end here.. e.g. += -= *=, etc...?
                        Debug.Assert(_valueAllocationPointers.ContainsKey(((IdentifierExpression)expression.LeftHandSide).Identifier));
                        // assert will fail in case of e.g. 50 += 5;
                        // this is ok though as thats not allowed and should be handled by type checking... But isn't for now..
                        var alloca = _valueAllocationPointers[((IdentifierExpression)expression.LeftHandSide).Identifier];
                        _builder.BuildStore(rhsValue, alloca);
                        break;
                    }
                case TokenType.Add:
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
                case TokenType.Subtract:
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
                case TokenType.Multiply:
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
                case TokenType.Divide:
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
                case TokenType.Modulo:
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
                case TokenType.Equivalent: //todo: actually make this do a type compare? 
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
                case TokenType.Equals:
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
                case TokenType.NotEquivalent:
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
                case TokenType.NotEquals:
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
                case TokenType.GreaterThan:
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
                case TokenType.GreaterThanEqual:
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
                case TokenType.LessThan:
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
                case TokenType.LessThanEqual:
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
                case TokenType.BitwiseOr:
                    {
                        _valueStack.Push(_builder.BuildOr(lhsValue, rhsValue));
                        break;
                    }
                case TokenType.BitwiseAnd:
                    {
                        _valueStack.Push(_builder.BuildAnd(lhsValue, rhsValue));
                        break;
                    }
                case TokenType.ConditionalOr:
                    {
                        _valueStack.Push(_builder.BuildOr(lhsValue, rhsValue));
                        break;
                    }
                case TokenType.ConditionalXOr:
                    {
                        _valueStack.Push(_builder.BuildOr(lhsValue, rhsValue));
                        break;
                    }
                case TokenType.ConditionalAnd:
                    {
                        _valueStack.Push(_builder.BuildAnd(lhsValue, rhsValue));
                        break;
                    }
                case TokenType.BitShiftLeft:
                    {
                        _valueStack.Push(_builder.BuildShl(lhsValue, rhsValue));
                        break;
                    }
                case TokenType.BitShiftRight:
                    {
                        _valueStack.Push(_builder.BuildLShr(lhsValue, rhsValue));
                        break;
                    }
                default:
                    {
                        throw new ArgumentException($"Invalid binary operator: '{expression.Token.TokenType}'.");
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
            if (function.ParamsCount != argumentCount) // todo: fis so it works for varargs...
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
            var call = _builder.BuildCall(function, argumentValues.ToArray(), isVoidFunc ? string.Empty : $"callTemp");
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

            for (int i = 0; i < argumentCount; ++i)
            {
                arguments[i] = GetReturnType(expression.Arguments[i].TypeToken.TypeIndicator);
            }

            var retType = GetReturnType(expression.ReturnTypeToken.TypeIndicator);
            function = _module.AddFunction(functionName, LLVMTypeRef.CreateFunction(retType, arguments, false));

            function.Linkage = LLVMLinkage.LLVMExternalLinkage;

            if (expression.IsExtern)
            {
                function.FunctionCallConv = (uint)LLVMCallConv.LLVMCCallConv;
                Debug.Assert(expression.Body is null);
                function.VerifyFunction(LLVMVerifierFailureAction.LLVMPrintMessageAction);
                _passManager.RunFunctionPassManager(function);
                return;
            }

            var entryBB = function.AppendBasicBlock("entry");
            _builder.PositionAtEnd(entryBB);
            _valueAllocationPointers.Clear();

            for (int i = 0; i < argumentCount; ++i)
            {
                Debug.Assert(!string.IsNullOrWhiteSpace(expression?.Arguments[i].ValueToken.Name));
                var argumentName = expression.Arguments[i].ValueToken.Name;

                LLVMValueRef param = function.GetParam((uint)i);
                param.Name = argumentName;

                //todo: is this requried for a func?
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

        public void Initialize(List<IScope> scopes)
        {
            _scopes = scopes;
        }

        public void VisitContextExpression(ContextDefinitionExpression expression)
        {
            foreach (var context in expression.Contexts)
            {
                Visit(context);
            }

            foreach (var @enum in expression.Enums)
            {
                Visit(@enum);
            }

            foreach (var @class in expression.Classes)
            {
                Visit(@class);
            }
        }

        public void VisitClassExpression(ClassDefinitionExpression expression)
        {
            //todo: create struct?
            foreach (var @enum in expression.Enums)
            {
                Visit(@enum);
            }

            foreach (var @class in expression.Classes)
            {
                Visit(@class);
            }

            foreach (var variable in expression.Variables)
            {
                Visit(variable);
            }

            foreach (var function in expression.Functions)
            {
                Visit(function);
            }
        }

        public void VisitImportExpression(ImportStatementExpression expression)
        {

        }

        public void VisitEnumExpression(EnumDefinitionExpression expression)
        {
            throw new NotImplementedException();
        }

        public void VisitForeachStatementExpression(ForeachStatementExpression expression)
        {
            throw new NotImplementedException();
        }

        public void VisitSwitchStatementExpression(SwitchStatementExpression expression)
        {
            throw new NotImplementedException();
        }

        public void VisitObjectInstantiationExpression(ObjectInstantiationExpression expression)
        {
            throw new NotImplementedException();
        }
    }
}
