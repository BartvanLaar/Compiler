using LLVMSharp;
using Parser.AbstractSyntaxTree;
using Parser.AbstractSyntaxTree.Expressions;
using Parser.CodeLexer;
using Parser.LLVMSupport;
using System.Diagnostics;

namespace Parser
{
    public class Driver
    {
        public static void Run(string text) => Run(text, new DummyAbstractSyntaxTreeVisitor(), new AbstractSyntaxTreeVisitorListener());
        public static void RunLLVM(string text)
        {
            //var (codeGenerationListener, module, builder) = SetupLLVM();
            Run(text, new DummyAbstractSyntaxTreeVisitor(), new AbstractSyntaxTreeVisitorListener());// todo: replace with LLVM bytecode generator.

            //var module = LLVM.ModuleCreateWithName("NativeBinary");
            //var functype = LLVM.FunctionType(LLVM.Int32Type(), new LLVMTypeRef[] { }, false);
            //var main = LLVM.AddFunction(module, "main", functype);
            //var entrypoint = LLVM.AppendBasicBlock(main, "entrypoint");
            //LLVMBuilderRef build = LLVM.CreateBuilder();
            //LLVM.PositionBuilderAtEnd(build, entrypoint);
            //var fortytwo = LLVM.ConstInt(LLVM.Int32Type(), 42, new LLVMBool(0));

            //LLVM.BuildRet(build, fortytwo);

            //LLVM.DumpModule(module);
            //LLVM.WriteBitcodeToFile(module, "test.bc");


            //LLVM.PositionBuilderAtEnd(builder, entrypoint);

            //LLVM.DumpModule(llvmModule);  // Print out all of the generated code.

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

            Console.WriteLine($"Lexing and parsing took {stopwatch.ElapsedMilliseconds} milliseconds");

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

        }

        internal interface IByteCodeGeneratorListener : IAbstractSyntaxTreeVisitor
        {
        }


        internal class AbstractSyntaxTreeVisitorListener : IByteCodeGeneratorListener
        {
            public void VisitDoubleExpression(DoubleExpression expression)
            {
                Log(expression);
            }

            public void VisitFloatExpression(FloatExpression expression)
            {
                Log(expression);
            }

            public void VisitIntegerExpression(IntegerExpression expression)
            {
                Log(expression);
            }

            public void VisitStringExpression(StringExpression expression)
            {
                Log(expression);
            }
            public void VisitCharacterExpression(CharacterExpression expression)
            {
                Log(expression);
            }

            public void VisitBinaryExpression(BinaryExpression expression)
            {
                Log(expression);
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

            private static void Log(ExpressionBase baseExp)
            {
                Console.WriteLine($"Visited tree node of type: '{baseExp?.GetType()?.Name ?? null}'");
            }
        }

        internal class DummyAbstractSyntaxTreeVisitor : IAbstractSyntaxTreeVisitorExecuter
        {
            private Queue<ExpressionBase> _abstractSyntaxTrees;
            private IByteCodeGeneratorListener _listener;

            public string Execute(Queue<ExpressionBase> abstractSyntaxTrees, IByteCodeGeneratorListener listener)
            {
                _abstractSyntaxTrees = abstractSyntaxTrees;
                _listener = listener;

                while (_abstractSyntaxTrees.Any())
                {
                    Visit(_abstractSyntaxTrees.Dequeue());
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
                        break;
                    // These are all handled by binary operator expressions.
                    case ExpressionType.Equivalent:
                    case ExpressionType.Equals:
                    case ExpressionType.GreaterThan:
                    case ExpressionType.GreaterThanEqual:
                    case ExpressionType.LessThan:
                    case ExpressionType.LessThanEqual:
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
                    default:
                        // should this be visiting a top level?
                        throw new ArgumentException($"Unknown expression type encountered: '{expression.GetType()}'");
                }
            }


            public void VisitIntegerExpression(IntegerExpression expression)
            {
                _listener.VisitIntegerExpression(expression);
            }

            public void VisitDoubleExpression(DoubleExpression expression)
            {
                _listener.VisitDoubleExpression(expression);
            }

            public void VisitFloatExpression(FloatExpression expression)
            {
                _listener.VisitFloatExpression(expression);
            }

            public void VisitStringExpression(StringExpression expression)
            {
                _listener.VisitStringExpression(expression);
            }

            public void VisitCharacterExpression(CharacterExpression expression)
            {
                _listener.VisitCharacterExpression(expression);
            }

