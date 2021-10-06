﻿using Parser.CodeLexer;

namespace Parser.AbstractSyntaxTree.Expressions
{
    public sealed class IdentifierExpression : ExpressionBase
    {
        public IdentifierExpression(Token token) : base(token, ExpressionType.Identifier)
        {
            Identifier = token.Name;
        }

        public string Identifier { get; }

    }
}
