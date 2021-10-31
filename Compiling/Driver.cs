using Compiling.Backends;
using Lexing;
using LLVMSharp;
using LLVMSharp.Interop;
using Parsing;
using Parsing.AbstractSyntaxTree.Expressions;
using Parsing.AbstractSyntaxTree.Visitors;
using System.Diagnostics;

namespace Compiling
{
    public class Driver
    {
        public static void Run(string text) => Run(text, new AbstractSyntaxTreeVisitorExecutor(), new AbstractSyntaxTreeVisitorLogger());
        public static void RunDotNet(string text)
        {
            var visitor = new DotNetCodeInterpreter();
            Run(text, new AbstractSyntaxTreeVisitorExecutor(), visitor);
            foreach (var res in visitor.Results)
            {
                Console.WriteLine(res.ToString());
            }
        }

        public static void RunLLVM(string text, string filename = "output", bool isExecutable = false)
        {
            var (module, builder, executionEngine, passManager) = SetupLLVM();
            var visitor = new LLVMCodeGenerationVisitor(module, builder, executionEngine, passManager);
            Run(text, new AbstractSyntaxTreeVisitorExecutor(), visitor);// todo: replace with LLVM bytecode generator.

            var output = Path.Join(Directory.GetCurrentDirectory(), $"{Path.GetFileNameWithoutExtension(filename)}.bc");
            module.WriteBitcodeToFile(output);
            // don't know if dispose is required, but disposing may cause an error..
            // update; test diposes below after refactor.
            module.Dump();
            //module.Dispose();
            //builder.Dispose();
            //executionEngine.Dispose();
            //passManager.Dispose();
      

            if (!isExecutable)
            {
                var llc = Process.Start(@"llc", $"--filetype=obj {output}");
                llc.WaitForExit();
            }


            Process lld;
            if (isExecutable)
            {
                lld = Process.Start(@"clang", $"{output} -o {Path.GetFileNameWithoutExtension(output)}.exe");
            }
            else
            {
                lld = Process.Start(@"lld-link", $"/subsystem:console /noentry /dll {Path.GetFileNameWithoutExtension(output)}.obj");
            }

            lld.WaitForExit();

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
            LLVMModuleRef module = LLVMModuleRef.CreateWithName("B#");
            LLVMContextRef ctx = LLVMContextRef.Create();
            LLVMBuilderRef builder = LLVMBuilderRef.Create(ctx);
            //ctx.Dispose(); // todo: should i dispose of this context somewhere?

            LLVM.LinkInMCJIT();
            LLVM.InitializeX86TargetMC();
            LLVM.InitializeX86Target();
            LLVM.InitializeX86TargetInfo();
            LLVM.InitializeX86AsmParser();
            LLVM.InitializeX86AsmPrinter();
            //LLVM.InitializeAllAsmParsers();
            //LLVM.InitializeAllAsmPrinters();
            //LLVM.InitializeAllDisassemblers();
            //LLVM.InitializeAllTargetInfos();
            //LLVM.InitializeAllTargetMCs();
            //LLVM.InitializeAllTargets();
            var engine = module.CreateExecutionEngine();

            // Create a function pass manager for this engine
            LLVMPassManagerRef passManager = module.CreateFunctionPassManager();

            // Set up the optimizer pipeline.  Start with registering info about how the
            // target lays out data structures.
            //LLVM.DisposeTargetData(LLVM.GetExecutionEngineTargetData(engine));

            // Provide basic AliasAnalysis support for GVN.
            passManager.AddBasicAliasAnalysisPass();

            // Promote allocations to registers.
            passManager.AddPromoteMemoryToRegisterPass();

            // Do simple "peephole" optimizations and bit-twiddling optzns.
            passManager.AddInstructionCombiningPass();

            // Reassociate expressions.
            passManager.AddReassociatePass();

            // Eliminate Common SubExpressions.
            passManager.AddGVNPass();

            // Simplify the control flow graph (deleting unreachable blocks, etc).
            passManager.AddCFGSimplificationPass();

            passManager.InitializeFunctionPassManager();


            //var codeGenerationListener = new CodeGenerationParserListener(new CodeGenerationVisitor(module, builder), engine, passManager);

            return (module, builder, engine, passManager);
        }

    }
}
