using NUnit.Framework;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace Compiler.Tests
{
    internal class LexerTests
    {

        [TestCase("this is a text divided into 8 tokens")]
        [TestCase("this is a text \n divided into 8 tokens")]
        [TestCase("this is a text\ndivided into 8 tokens")]
        [TestCase("this is a text \ndivided into 8 tokens")]
        [TestCase("this is a text\n divided into 8 tokens")]
        [TestCase("this is a text \r\n divided into 8 tokens")]
        [TestCase("this is a text\r\ndivided into 8 tokens")]
        [TestCase("this is a text \r\ndivided into 8 tokens")]
        [TestCase("this is a text\r\n divided into 8 tokens")]
        public void Lexer_Test(string text)
        {
            var lexer = new Lexer(text);
            var toks = lexer.PeekTokens(8);
            Assert.AreEqual(8, toks.Length);
            Assert.AreEqual("this", toks[0].Name);
            Assert.AreEqual("is", toks[1].Name);
            Assert.AreEqual("a", toks[2].Name);
            Assert.AreEqual("text", toks[3].Name);
            Assert.AreEqual("divided", toks[4].Name);
            Assert.AreEqual("into", toks[5].Name);
            Assert.AreEqual("8", toks[6].Name);
            Assert.AreEqual("tokens", toks[7].Name);

            lexer.PeekTokens(100);

            for (int i = 0; i < 100; i++)
            {
                lexer.PeekToken();
            }

            toks = lexer.PeekTokens(7);
            Assert.AreEqual(7, toks.Length);

            toks = lexer.PeekTokens(9);
            Assert.AreEqual(9, toks.Length);

            var tok = lexer.PeekToken();
            Assert.AreEqual("this", tok.Name);

            toks = lexer.PeekTokens(1);
            Assert.AreEqual("this", toks.First().Name);

            tok = lexer.ConsumeToken();
            Assert.AreEqual("this", tok.Name);

            toks = lexer.ConsumeTokens(1);
            Assert.AreEqual("is", toks.First().Name);

            toks = lexer.ConsumeTokens(6);
            Assert.IsFalse(toks.Any(t => t.TokenType == TokenType.EndOfFile));
            Assert.AreEqual(6, toks.Length);
            Assert.AreEqual("a", toks[0].Name);
            Assert.AreEqual("text", toks[1].Name);
            Assert.AreEqual("divided", toks[2].Name);
            Assert.AreEqual("into", toks[3].Name);
            Assert.AreEqual("8", toks[4].Name);
            Assert.AreEqual("tokens", toks[5].Name);

            toks = lexer.ConsumeTokens(1);
            Assert.AreEqual(1, toks.Length);
            Assert.AreEqual(TokenType.EndOfFile, toks.First().TokenType);

            toks = lexer.PeekTokens(1);
            Assert.AreEqual(1, toks.Length);
            Assert.AreEqual(TokenType.EndOfFile, toks.First().TokenType);
            Assert.Throws<ArgumentException>(() => lexer.ConsumeTokens(-1));

            tok = lexer.PeekToken();
            Assert.AreEqual(TokenType.EndOfFile, tok.TokenType);

            tok = lexer.ConsumeToken();
            Assert.AreEqual(TokenType.EndOfFile, tok.TokenType);
        }

    }
}
