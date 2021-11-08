﻿using Lexing;
using LLVMSharp.Interop;
using Parsing.AbstractSyntaxTree.Expressions;
using Parsing.AbstractSyntaxTree.Visitors;
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
        private readonly Dictionary<string, LLVMValueRef> _namedValues = new();
        private delegate double D_FUNCTION_PTR(); // temp?

        private readonly Stack<LLVMValueRef> _valueStack = new();
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
            glob.Initializer = LLVMValueRef.CreateConstInt(LLVMTypeRef.Int1, 1, false);
        }

        public string Name => "LLVM backend";

        public void Visit(ExpressionBase? expression) => AbstractSyntaxTreeVisitor.Visit(this, expression);

        public void VisitVariableDeclarationExpression(VariableDeclarationExpression expression)
        {
            Visit(expression.ValueExpression);
            var rhsValue = _valueStack.Pop();
            var variableName = expression.Identifier;
            Debug.Assert(variableName is not null);
            if (_namedValues.ContainsKey(variableName))
            {
                throw new ArgumentException($"Redeclaration of {variableName}! Scopes are not yet supported! Don't re-use variable names!");
            }
            _namedValues.Add(variableName, rhsValue);
        }

        public void VisitBooleanExpression(BooleanExpression expression)
        {
            _valueStack.Push(LLVMValueRef.CreateConstInt(LLVMTypeRef.Int1, (ulong)(expression.Value ? 1 : 0)));
        }

        public void VisitIntegerExpression(IntegerExpression expression)
        {
            _valueStack.Push(LLVMValueRef.CreateConstInt(LLVMTypeRef.Int64, (ulong)expression.Value, true));
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
            if (expression.Value?.Length > 1)
            {
                throw new InvalidOperationException("Characters may only have a length of one");
            }

            Debug.Assert(expression.Value is not null);
            _valueStack.Push(LLVMValueRef.CreateConstInt(LLVMTypeRef.Int16, (ulong)expression.Value.First(), false));
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

            var argumentCount = expression.Arguments.Length;
            if (calleeFunction.ParamsCount != argumentCount)
            {
                throw new Exception($"Incorrect # arguments passed to {expression.FunctionName}");
            }

            foreach (var argument in expression.Arguments)
            {
                Visit(argument);
            }

            var argumentValues = new LLVMValueRef[argumentCount];
            for (int i = argumentCount - 1; i >= 0; i--) // shouldnt this be reverse?? last one on 
            {
                argumentValues[i] = _valueStack.Pop();
            }

            var call = _builder.BuildCall(calleeFunction, argumentValues);
            _valueStack.Push(call);
        }

        public void VisitFunctionDefinitionExpression(FunctionDefinitionExpression expression)
        {
            var argumentCount = (uint)expression.Arguments.Length;
            var arguments = new LLVMTypeRef[argumentCount];
            var expressionName = expression.FunctionName;

            var function = _module.GetNamedFunction(expressionName);
            if (function.Handle != IntPtr.Zero)
            {
                if (function.BasicBlocksCount != 0)
                {
                    throw new Exception($"Redefinition of function :'{expressionName}'");
                }

                if (function.ParamsCount != argumentCount)
                {
                    // function overloading should be dealth with on language level
                    // see https://mapping-high-level-constructs-to-llvm-ir.readthedocs.io/en/latest/basic-constructs/functions.html#function-overloading
                    throw new Exception($"Redefinition of function :'{expressionName}' with a different number of arguments.");
                }

            }
            else
            {
                //todo: support values other than doubles? We do have access to the tokens and their types... so why not?
                for (int i = 0; i < argumentCount; ++i)
                {
                    arguments[i] = GetReturnType(expression.Arguments[i].TypeToken.TypeIndicator);
                }
                var retType = GetReturnType(expression.ReturnTypeToken.TypeIndicator);
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
            _builder.PositionAtEnd(function.AppendBasicBlock(expressionName)); // this in combination with specifying /entry:Main causes an .exe to be able to be build.

            //todo: implement visit body and add it to the function? So actual code can be run
            if (expression.FunctionBody is not null)
            {
                try
                {
                    Visit(expression.FunctionBody);
                }
                catch (Exception)
                {
                    // should we remove the function if an error happens in the body?
                    throw;
                }
            }

            if (expression.ReturnTypeToken.TypeIndicator is TypeIndicator.Void)
            {
                _builder.BuildRetVoid();
            }
            else
            {
                var retValue = _valueStack.Pop();
                _builder.BuildRet(retValue);
            }

            function.VerifyFunction(LLVMVerifierFailureAction.LLVMPrintMessageAction);
            _valueStack.Push(function);
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
            var rhsValue = _valueStack.Pop();

            var lhsAndRhsBothIntegers = rhsValue.TypeOf.Kind is LLVMTypeKind.LLVMIntegerTypeKind && lhsValue.TypeOf.Kind is LLVMTypeKind.LLVMIntegerTypeKind;

            switch (expression.NodeExpressionType)
            {
                case ExpressionType.Assignment:
                    {
                        //todo: refactor the code so all assignments end here.. e.g. += -= *=, etc...?
                        _namedValues[((IdentifierExpression)expression.LeftHandSide).Identifier] = rhsValue;
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
                case ExpressionType.LogicalOr:
                    {
                        _valueStack.Push(_builder.BuildOr(lhsValue, rhsValue));
                        break;
                    }
                case ExpressionType.LogicalXOr:
                    {
                        _valueStack.Push(_builder.BuildXor(lhsValue, rhsValue));
                        break;
                    }
                case ExpressionType.LogicalAnd:
                    {
                        _valueStack.Push(_builder.BuildAnd(lhsValue, rhsValue));
                        break;
                    }
                case ExpressionType.ConditionalOr:
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
            // todo: fix so it works without else...
            Visit(expression.IfCondition);
            var condExpr = _valueStack.Pop(); // this is true or false right? or should i compare this again like the example? ->   var condv = LLVM.BuildFCmp(this.builder, LLVMRealPredicate.LLVMRealONE, this.valueStack.Pop(), LLVM.ConstReal(LLVM.DoubleType(), 0.0), "ifcond");
            var condv = _builder.BuildICmp(LLVMIntPredicate.LLVMIntEQ, condExpr, LLVMValueRef.CreateConstInt(LLVMTypeRef.Int1, 1), "ifcond");
            var parentFuncBlock = _builder.InsertBlock.Parent;
            var ifBodyBB = parentFuncBlock.AppendBasicBlock("ifBody");
            var elseBB = parentFuncBlock.AppendBasicBlock("else");
            var mergeBB = parentFuncBlock.AppendBasicBlock("ifcontext");

            _builder.BuildCondBr(condv, ifBodyBB, elseBB);

            _builder.PositionAtEnd(ifBodyBB); // this is called "emitting the value..." even though we havent visited it yet.. Perhaps it now starts registering or something?
            Visit(expression.IfBody);
    
            var ifBodyValue = _valueStack.Pop();
            _builder.BuildBr(mergeBB);

            //ifBody can change current code block, update thenBB for the PHI
            ifBodyBB = _builder.InsertBlock;

            // emit again before visit...
            _builder.PositionAtEnd(elseBB);
            Visit(expression.Else);
 
            var elseValue = _valueStack.Pop();
            _builder.BuildBr(mergeBB);

            //body of else can change current code block, update thenBB for the PHI
            elseBB = _builder.InsertBlock;

            // emit the merge of if and else
            _builder.PositionAtEnd(mergeBB);

            // what does Phi mean? Phony? https://en.wikipedia.org/wiki/Static_single_assignment_form
            var phi = _builder.BuildPhi(LLVMTypeRef.Int64); // example uses a double type... Will int also work? -> no, phi is a floating point operation...? but lets test it anyway
            phi.AddIncoming(new[] { ifBodyValue }, new[] { ifBodyBB }, 1);
            phi.AddIncoming(new[] { elseValue }, new[] { elseBB }, 1);

            _valueStack.Push(phi);
        }

        public void VisitForStatementExpression(ForStatementExpression expression)
        {
            // Output this as:
            //   ...
            //   start = startexpr
            //   goto loop
            // loop:
            //   variable = phi [start, loopheader], [nextvariable, loopend]
            //   ...
            //   bodyexpr
            //   ...
            // loopend:
            //   step = stepexpr
            //   nextvariable = variable + step
            //   endcond = endexpr
            //   br endcond, loop, endloop
            // outloop:

            // Emit the start code first, without 'variable' in scope.
            _namedValues.TryGetValue(expression.VariableName, out var oldVal);
            Visit(expression.VariableDeclaration);
            var startValue = _namedValues[expression.VariableName];
            // Make the new basic block for the loop header, inserting after current
            // block.
            var preHeaderBB = _builder.InsertBlock;
            var function = preHeaderBB.Parent;
            var loopBB = function.AppendBasicBlock("loop");

            // Insert an explicit fall through from the current block to the LoopBB.
            _builder.BuildBr(loopBB);
            // Start insertion in LoopBB.
            _builder.PositionAtEnd(loopBB);
            var variable = _builder.BuildPhi(LLVMTypeRef.Double, expression.VariableName);
            variable.AddIncoming(new[] { startValue }, new[] { preHeaderBB }, 1);

            // Within the loop, the variable is defined equal to the PHI node.  If it
            // shadows an existing variable, we have to restore it, so save it now.

            //LLVMValueRef oldVal;
            //if (_namedValues.TryGetValue(expression.VariableName, out oldVal))
            //{
            //    _namedValues[expression.VariableName] = variable;
            //}
            //else
            //{
            //    _namedValues.Add(expression.VariableName, variable);
            //}

            // Emit the body of the loop.  This, like any other expr, can change the
            // current BB.  Note that we ignore the value computed by the body, but don't
            // allow an error.
            Visit(expression.Body);

            Visit(expression.VariableIncreaseExpression);
            var stepValue = _namedValues[expression.VariableName];
            //var stepValue = _valueStack.Pop();

            var nextVariable = _builder.BuildFAdd(variable, stepValue, "nextvar");

            // Compute the end condition.
            Visit(expression.Condition);
            var conditionExprValue = _valueStack.Pop();

            var endCondition = _builder.BuildICmp(LLVMIntPredicate.LLVMIntEQ, conditionExprValue, LLVMValueRef.CreateConstInt(LLVMTypeRef.Int1, 0));
            // Create the "after loop" block and insert it.
            var loopEndBB = _builder.InsertBlock;
            var afterBB = function.AppendBasicBlock("afterLoop");

            _builder.BuildCondBr(endCondition, loopBB, afterBB);
            // Any new code will be inserted in AfterBB.
            _builder.PositionAtEnd(afterBB);
            variable.AddIncoming(new[] { nextVariable }, new[] { loopEndBB }, 1);

            // Restore the unshadowed variable.
            if (oldVal.Handle != IntPtr.Zero)
            {
                _namedValues[expression.VariableName] = oldVal;
            }
            else
            {
                _namedValues.Remove(expression.VariableName);
            }

            _valueStack.Push(LLVMValueRef.CreateConstInt(LLVMTypeRef.Int1, 0));// why?
        }

        public void VisitBodyExpression(BodyExpression expression)
        {
            // BodyExpression should probably have a function name attached and some more info of the parent e.g. token type?
            // if this body isn't valid then we should remove the function all together.. and throw an error.
            // todo: do we need to do anything here?
            foreach (var expr in expression.Body)
            {
                Visit(expr);
            }
        }

        public void VisitWhileStatementExpression(WhileStatementExpression expression)
        {
            Visit(expression.WhileCondition);
            var whileCondition = _valueStack.Pop();

            Visit(expression.DoBody);
            var doBody = _valueStack.Pop();
        }

        public void VisitReturnExpression(ReturnExpression expression)
        {
            Visit(expression.Expression);
            //if (_valueStack.Count > 0)
            //{
            //    _valueStack.Push(_builder.BuildRet(_valueStack.Pop()));
            //}
            //else
            //{
            //    _valueStack.Push(_builder.BuildRetVoid());
            //}
        }

    }
}
