using LLVMSharp;
using Parser.AbstractSyntaxTree.Expressions;
using Parser.CodeLexer;
using System.Diagnostics;
using System.Runtime.InteropServices;

namespace Parser
{
    public class Driver
    {   
        private delegate double D_FUNCTION_PTR();
        public static void Run(string text) => Run(text, new AbstractSyntaxTreeVisitorExecutor(), new AbstractSyntaxTreeVisitorLogger());
        public static void RunLLVM(string text)
        {
            var (module, builder, executionEngine, passManager) = SetupLLVM();
            var visitor = new CodeGenerationVisitor(module, builder);
            Run(text, new AbstractSyntaxTreeVisitorExecutor(), visitor);// todo: replace with LLVM bytecode generator.
            var anonymousFunction = visitor.ResultStack.Pop();

            var dFunc = (D_FUNCTION_PTR)Marshal.GetDelegateForFunctionPointer(
                LLVM.GetPointerToGlobal(executionEngine, anonymousFunction), typeof(D_FUNCTION_PTR));
            LLVM.RunFunctionPassManager(passManager, anonymousFunction);
            LLVM.DumpModule(module);
            LLVM.WriteBitcodeToFile(module, "test.bc");

       

            Console.WriteLine("Evaluated to " + dFunc());

            //var path = Path.Combine(Directory.GetCurrentDirectory(), "test.bc");
            //LLVM.WriteBitcodeToFile(module, path);
            //var lldLink = Process.Start("clang++", $"{path} -v -o {Path.Combine(Directory.GetCurrentDirectory(), "test.exe")}");
            //lldLink.Start();
            //lldLink.WaitForExit();
        }
        internal static void Run(string text, IAbstractSyntaxTreeVisitorExecuter byteCodeGenerator, IByteCodeGeneratorListener byteCodeGeneratorListener)
        {
            var stopwatch = new Stopwatch();
            stopwatch.Start();
            var lexer = new Lexer(text);
            var parser = new Parser(lexer);
            Queue<ExpressionBase> abstractSyntaxTrees = parser.Parse();
            stopwatch.Stop();
            Console.WriteLine($"Lexing and parsing took {stopwatch.ElapsedMilliseconds} milliseconds");

            stopwatch.Restart();
            // For now one file is enough to support.
            // We should some day support a way of importing other files, but should that result in multiple bytecode files? or even abstract trees? not sure.. Probably not.
            string byteCodeFile = byteCodeGenerator.Execute(abstractSyntaxTrees, byteCodeGeneratorListener);
            stopwatch.Stop();

            Console.WriteLine($"Generating bytecode took {stopwatch.ElapsedMilliseconds} milliseconds");

        }
        internal interface IAbstractSyntaxTreeVisitorExecuter : IAbstractSyntaxTreeVisitor
        {
            string Execute(Queue<ExpressionBase> abstractSyntaxTrees, IByteCodeGeneratorListener listener);
        }

        internal interface IAbstractSyntaxTreeVisitor
        {
            void VisitIntegerExpression(IntegerExpression expression);
            void VisitDoubleExpression(DoubleExpression expression);
            void VisitFloatExpression(FloatExpression expression);
            void VisitStringExpression(StringExpression expression);
            void VisitCharacterExpression(CharacterExpression expression);
            void VisitBinaryExpression(BinaryExpression expression);
            void VisitPrototypeExpression(PrototypeExpression expression);
            void VisitMethodCallExpression(MethodCallExpression expression);
            void VisitFunctionCallExpression(FunctionCallExpression expression);
            void VisitAssignmentExpression(AssignmentExpression expression);
            void VisitIdentifierExpression(IdentifierExpression expression);
            void VisitBodyExpression(BodyExpression expression);

        }

        internal interface IByteCodeGeneratorListener : IAbstractSyntaxTreeVisitor
        {
        }


        internal class AbstractSyntaxTreeVisitorLogger : IByteCodeGeneratorListener
        {
            public void VisitDoubleExpression(DoubleExpression expression)
            {
                LogValue(expression);
            }

            public void VisitFloatExpression(FloatExpression expression)
            {
                LogValue(expression);
            }

            public void VisitIntegerExpression(IntegerExpression expression)
            {
                LogValue(expression);
            }

