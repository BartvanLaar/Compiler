using NUnit.Framework;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace Compiler.Tests
{
    internal class LexerTests
    {

        [Test]
        public async Task Lexer_Test()
        {
            var text = "this is a text divided into 8 tokens";
            var lexer = new Lexer(text);
            var toks = lexer.ConsumeTokens(8);
            Assert.AreEqual(8, toks.Length);

            toks = lexer.ConsumeTokens(7);
            Assert.AreEqual(7, toks.Length);


            Assert.Throws<ArgumentException>(() => lexer.ConsumeTokens(-1));

            toks = lexer.ConsumeTokens(9);
            Assert.AreEqual(9, toks.Length);
            Assert.AreEqual(TokenType.EndOfFile, toks.Last().TokenType);

            toks = lexer.ConsumeTokens(10);
            Assert.AreEqual(10, toks.Length);
            Assert.AreEqual(TokenType.EndOfFile, toks.Last().TokenType);
        }


    }
}
