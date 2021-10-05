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
        public static void Run(string text) => Run(text, new DummyByteCodeGenerator());
        public static void RunLLVM(string text)
        {
            //var (codeGenerationListener, module, builder) = SetupLLVM();
            Run(text, new DummyByteCodeGenerator());// todo: replace with LLVM bytecode generator.

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
        internal static void Run(string text, IByteCodeGenerator byteCodeGenerator)
        {
            var stopwatch = new Stopwatch();
            stopwatch.Start();
            var lexer = new Lexer(text);
            var parser = new Parser(lexer);
            Queue<ExpressionBase> abstractSyntaxTrees = parser.Parse();
            stopwatch.Stop();
            Console.WriteLine($"Lexing and parsing took {stopwatch.ElapsedMilliseconds} milliseconds");
            
            stopwatch.Restart();
            string[] byteCodeFiles = byteCodeGenerator.Generate(abstractSyntaxTrees);
            stopwatch.Stop();

            Console.WriteLine($"Lexing and parsing took {stopwatch.ElapsedMilliseconds} milliseconds");

        }
        internal interface IByteCodeGenerator
        {
            string[] Generate(Queue<ExpressionBase> abstractSyntaxTrees);
        }

        internal class DummyByteCodeGenerator : IByteCodeGenerator
        {
            public string[] Generate(Queue<ExpressionBase> abstractSyntaxTrees)
            {
                return Array.Empty<string>();
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