            public void VisitStringExpression(StringExpression expression)
            {
                LogValue(expression);
            }
            public void VisitCharacterExpression(CharacterExpression expression)
            {
                LogValue(expression);
            }

            public void VisitBinaryExpression(BinaryExpression expression)
            {
                LogValue(expression);
            }

            public void VisitPrototypeExpression(PrototypeExpression expression)
            {
                Log(expression);
            }

            public void VisitMethodCallExpression(MethodCallExpression expression)
            {
                Log(expression);
            }

            public void VisitFunctionCallExpression(FunctionCallExpression expression)
            {
                Log(expression);
            }

            public void VisitAssignmentExpression(AssignmentExpression expression)
            {
                Log(expression);
            }

            public void VisitIdentifierExpression(IdentifierExpression expression)
            {
                Log(expression);
            }

            public void VisitBodyExpression(BodyExpression expression)
            {
                LogValue(expression.Expression);
            }

            private static void LogValue(ExpressionBase baseExp)
            {
                Console.WriteLine($"Visited tree node of type: '{baseExp?.GetType()?.Name ?? null}' with token: '{baseExp?.Token}'.");
            }
            private static void Log(ExpressionBase baseExp)
            {
                Console.WriteLine($"Visited tree node of type: '{baseExp?.GetType()?.Name ?? null}'");
            }
        }


        internal class CodeGenerationVisitor : IByteCodeGeneratorListener
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

            public void VisitBodyExpression(BodyExpression expression)
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
                if(!LLVM.VerifyFunction(protoTypeFunction, LLVMVerifierFailureAction.LLVMPrintMessageAction))
                {
                    LLVM.DeleteFunction(protoTypeFunction);
                    throw new Exception("Encoutered a bad function..?"); // clarify message?
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
        }

        internal class AbstractSyntaxTreeVisitorExecutor : IAbstractSyntaxTreeVisitorExecuter
        {
            private IByteCodeGeneratorListener? _listener;

            public string Execute(Queue<ExpressionBase> abstractSyntaxTrees, IByteCodeGeneratorListener listener)
            {
                _listener = listener;

                while (abstractSyntaxTrees.Any())
                {
                    Visit(abstractSyntaxTrees.Dequeue());
                }

                var resultFilePath = string.Empty;
                return resultFilePath; // for now support a single file.
            }

            public void Visit(ExpressionBase expression)
            {
                if (expression == null)
                {
                    return;
                }

                switch (expression.NodeExpressionType)
                {
                    // These are all handled by binary operator expressions.
                    case ExpressionType.Add:
                    case ExpressionType.Subtract:
                    case ExpressionType.Multiply:
                    case ExpressionType.Divide:
                    case ExpressionType.DivideRest:
                        VisitBinaryExpression((BinaryExpression)expression);
                        break;
                    // These are all handled by binary operator expressions.
                    case ExpressionType.Equivalent:
                    case ExpressionType.Equals:
                    case ExpressionType.GreaterThan:
                    case ExpressionType.GreaterThanEqual:
                    case ExpressionType.LessThan:
                    case ExpressionType.LessThanEqual:
                        VisitBinaryExpression((BinaryExpression)expression);
                        break;
                    case ExpressionType.MethodCall:
                        VisitMethodCallExpression((MethodCallExpression)expression);
                        break;
                    case ExpressionType.Identifier:
                        VisitIdentifierExpression((IdentifierExpression)expression);
                        break;
                    case ExpressionType.Prototype:
                        VisitPrototypeExpression((PrototypeExpression)expression);
                        break;
                    case ExpressionType.FunctionCall:
                        VisitFunctionCallExpression((FunctionCallExpression)expression);
                        break;
                    case ExpressionType.Double:
                        VisitDoubleExpression((DoubleExpression)expression);
                        break;
                    case ExpressionType.Float:
                        VisitFloatExpression((FloatExpression)expression);
                        break;
                    case ExpressionType.Integer:
                        VisitIntegerExpression((IntegerExpression)expression);
                        break;
                    case ExpressionType.String:
                        VisitStringExpression((StringExpression)expression);
                        break;
                    case ExpressionType.Character:
                        VisitCharacterExpression((CharacterExpression)expression);
                        break;
                    case ExpressionType.Assignment:
                        VisitAssignmentExpression((AssignmentExpression)expression);
                        break;
                    case ExpressionType.Body:
                        VisitBodyExpression((BodyExpression)expression);
                        break;
                    default:
                        // should this be visiting a top level?
                        throw new ArgumentException($"Unknown expression type encountered: '{expression.GetType()}'");
                }
            }

