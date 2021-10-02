using Parser.AbstractSyntaxTree;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Compiler
{
    internal interface IParser
    {
        void Parse(Lexer lexer);
    }

    internal class Parser : IParser
    {

        public void Parse(Lexer lexer)
        {
            var token = lexer.ConsumeToken();

            while (token.TokenType != TokenType.EndOfFile)
            {
                ParsePrimary(token);
                token = lexer.ConsumeToken();
            }
        }

        private ExpressionAST ParsePrimary(Token token)
        {
            switch (token.TokenType)
            {
                case TokenType.Identifier: return ParseIdentifierExpression(token);
                case TokenType.Double: return ParseDoubleExpression(token);
                case TokenType.Float: return ParseFloatExpression(token);
                case TokenType.Integer: return ParseIntegerExpression(token);
                case TokenType.String: return ParseStringExpression(token);
                case TokenType.Character: return ParseCharacterExpression(token);
                default: throw new InvalidOperationException("Encountered an unkown token when expeting an expression.");// todo: what to do here?                    
            }
        }

        private ExpressionAST ParseIdentifierExpression(Token token)
        {
            //todo: make smarter so it knows whether this is a function call? Or should the lexer handle this for me, as it can also peek forward...
            //todo: how to handle nullables?
            var result = new VariableEvaluationExpressionAST(token.Name);

            return result;
        }

        private ExpressionAST ParseFloatExpression(Token token)
        {
            //todo: how to handle nullables?
            var result = new FloatExpressionAST(token.FloatValue ?? 0f);

            return result;
        }

        private ExpressionAST ParseDoubleExpression(Token token)
        {
            //todo: how to handle nullables?
            var result = new DoubleExpressionAST(token.FloatValue ?? 0d);

            return result;
        }

        private ExpressionAST ParseIntegerExpression(Token token)
        {
            //todo: how to handle nullables?
            var result = new IntegerExpressionAST(token.IntegerValue ?? 0);

            return result;
        }

        private ExpressionAST ParseStringExpression(Token token)
        {
            //todo: how to handle nullables?
            var result = new StringExpressionAST(token.StringValue ?? string.Empty);

            return result;
        }

        private ExpressionAST ParseCharacterExpression(Token token)
        {
            if (token.StringValue?.Length > 1)
            {
                //todo: this should be a compiler error... how are we going to notify the user?
                throw new InvalidOperationException("Character can only have a length of 1.");
            }

            //todo: how to handle nullables?
            //should string value be filled?
            var result = new CharacterExpressionAST(token.StringValue?.FirstOrDefault() ?? '?');

            return result;
        }
    }
}
