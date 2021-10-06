using Parser.AbstractSyntaxTree.Expressions;

namespace Parser.AbstractSyntaxTree.Visitors
{
    public interface IAbstractSyntaxTreeVisitorExecuter : IAbstractSyntaxTreeVisitor
    {
        string Execute(Queue<ExpressionBase> abstractSyntaxTrees, IByteCodeGeneratorListener listener);
    }
}
