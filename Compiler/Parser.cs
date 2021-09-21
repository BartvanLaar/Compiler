using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Compiler
{
    internal interface IParser
    {
        Task<IParserResults> ParseAsync(ILexerResults result);
    }

    internal interface IParserResults
    {

    }

    internal class ParserResults : IParserResults
    {

    }

    internal class Parser : IParser
    {

        public async Task<IParserResults> ParseAsync(ILexerResults result)
        {
            var results = new ParserResults();
            return results;
        }
    }
}
