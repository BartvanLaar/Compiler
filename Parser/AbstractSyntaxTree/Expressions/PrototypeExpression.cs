using Parser.CodeLexer;

namespace Parser.AbstractSyntaxTree.Expressions
{
    //todo: name is copied from example on the internet.. What is this actually?
    internal sealed class PrototypeExpression : ExpressionBase
    {
        public PrototypeExpression(Token token, Token[] arguments) : base(token, ExpressionType.Prototype)
        {
            Name = token.Name;
            Arguments = arguments;
        }

        public string Name { get; }
        public Token[] Arguments { get; }

        public string[] ArgumentNames => Arguments.Select(t => t.Name).ToArray();

        protected internal override ExpressionBase Accept(ExpressionVisitor visitor)
        {
            return visitor.VisitPrototypeAST(this);
        }
    }
}