            public void VisitIntegerExpression(IntegerExpression expression)
            {
                _listener?.VisitIntegerExpression(expression);
            }

            public void VisitDoubleExpression(DoubleExpression expression)
            {
                _listener?.VisitDoubleExpression(expression);
            }

            public void VisitFloatExpression(FloatExpression expression)
            {
                _listener?.VisitFloatExpression(expression);
            }

            public void VisitStringExpression(StringExpression expression)
            {
                _listener?.VisitStringExpression(expression);
            }

            public void VisitCharacterExpression(CharacterExpression expression)
            {
                _listener?.VisitCharacterExpression(expression);
            }

            public void VisitBinaryExpression(BinaryExpression expression)
            {
                Visit(expression.LeftHandSide);
                Visit(expression.RightHandSide);

                _listener?.VisitBinaryExpression(expression);
            }

            public void VisitPrototypeExpression(PrototypeExpression expression)
            {
                //Visit(expression); \\ this triggers an overflow for obvious reasons...
                _listener?.VisitPrototypeExpression(expression);
            }

            public void VisitMethodCallExpression(MethodCallExpression expression)
            {
                foreach (var argument in expression.MethodArguments)
                {
                    Visit(argument);
                }

                _listener?.VisitMethodCallExpression(expression);
            }

            public void VisitFunctionCallExpression(FunctionCallExpression expression)
            {
                Visit(expression.Prototype);
                Visit(expression.Body);

                _listener?.VisitFunctionCallExpression(expression);
            }

            public void VisitAssignmentExpression(AssignmentExpression expression)
            {
                Visit(expression.IdentificationExpression);
                Visit(expression.ValueExpression);

                _listener?.VisitAssignmentExpression(expression);
            }

            public void VisitIdentifierExpression(IdentifierExpression expression)
            {
                _listener?.VisitIdentifierExpression(expression);
            }

            public void VisitBodyExpression(BodyExpression expression)
            {
                Visit(expression.Expression);
                _listener?.VisitBodyExpression(expression);
            }
        }

        private static (LLVMModuleRef Module, LLVMBuilderRef Builder, LLVMExecutionEngineRef engine, LLVMPassManagerRef passManager) SetupLLVM()
        {
            LLVMModuleRef module = LLVM.ModuleCreateWithName("B#");
            LLVMBuilderRef builder = LLVM.CreateBuilder();

            LLVM.LinkInMCJIT();
            LLVM.InitializeX86TargetMC();
            LLVM.InitializeX86Target();
            LLVM.InitializeX86TargetInfo();
            LLVM.InitializeX86AsmParser();
            LLVM.InitializeX86AsmPrinter();

            if (LLVM.CreateExecutionEngineForModule(out var engine, module, out var errorMessage).Value == 1)
            {
                Console.WriteLine(errorMessage);
                // LLVM.DisposeMessage(errorMessage);
                throw new Exception("Unable to setup LLVM.", new Exception(errorMessage));
            }

            // Create a function pass manager for this engine
            LLVMPassManagerRef passManager = LLVM.CreateFunctionPassManagerForModule(module);

            // Set up the optimizer pipeline.  Start with registering info about how the
            // target lays out data structures.
            //LLVM.DisposeTargetData(LLVM.GetExecutionEngineTargetData(engine));

            // Provide basic AliasAnalysis support for GVN.
            LLVM.AddBasicAliasAnalysisPass(passManager);

            // Promote allocations to registers.
            LLVM.AddPromoteMemoryToRegisterPass(passManager);

            // Do simple "peephole" optimizations and bit-twiddling optzns.
            LLVM.AddInstructionCombiningPass(passManager);

            // Reassociate expressions.
            LLVM.AddReassociatePass(passManager);

            // Eliminate Common SubExpressions.
            LLVM.AddGVNPass(passManager);

            // Simplify the control flow graph (deleting unreachable blocks, etc).
            LLVM.AddCFGSimplificationPass(passManager);

            LLVM.InitializeFunctionPassManager(passManager);


            //var codeGenerationListener = new CodeGenerationParserListener(new CodeGenerationVisitor(module, builder), engine, passManager);

            return (module, builder, engine, passManager);
        }

    }
}
