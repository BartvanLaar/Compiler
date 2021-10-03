using NUnit.Framework;
using Parser.Lexer;
using System;
using System.Linq;

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
        public static void Lexer_Test(string text)
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


        [TestCase("=", TokenType.Assignment)]
        [TestCase("==", TokenType.Equivalent)]
        [TestCase("===", TokenType.Equals)]
        [TestCase("!", TokenType.BooleanInvert)]
        [TestCase("!=", TokenType.NotEquivalent)]
        [TestCase("!==", TokenType.NotEquals)]
        [TestCase("<", TokenType.LessThan)]
        [TestCase("<=", TokenType.LessThanOrEqualTo)]
        [TestCase(">", TokenType.GreaterThan)]
        [TestCase(">=", TokenType.GreaterThanOrEqualTo)]
        [TestCase("+", TokenType.Plus)]
        [TestCase("+=", TokenType.PlusAssignment)]
        [TestCase("-", TokenType.Minus)]
        [TestCase("-=", TokenType.MinusAssignment)]
        [TestCase("%", TokenType.Modulo)]
        [TestCase("%=", TokenType.ModuloAssignment)]
        [TestCase("*", TokenType.Times)]
        [TestCase("*=", TokenType.TimesAssignment)]
        [TestCase("/", TokenType.Divide)]
        [TestCase("/=", TokenType.DivideAssignment)]
        [TestCase(":", TokenType.TerniaryOperatorFalse)]
        [TestCase("?", TokenType.TerniaryOperatorTrue)]
        [TestCase("??", TokenType.NullableCoalesce)]
        [TestCase("//", TokenType.Comment)]
        [TestCase("///", TokenType.Summary)]
        public static void Lexer_Test_SingleToken(string text, TokenType expectedTokenType)
        {
            var lexer = new Lexer(text);
            var toks = lexer.ConsumeTokens(2);

            Assert.AreEqual(2, toks.Length);
            Assert.AreEqual(expectedTokenType, toks.First().TokenType);
            Assert.AreEqual(TokenType.EndOfFile, toks.Last().TokenType);
        }

        [TestCase("= =", TokenType.Assignment)]
        [TestCase("== ==", TokenType.Equivalent)]
        [TestCase("=== ===", TokenType.Equals)]
        [TestCase("! !", TokenType.BooleanInvert)]
        [TestCase("!= !=", TokenType.NotEquivalent)]
        [TestCase("!== !==", TokenType.NotEquals)]
        [TestCase("< <", TokenType.LessThan)]
        [TestCase("<= <=", TokenType.LessThanOrEqualTo)]
        [TestCase("> >", TokenType.GreaterThan)]
        [TestCase(">= >=", TokenType.GreaterThanOrEqualTo)]
        [TestCase("+ +", TokenType.Plus)]
        [TestCase("+= +=", TokenType.PlusAssignment)]
        [TestCase("- -", TokenType.Minus)]
        [TestCase("-= -=", TokenType.MinusAssignment)]
        [TestCase("% %", TokenType.Modulo)]
        [TestCase("%= %=", TokenType.ModuloAssignment)]
        [TestCase("* *", TokenType.Times)]
        [TestCase("*= *=", TokenType.TimesAssignment)]
        [TestCase("/ /", TokenType.Divide)]
        [TestCase("/= /=", TokenType.DivideAssignment)]
        [TestCase(": :", TokenType.TerniaryOperatorFalse)]
        [TestCase("? ?", TokenType.TerniaryOperatorTrue)]
        [TestCase("?? ??", TokenType.NullableCoalesce)]
        [TestCase("// //", TokenType.Comment)]
        [TestCase("/// ///", TokenType.Summary)]
        public static void Lexer_Test_Two_SingleTokens(string text, TokenType expectedTokenType)
        {
            var lexer = new Lexer(text);
            var toks = lexer.ConsumeTokens(3);

            Assert.AreEqual(3, toks.Length);
            Assert.AreEqual(expectedTokenType, toks[0].TokenType);
            Assert.AreEqual(expectedTokenType, toks[1].TokenType);
            Assert.AreEqual(TokenType.EndOfFile, toks[2].TokenType);
        }


        [TestCase("\'t\'", ExpectedResult ="t")]
        [TestCase("\'t t\'", ExpectedResult ="t t")]
        [TestCase("\'t \\n t\'", ExpectedResult ="t \\n t")]//todo: is this even right?
        [TestCase("\'\'", ExpectedResult = "")]
        public static string? Lexer_Test_Character(string text)
        {
            var lexer = new Lexer(text);
            var toks = lexer.ConsumeTokens(text.Length + 1);
            return toks.First().StringValue;
        }


        [TestCase("\"t\"", ExpectedResult = "t")]
        [TestCase("\"t t\"", ExpectedResult = "t t")]
        [TestCase("\"\"", ExpectedResult = "")]
        public static string? Lexer_Test_String(string text)
        {
            var lexer = new Lexer(text);
            var toks = lexer.ConsumeTokens(2);
            Assert.AreEqual(TokenType.EndOfFile, toks.Last().TokenType);
            return toks.First().StringValue;
        }

        [Test]
        public static void Lexer_Test_Code()
        {
            var code = "var variableName = 20 + 5;";
            var lexer = new Lexer(code);

            var toks = lexer.ConsumeTokens(7);
            Assert.AreEqual(TokenType.VariableTypeInferred, toks[0].TokenType);
            Assert.AreEqual(TokenType.Identifier, toks[1].TokenType);
            Assert.AreEqual(TokenType.Assignment, toks[2].TokenType);
            Assert.AreEqual(TokenType.Integer, toks[3].TokenType);
            Assert.AreEqual(TokenType.Plus, toks[4].TokenType);
            Assert.AreEqual(TokenType.Integer, toks[5].TokenType);
            Assert.AreEqual(TokenType.EndOfStatement, toks[6].TokenType);
            Assert.AreEqual(TokenType.EndOfFile, lexer.PeekToken().TokenType);
        }
    }
}
