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

    internal struct GrammarRule
    {
        public GrammarRule(TokenType tokenType)
        {
            TokenType = tokenType;
        }

        public TokenType TokenType { get; }
    }

    internal class GrammarDefinition
    {
        public GrammarRule CurrentRule { get; }
        public GrammarDefinition[] NextPossibleGrammars { get; internal set; }

        public GrammarDefinition(TokenType tokenType)
        {
            CurrentRule = new GrammarRule(tokenType);
            NextPossibleGrammars = Array.Empty<GrammarDefinition>();
        }
    }

    internal static class GrammarBuilder
    {
        public static GrammarDefinition BuildExpressionGrammar()
        {

            var numberGrammar = new GrammarDefinition(TokenType.Integer);
            var plusGrammar = new GrammarDefinition(TokenType.Plus)
            {
                NextPossibleGrammars = new GrammarDefinition[] {
                    numberGrammar
                }
            };

            numberGrammar.NextPossibleGrammars = new GrammarDefinition[] { plusGrammar };

            var assignmentGrammar = new GrammarDefinition(TokenType.Assignment);
            assignmentGrammar.NextPossibleGrammars = new GrammarDefinition[] { numberGrammar };

            var variableNameGrammar = new GrammarDefinition(TokenType.VariableName)
            {
                NextPossibleGrammars = new GrammarDefinition[]
                {
                    assignmentGrammar
                }
            };

            var start = new GrammarDefinition(TokenType.VariableTypeInferred)
            {
                NextPossibleGrammars = new GrammarDefinition[]
                {
                    variableNameGrammar,
                }
            };

            return start;
        }
    }

    internal static class GrammarDefinitions
    {
        internal static GrammarDefinition EXPRESSION = GrammarBuilder.BuildExpressionGrammar();

        public static GrammarDefinition[] StartingGrammarDefinitions = new GrammarDefinition[]
        {
            EXPRESSION,
        };
    }



    internal class Parser : IParser
    {

        public IParserResults Parse(Lexer lexer)
        {
            var token = lexer.ConsumeToken();

            while(token.TokenType != TokenType.EndOfFile)
            {
                ProcessToken(token);
                token = lexer.ConsumeToken();
            }
            var results = new ParserResults();
            return results;
        }

        private void ProcessToken(Token token)
        {
            throw new NotImplementedException();
        }
    }
}
