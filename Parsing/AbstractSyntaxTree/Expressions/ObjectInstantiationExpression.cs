using Lexing;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Parsing.AbstractSyntaxTree.Expressions
{
    public class ObjectInstantiationExpression : ValueExpressionBase
    {
        public ObjectInstantiationExpression(Token objectToken, IdentifierExpression classIdentifier, ExpressionBase[] arguments) 
            // todo: is below base code right??
            : base(objectToken, classIdentifier.TypeToken)
        {
            ClassIdentifier = classIdentifier;
            Arguments = arguments;
        }

        public IdentifierExpression ClassIdentifier { get; }
        public ExpressionBase[] Arguments { get; }
        public override ExpressionType DISCRIMINATOR => ExpressionType.ObjectInstantiation;
    }
}
