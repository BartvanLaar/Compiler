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

            while(token.TokenType != TokenType.EndOfFile)
            {
                ProcessToken(token);
                token = lexer.ConsumeToken();
            }
        }

        private void ProcessToken(Token token)
        {
            throw new NotImplementedException();
        }
    }
}
