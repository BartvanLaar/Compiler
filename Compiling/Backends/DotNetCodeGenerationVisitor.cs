using Parsing.AbstractSyntaxTree.Expressions;
using Parsing.AbstractSyntaxTree.Visitors;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Compiling.Backends
{
    internal class DotNetCodeGenerationVisitor : IByteCodeGeneratorListener
    {
        public void VisitAssignmentExpression(AssignmentExpression expression)
        {
            throw new NotImplementedException();
        }

        public void VisitBinaryExpression(BinaryExpression expression)
        {
            throw new NotImplementedException();
        }

        public void VisitBodyStatementExpression(BodyExpression expression)
        {
            throw new NotImplementedException();
        }

        public void VisitCharacterExpression(CharacterExpression expression)
        {
            throw new NotImplementedException();
        }

        public void VisitDoubleExpression(DoubleExpression expression)
        {
            throw new NotImplementedException();
        }

        public void VisitFloatExpression(FloatExpression expression)
        {
            throw new NotImplementedException();
        }

        public void VisitForStatementExpression(ForStatementExpression expression)
        {
            throw new NotImplementedException();
        }

        public void VisitFunctionCallExpression(FunctionCallExpression expression)
        {
            throw new NotImplementedException();
        }

        public void VisitIdentifierExpression(IdentifierExpression expression)
        {
            throw new NotImplementedException();
        }

        public void VisitIfStatementExpression(IfStatementExpression expression)
        {
            throw new NotImplementedException();
        }

        public void VisitIntegerExpression(IntegerExpression expression)
        {
            throw new NotImplementedException();
        }

        public void VisitMethodCallExpression(MethodCallExpression expression)
        {
            throw new NotImplementedException();
        }

        public void VisitPrototypeExpression(PrototypeExpression expression)
        {
            throw new NotImplementedException();
        }

        public void VisitStringExpression(StringExpression expression)
        {
            throw new NotImplementedException();
        }
    }
}
