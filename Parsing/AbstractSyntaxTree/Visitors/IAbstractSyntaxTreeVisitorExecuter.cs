using Parsing.AbstractSyntaxTree.Expressions;

namespace Parsing.AbstractSyntaxTree.Visitors
{
    public interface IAbstractSyntaxTreeVisitorExecuter : IAbstractSyntaxTreeVisitor
    {
        string Execute(Queue<ExpressionBase> abstractSyntaxTrees, IByteCodeGeneratorListener listener);
    }
}
