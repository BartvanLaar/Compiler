using LLVMSharp;
using Parser.AbstractSyntaxTree;
using Parser.AbstractSyntaxTree.Expressions;
using System.Runtime.InteropServices;

namespace Parser.LLVMSupport
{
    internal sealed class CodeGenerationParserListener : IParserListener
    {
        private readonly CodeGenerationVisitor _visitor;

        private readonly LLVMExecutionEngineRef _executionEngine;

        private readonly LLVMPassManagerRef _passManager;


        private delegate double D_FUNCTION_PTR(); // should we support all types of generic functions? probably..
        public CodeGenerationParserListener(CodeGenerationVisitor visitor, LLVMExecutionEngineRef executionEngine, LLVMPassManagerRef passManager)
        {
            _visitor = visitor;
            _executionEngine = executionEngine;
            _passManager = passManager;
        }

        public void EnterHandleAssignmentExpression(AssignmentExpression data)
        {
        }



        public void ExitHandleAssignmentExpression(AssignmentExpression data)
        {
            _visitor.Visit(data);
            //var function = _visitor.ResultStack.Pop();
            // LLVM.DumpValue(function);  // Dump the function for exposition purposes.
        }

        public void EnterHandleTopLevelExpression(FunctionCallExpression data)
        {
        }

        public void ExitHandleTopLevelExpression(FunctionCallExpression data)
        {
            _visitor.Visit(data);
            var anonymousFunction = _visitor.ResultStack.Pop();
            LLVM.DumpValue(anonymousFunction);
            //// LLVM.DumpValue(anonymousFunction); // Dump the function for exposition purposes.
            //var dFunc = (D_FUNCTION_PTR)Marshal.GetDelegateForFunctionPointer(
            //    LLVM.GetPointerToGlobal(_executionEngine, anonymousFunction), typeof(D_FUNCTION_PTR));
            //LLVM.RunFunctionPassManager(_passManager, anonymousFunction);

            ////            LLVM.DumpValue(anonymousFunction); // Dump the function for exposition purposes.
            //Console.WriteLine("Evaluated to " + dFunc());
        }
    }
}
