using Parsing.AbstractSyntaxTree.Expressions;
using System.Reflection;

namespace Parsing.AbstractSyntaxTree
{
    internal sealed class AbstractSyntaxTreeContext
    {

        public AbstractSyntaxTreeContext(MethodInfo methodInfo, object instance, ExpressionBase? expressionArgument)
        {
            MethodInfo = methodInfo;
            Instance = instance;
            ExpressionArgument = expressionArgument;
        }

        public MethodInfo MethodInfo { get; }
        public object Instance { get; }
        public ExpressionBase? ExpressionArgument { get; }
    }
}
