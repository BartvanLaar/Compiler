using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Compiler
{
    internal interface IParser
    {
        IParserResults Parse(Lexer lexer);
    }

    internal interface IParserResults
    {

    }

    internal class ParserResults : IParserResults
    {

    }

    internal class Parser : IParser
    {

        public IParserResults Parse(Lexer lexer)
        {
            var results = new ParserResults();
            return results;
        }
    }
}
