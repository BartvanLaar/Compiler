using Parser.AbstractSyntaxTree.Expressions;

namespace Parser.AbstractSyntaxTree.Visitors
{
    internal interface IAbstractSyntaxTreeVisitorExecuter : IAbstractSyntaxTreeVisitor
    {
        string Execute(Queue<ExpressionBase> abstractSyntaxTrees, IByteCodeGeneratorListener listener);
    }
}
