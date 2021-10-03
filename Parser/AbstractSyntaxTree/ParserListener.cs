
using Parser.AbstractSyntaxTree.Expressions;

namespace Parser.AbstractSyntaxTree
{
    internal interface IParserListener
    {
        void EnterHandleAssignmentExpression(AssignmentExpression data);
        void ExitHandleAssignmentExpression(AssignmentExpression data);

        void EnterHandleTopLevelExpression(FunctionCallExpression data);
        void ExitHandleTopLevelExpression(FunctionCallExpression data);
    }

    internal sealed class ParserListener
    {
        private static readonly Type IParstenerListenerType = typeof(IParserListener);

        private readonly Stack<string> _descentStack = new();
        private readonly Stack<AbstractSyntaxTreeContext> _ascentStack = new();
        private readonly IParserListener _listener;

        public ParserListener(IParserListener listener)
        {
            _listener = listener;
        }

        public void EnterRule(string ruleName)
        {
            _descentStack.Push(ruleName);
        }

        public void ExitRule(ExpressionBase? expressionArgument)
        {
            var ruleName = _descentStack.Pop();
            const string EXIT = "Exit";
            const string ENTER = "Enter";
            _ascentStack.Push(new AbstractSyntaxTreeContext(IParstenerListenerType.GetMethod(EXIT + ruleName) ?? throw new Exception($"Provided {_listener.GetType().Name} is missing method: {EXIT + ruleName}"), _listener, expressionArgument));
            _ascentStack.Push(new AbstractSyntaxTreeContext(IParstenerListenerType.GetMethod(ENTER + ruleName) ?? throw new Exception($"Provided  {_listener.GetType().Name} is missing method: {ENTER + ruleName}"), _listener, expressionArgument));
        }

        public void Listen()
        {
            if(_listener == null)
            {
                return;
            }

            while (_ascentStack.Any())
            {
                var ctx = _ascentStack.Pop();
                ctx.MethodInfo.Invoke(ctx.Instance, new object?[] { ctx.ExpressionArgument });
            }
        }
    }
}
