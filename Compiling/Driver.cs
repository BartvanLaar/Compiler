﻿using Compiling.Backends;
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

        public static void RunLLVM(string text, string filename = "output", bool isExecutable = false, bool isDebug = false)
        {
            var (module, builder, executionEngine, passManager, ctx) = SetupLLVM();
            var visitor = new LLVMCodeGenerationVisitor(module, builder, executionEngine, passManager);
            Run(text, new AbstractSyntaxTreeVisitorExecutor(), visitor);// todo: replace with LLVM bytecode generator.
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

            var lld = Process.Start(@"clang", $"{(isExecutable ? string.Empty : "--shared")} {output} -o {Path.GetFileNameWithoutExtension(output)}.{(isExecutable ? "exe" : "dll")} {(isDebug ? "--debug" : string.Empty)}");

            lld.WaitForExit();
            sw.Stop();
            Console.WriteLine($"Creating exe or DLL took {sw.ElapsedMilliseconds} ms.");

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