            public void VisitBinaryExpression(BinaryExpression expression)
            {
                _listener.VisitBinaryExpression(expression);

                Visit(expression.LeftHandSide);
                Visit(expression.RightHandSide);
            }

            public void VisitPrototypeExpression(PrototypeExpression expression)
            {
                //Visit(expression); \\ this triggers an overflow for obvious reasons...
                _listener.VisitPrototypeExpression(expression);
            }

            public void VisitMethodCallExpression(MethodCallExpression expression)
            {
                _listener.VisitMethodCallExpression(expression);

                foreach (var argument in expression.MethodArguments)
                {
                    Visit(argument);
                }
            }

            public void VisitFunctionCallExpression(FunctionCallExpression expression)
            {
                _listener.VisitFunctionCallExpression(expression);
                
                Visit(expression.Prototype);
                Visit(expression.Body);
            }

            public void VisitAssignmentExpression(AssignmentExpression expression)
            {
                _listener.VisitAssignmentExpression(expression);
               
                Visit(expression.IdentificationExpression);
                Visit(expression.ValueExpression);
            }

            public void VisitIdentifierExpression(IdentifierExpression expression)
            {
                _listener.VisitIdentifierExpression(expression);
            }
        }

        //private static (CodeGenerationParserListener CodeGenerationListener, LLVMModuleRef Module, LLVMBuilderRef Builder) SetupLLVM()
        //{
        //    LLVMModuleRef module = LLVM.ModuleCreateWithName("B#");
        //    LLVMBuilderRef builder = LLVM.CreateBuilder();


        //    LLVM.LinkInMCJIT();
        //    LLVM.InitializeX86TargetMC();
        //    LLVM.InitializeX86Target();
        //    LLVM.InitializeX86TargetInfo();
        //    LLVM.InitializeX86AsmParser();
        //    LLVM.InitializeX86AsmPrinter();

        //    if (LLVM.CreateExecutionEngineForModule(out var engine, module, out var errorMessage).Value == 1)
        //    {
        //        Console.WriteLine(errorMessage);
        //        // LLVM.DisposeMessage(errorMessage);
        //        throw new Exception("Unable to setup LLVM.", new Exception(errorMessage));
        //    }

        //    // Create a function pass manager for this engine
        //    LLVMPassManagerRef passManager = LLVM.CreateFunctionPassManagerForModule(module);

        //    // Set up the optimizer pipeline.  Start with registering info about how the
        //    // target lays out data structures.
        //    //LLVM.DisposeTargetData(LLVM.GetExecutionEngineTargetData(engine));

        //    // Provide basic AliasAnalysis support for GVN.
        //    LLVM.AddBasicAliasAnalysisPass(passManager);

        //    // Promote allocations to registers.
        //    LLVM.AddPromoteMemoryToRegisterPass(passManager);

        //    // Do simple "peephole" optimizations and bit-twiddling optzns.
        //    LLVM.AddInstructionCombiningPass(passManager);

        //    // Reassociate expressions.
        //    LLVM.AddReassociatePass(passManager);

        //    // Eliminate Common SubExpressions.
        //    LLVM.AddGVNPass(passManager);

        //    // Simplify the control flow graph (deleting unreachable blocks, etc).
        //    LLVM.AddCFGSimplificationPass(passManager);

        //    LLVM.InitializeFunctionPassManager(passManager);


        //    var codeGenerationListener = new CodeGenerationParserListener(new CodeGenerationVisitor(module, builder), engine, passManager);

        //    return (codeGenerationListener, module, builder);
        //}

        private class ConsoleParserListener : IParserListener
        {
            public void EnterHandleAssignmentExpression(AssignmentExpression data)
            {
                WriteStart(data);
            }
            public void ExitHandleAssignmentExpression(AssignmentExpression data)
            {
                WriteFinish(data);
            }

            public void EnterHandleTopLevelExpression(FunctionCallExpression data)
            {
                WriteStart(data);
            }

            public void ExitHandleTopLevelExpression(FunctionCallExpression data)
            {
                WriteFinish(data);
            }

            private static void WriteStart(ExpressionBase data)
            {
                Console.WriteLine($"Start parsing {data.NodeExpressionType}");
            }

            private static void WriteFinish(ExpressionBase data)
            {
                Console.WriteLine($"Finished parsing {data.NodeExpressionType}");
            }
        }

    }
}
