using Parser.CodeLexer;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Parser.AbstractSyntaxTree.Expressions
{
    internal class AssignmentExpression : ExpressionBase
    {
        
        public AssignmentExpression(Token declarationTypeToken, ExpressionBase identificationExpression, Token assignmentToken, ExpressionBase valueExpression) : base(declarationTypeToken, ExpressionType.Assignment) // todo: do we/should we pass a token to the base?
        {
            DeclarationTypeToken = declarationTypeToken;
            IdentificationExpression = identificationExpression;
            AssignmentToken = assignmentToken;
            ValueExpression = valueExpression;
        }

        public Token DeclarationTypeToken { get; }
        public Token AssignmentToken { get; }
        public ExpressionBase IdentificationExpression {  get; }
        public ExpressionBase ValueExpression {  get; }

        protected internal override ExpressionBase? Accept(ExpressionVisitor visitor)
        {
            return visitor.VisitAssignmentExpression(this);
        }
    }
}
