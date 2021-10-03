using Parser.AbstractSyntaxTree;
using Parser.AbstractSyntaxTree.Expressions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Parser.LLVMSupport
{
    internal sealed class CodeGenerationParserListener : IParserListener
    {

        public CodeGenerationParserListener()
        {

        }
        public void EnterHandleAssignmentExpression(AssignmentExpression data)
        {
            throw new NotImplementedException();
        }

        public void EnterHandleTopLevelExpression(FunctionCallExpression data)
        {
            throw new NotImplementedException();
        }

        public void ExitHandleAssignmentExpression(AssignmentExpression data)
        {
            throw new NotImplementedException();
        }

        public void ExitHandleTopLevelExpression(FunctionCallExpression data)
        {
            throw new NotImplementedException();
        }
    }
}
