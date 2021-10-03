using Parser.AbstractSyntaxTree;
using Parser.AbstractSyntaxTree.Expressions;
using Parser.CodeLexer;

namespace Parser
{
    public class Driver
    {
        public static void Run(string text) => Run(text, new ConsoleParserListener());

        internal static void Run(string text, IParserListener listener)
        {
            var lexer = new Lexer(text);
            var parser = new Parser(lexer, listener);
            parser.Parse();
        }

        private class ConsoleParserListener : IParserListener
        {
            public void EnterHandleAssignmentExpression(AssignmentExpression data)
            {
                WriteStart(data);
            }
            public void ExitHandleAssignmentExpression(AssignmentExpression data)
            {
                WriteFinish(data);
            }

            public void EnterHandleTopLevelExpression(FunctionCallExpression data)
            {
                WriteStart(data);
            }

            public void ExitHandleTopLevelExpression(FunctionCallExpression data)
            {
                WriteFinish(data);
            }

            private static void WriteStart(ExpressionBase data)
            {
                Console.WriteLine($"Start parsing {data.NodeExpressionType}");
            }

            private static void WriteFinish(ExpressionBase data)
            {
                Console.WriteLine($"Finished parsing {data.NodeExpressionType}");
            }
        }

    }
}
