﻿using Parser.CodeLexer;
using System.Diagnostics;

namespace Parser.AbstractSyntaxTree.Expressions
{
    public sealed class IntegerExpression : ExpressionBase
    {
        public IntegerExpression(Token token) : base(token, ExpressionType.Integer)
        {
            // todo: how to handle nullables?
            Debug.Assert(token.IntegerValue.HasValue);
            Value = token.IntegerValue.Value;
        }
        public int Value { get; }

    }
}
