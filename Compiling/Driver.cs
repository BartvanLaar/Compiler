using Compiling.Backends;
using Lexing;
using LLVMSharp;
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

            LLVM.DumpModule(module);
            var output = Path.Join(Directory.GetCurrentDirectory(), $"{Path.GetFileNameWithoutExtension(filename)}.bc");
            LLVM.WriteBitcodeToFile(module, output);

            var llc = Process.Start(@"llc", $"--filetype=obj {output}");
            llc.WaitForExit();
            //dll...
            //"lld-link /subsystem:console /dll /noentry output.obj";

            // no dll but need to specify entry point?
            //"lld-link /subsystem:console /entry:main output.obj";
            Process lld;
            if (isExecutable)
            {
                lld = Process.Start(@"lld-link", $"/subsystem:console /entry:Main {Path.GetFileNameWithoutExtension(output)}.obj");
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
            LLVMModuleRef module = LLVM.ModuleCreateWithName("B#");
            LLVMBuilderRef builder = LLVM.CreateBuilder();

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
