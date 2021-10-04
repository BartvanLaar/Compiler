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
        public static void Run(string text) => Run(text, new ConsoleParserListener());
        public static void RunLLVM(string text)
        {
            var stopwatch = new Stopwatch();
            stopwatch.Start();
            var (codeGenerationListener, llvmModule) = SetupLLVM();
            Run(text, codeGenerationListener);

            //LLVM.DumpModule(llvmModule);  // Print out all of the generated code.

            var path = Path.Combine(Directory.GetCurrentDirectory(), "test.bc");
            LLVM.WriteBitcodeToFile(llvmModule, path);
            //var lldLink = Process.Start("lld-link.exe", $"{path} /nodefaultlib /subsystem:console /machine:x64 /out:{Path.Combine(Directory.GetCurrentDirectory(), "test.exe")}");
            //lldLink.WaitForExit();
            stopwatch.Stop();
            Console.WriteLine($"Compiling took {stopwatch.Elapsed.Milliseconds} milliseconds");
        }

        private static void Run(string text, IParserListener listener)
        {
            var lexer = new Lexer(text);
            var parser = new Parser(lexer, listener);
            parser.Parse();
        }

        private static (CodeGenerationParserListener CodeGenerationListener, LLVMModuleRef Module) SetupLLVM()
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
            // LLVM.DisposeTargetData(LLVM.GetExecutionEngineTargetData(engine));

            // Provide basic AliasAnalysis support for GVN.
            LLVM.AddBasicAliasAnalysisPass(passManager);

            // Promote allocas to registers.
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

            var codeGenerationListener = new CodeGenerationParserListener(new CodeGenerationVisitor(module, builder), engine, passManager);

            return (codeGenerationListener, module);
        }

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
