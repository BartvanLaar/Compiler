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
    //todo: handle parse exceptions?
    public class Driver
    {
        public static void RunLogger(string text) => Run(text, new AbstractSyntaxTreeVisitorLogger());
        public static void RunDotNet(string text)
        {
            var visitor = new DotNetCodeInterpreter();
            Run(text, visitor);
            foreach (var res in visitor.Results)
            {
                Console.WriteLine(res?.ToString() ?? "null value");
            }
        }

        public static void RunLLVM(string text, string filename = "output", bool isExecutable = false, bool isDebug = false, bool useClangCompiler = false)
        {
            var (module, builder, executionEngine, passManager, ctx) = SetupLLVM();

            // below are all compilation steps..
            var visitor = new LLVMCodeGenerator(module, builder, executionEngine, passManager);
            Run(text, visitor);// todo: replace with LLVM bytecode generator.
            var sw = new Stopwatch();
            sw.Start();
            var output = Path.Join(Directory.GetCurrentDirectory(), $"{Path.GetFileNameWithoutExtension(filename)}.bc");
            module.WriteBitcodeToFile(output);
            sw.Stop();
            Console.WriteLine($"Writing bitcode to file took {sw.ElapsedMilliseconds} ms.");
            sw.Restart();
            module.Dump();
            ctx.Dispose();
            // i think the module is disposed by disposing the passManager and executionEngine...
            passManager.Dispose();
            executionEngine.Dispose();
            builder.Dispose();
            sw.Stop();
            Console.WriteLine($"Cleaning up LLVM leftovers took {sw.ElapsedMilliseconds} ms.");
            sw.Restart();
            Process lld;
            if (useClangCompiler)
            {
                lld = Process.Start(@"clang", $"{(isExecutable ? string.Empty : "--shared")} {output} -o {Path.GetFileNameWithoutExtension(output)}.{(isExecutable ? "exe" : "dll")} {(isDebug ? "--debug" : string.Empty)}");
            }
            else
            {
                var llc = Process.Start(@"llc", $"--filetype=obj {output}");
                llc.WaitForExit();
                lld = isExecutable ? Process.Start(@"lld-link", $"/entry:main {Path.GetFileNameWithoutExtension(output)}.obj")
                    : Process.Start(@"lld-link", $"/subsystem:console /dll /noentry {Path.GetFileNameWithoutExtension(output)}.obj");
            }

            lld.WaitForExit();
            sw.Stop();
            Console.WriteLine($"Creating exe or DLL took {sw.ElapsedMilliseconds} ms.");

        }

        internal static void Run(string text, params IAbstractSyntaxTreeVisitor[] visitors)
        {
            var stopwatch = new Stopwatch();
            stopwatch.Start();
            ILexer lexer = new Lexer(text);
            IParser parser = new Parser(lexer);
            ExpressionBase[] abstractSyntaxTrees = parser.Parse();
            stopwatch.Stop();
            Console.WriteLine($"Lexing and parsing took {stopwatch.ElapsedMilliseconds} milliseconds");

            // For now one file is enough to support.
            // We should some day support a way of importing other files, but should that result in multiple bytecode files? or even abstract trees? not sure.. Probably not.
            foreach (var visitor in visitors)
            {
                stopwatch.Restart();
                AbstractSyntaxTreeVisitor.Visit(abstractSyntaxTrees, visitor);
                stopwatch.Stop();
                Console.WriteLine($"Running {visitor.Name} visitor took {stopwatch.ElapsedMilliseconds} milliseconds.");
            }

            stopwatch.Stop();
        }

        private static (LLVMModuleRef Module, LLVMBuilderRef Builder, LLVMExecutionEngineRef engine, LLVMPassManagerRef passManagerLLVMContextRef, LLVMContextRef Context) SetupLLVM()
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

            return (module, builder, engine, passManager, ctx);
        }

    }
}
