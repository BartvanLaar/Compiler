using LLVMSharp;
using Parser.AbstractSyntaxTree.Expressions;
using Parser.AbstractSyntaxTree.Visitors;
using Parser.CodeLexer;
using Parser.LLVMSupport;
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
