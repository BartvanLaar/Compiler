using NUnit.Framework;
using System.Linq;
using System.Threading.Tasks;

namespace Compiler.Tests
{
    internal class LexerTests
    {
        private static readonly Lexer _lexer = new();

        [Test]
        public async Task Lexer_Test()
        {
            var text = string.Empty;
            var result = await _lexer.LexAsync(text);
            var tokens = result.Tokens;
            Assert.AreEqual(1, tokens.Length);
            Assert.AreEqual(TokenType.EndOfFile, tokens.First().TokenType);

            text = "var stuff 20.0";
            result = await _lexer.LexAsync(text);
            tokens = result.Tokens;
            Assert.AreEqual(4, tokens.Length);
            Assert.AreEqual(text.Length - tokens.Last().Value.Length + 1, tokens.Last().Column);

            text = "var stuff 20.0f";
            result = await _lexer.LexAsync(text);
            tokens = result.Tokens;
            Assert.AreEqual(4, tokens.Length);

            text = "var stuff 20.0i";
            result = await _lexer.LexAsync(text);
            tokens = result.Tokens;
            Assert.AreEqual(4, tokens.Length);

            text = "var stuff 20.0d";
            result = await _lexer.LexAsync(text);
            tokens = result.Tokens;
            Assert.AreEqual(4, tokens.Length);
        }

        [Test]
        public async Task Lexer_Test_Columns()
        {
            var text = "var stuff 20.0";

            var result = await _lexer.LexAsync(text);
            var tokens = result.Tokens;
            Assert.AreEqual(4, tokens.Length);
            Assert.AreEqual(1, tokens.First().Column);
            Assert.AreEqual(text.Length - tokens.Last().Value.Length + 1, tokens.Last().Column);
        }

        [Test]
        public async Task Lexer_Test_EndWithNewLine()
        {
            var text = "stuff ";
            var result = await _lexer.LexAsync(text);
            var tokens = result.Tokens;
            Assert.AreEqual(1, tokens.Last().Row);

            text = "stuff \n ";
            result = await _lexer.LexAsync(text);
            tokens = result.Tokens;
            Assert.AreEqual(2, tokens.Last().Row);

            text = "stuff \n stuff \n ";
            result = await _lexer.LexAsync(text);
            tokens = result.Tokens;
            Assert.AreEqual(3, tokens.Last().Row);
        }

        [Test]
        public async Task Lexer_Test_NewLine()
        {
            var text = " stuff ";
            var result = await _lexer.LexAsync(text);
            var tokens = result.Tokens;
            Assert.AreEqual(1, tokens.Last().Row);

            text = "stuff \n 20d";

            result = await _lexer.LexAsync(text);
            tokens = result.Tokens;
            Assert.AreEqual(2, tokens.Last().Row);

            text = "stuff \n 20d ";

            result = await _lexer.LexAsync(text);
            tokens = result.Tokens;
            Assert.AreEqual(2, tokens.Last().Row);
        }

        [Test]
        public async Task Lexer_Test_Hex()
        {
            var text = " 0xFFdd90 ";
            var result = await _lexer.LexAsync(text);
            var tokens = result.Tokens;
            Assert.AreEqual(2, tokens.Length);

            text = "0xFFdd90 ";
            result = await _lexer.LexAsync(text);
            tokens = result.Tokens;
            Assert.AreEqual(2, tokens.Length);

            text = " 0xFFdd90";
            result = await _lexer.LexAsync(text);
            tokens = result.Tokens;
            Assert.AreEqual(2, tokens.Length);
        }
    }
}
