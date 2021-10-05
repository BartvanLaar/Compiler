using NUnit.Framework;
using Parser.CodeLexer;
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
            Assert.AreEqual(8, toks[6].IntegerValue);
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
            Assert.AreEqual(8, toks[4].IntegerValue);
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
        [TestCase("+", TokenType.Add)]
        [TestCase("+=", TokenType.AddAssign)]
        [TestCase("-", TokenType.Subtract)]
        [TestCase("-=", TokenType.SubtractAssign)]
        [TestCase("%", TokenType.Modulo)]
        [TestCase("%=", TokenType.ModuloAssignment)]
        [TestCase("*", TokenType.Multiply)]
        [TestCase("*=", TokenType.MultiplyAssign)]
        [TestCase("/", TokenType.Divide)]
        [TestCase("/=", TokenType.DivideAssign)]
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
        [TestCase("+ +", TokenType.Add)]
        [TestCase("+= +=", TokenType.AddAssign)]
        [TestCase("- -", TokenType.Subtract)]
        [TestCase("-= -=", TokenType.SubtractAssign)]
        [TestCase("% %", TokenType.Modulo)]
        [TestCase("%= %=", TokenType.ModuloAssignment)]
        [TestCase("* *", TokenType.Multiply)]
        [TestCase("*= *=", TokenType.MultiplyAssign)]
        [TestCase("/ /", TokenType.Divide)]
        [TestCase("/= /=", TokenType.DivideAssign)]
        [TestCase(": :", TokenType.TerniaryOperatorFalse)]
        [TestCase("? ?", TokenType.TerniaryOperatorTrue)]
        [TestCase("?? ??", TokenType.NullableCoalesce)]
        [TestCase("// \n //", TokenType.Comment)]
        [TestCase("/// \n ///", TokenType.Summary)]
        [TestCase("// \r\n //", TokenType.Comment)]
        [TestCase("/// \r\n ///", TokenType.Summary)]
        public static void Lexer_Test_Two_SingleTokens(string text, TokenType expectedTokenType)
        {
            var lexer = new Lexer(text);
            var toks = lexer.ConsumeTokens(3);

            Assert.AreEqual(3, toks.Length);
            Assert.AreEqual(expectedTokenType, toks[0].TokenType);
            Assert.AreEqual(expectedTokenType, toks[1].TokenType);
            Assert.AreEqual(TokenType.EndOfFile, toks[2].TokenType);
        }

        [TestCase("\"t\"c", ExpectedResult = "t")]
        [TestCase("\"t t\"c", ExpectedResult = "t t")]
        [TestCase("\"t \\n t\"c", ExpectedResult = "t \\n t")]//todo: is this even right? Answer, yes, length check can be done in parser... We shouldn't care about that here... I think
        [TestCase("\"\"c", ExpectedResult = "")]
        public static string? Lexer_Test_Character(string text)
        {
            var lexer = new Lexer(text);
            var toks = lexer.ConsumeTokens(text.Length + 1);
            Assert.IsTrue(toks.First().TokenType is TokenType.Character);
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
            Assert.AreEqual(TokenType.VariableDeclaration, toks[0].TokenType);
            Assert.AreEqual(TokenType.Identifier, toks[1].TokenType);
            Assert.AreEqual(TokenType.Assignment, toks[2].TokenType);
            Assert.AreEqual(TokenType.Integer, toks[3].TokenType);
            Assert.AreEqual(20, toks[3].IntegerValue);
            Assert.AreEqual(TokenType.Add, toks[4].TokenType);
            Assert.AreEqual(TokenType.Integer, toks[5].TokenType);
            Assert.AreEqual(5, toks[5].IntegerValue);
            Assert.AreEqual(TokenType.EndOfStatement, toks[6].TokenType);
            Assert.AreEqual(TokenType.EndOfFile, lexer.PeekToken().TokenType);
        }

        [TestCase("// this text is to be ignored", 1)]
        [TestCase("// this text is to be ignored // so is this text ", 1)]
        [TestCase("// this text is to be ignored \r // so is this text ", 1)]
        [TestCase("// this text is to be ignored \n // so is this text ", 2)]
        [TestCase("// this text is to be ignored \r\n // so is this text ", 2)]
        public static void Lexer_Test_Comments(string text, int commentTokCount)
        {
            var lexer = new Lexer(text);
            var toks = lexer.ConsumeTokens(50);
            Assert.AreEqual(commentTokCount, toks.Count(x => x.TokenType is TokenType.Comment));
            Assert.AreEqual(toks.Length - commentTokCount, toks.Count(x => x.TokenType is not TokenType.Comment));
        }

        [TestCase("// this text is to be ignored \n var x = 10;", 1)]
        [TestCase("// this text is to be ignored \n // so is this text \n var x = 10;", 2)]
        [TestCase("// this text is to be ignored \n // so is this text \r\n var x = 10;", 2)]
        public static void Lexer_Test_Comments_And_Code(string text, int commentTokCount)
        {
            var lexer = new Lexer(text);
            var toks = lexer.ConsumeTokens(commentTokCount);
            Assert.AreEqual(commentTokCount, toks.Count(x => x.TokenType is TokenType.Comment));
            Assert.AreEqual(TokenType.VariableDeclaration, lexer.ConsumeToken().TokenType);
            Assert.AreEqual(TokenType.Identifier, lexer.ConsumeToken().TokenType);
            Assert.AreEqual(TokenType.Assignment, lexer.ConsumeToken().TokenType);
            Assert.AreEqual(TokenType.Integer, lexer.ConsumeToken().TokenType);
            Assert.AreEqual(TokenType.EndOfStatement, lexer.ConsumeToken().TokenType);
        }

        [TestCase("/// this text is to be ignored", 1)]
        [TestCase("/// this text is to be ignored /// so is this text ", 1)]
        [TestCase("/// this text is to be ignored \r /// so is this text ", 1)]
        [TestCase("/// this text is to be ignored \n /// so is this text ", 2)]
        [TestCase("/// this text is to be ignored \r\n /// so is this text ", 2)]
        public static void Lexer_Test_Summaries(string text, int commentTokCount)
        {
            var lexer = new Lexer(text);
            var toks = lexer.ConsumeTokens(50);
            Assert.AreEqual(commentTokCount, toks.Count(x => x.TokenType is TokenType.Summary));
            Assert.AreEqual(toks.Length - commentTokCount, toks.Count(x => x.TokenType is not TokenType.Summary));
        }

        [TestCase("/// this text is to be ignored \n var x = 10;", 1)]
        [TestCase("/// this text is to be ignored \n /// so is this text \n var x = 10;", 2)]
        [TestCase("/// this text is to be ignored \n /// so is this text \r\n var x = 10;", 2)]
        public static void Lexer_Test_Summaries_And_Code(string text, int commentTokCount)
        {
            var lexer = new Lexer(text);
            var toks = lexer.ConsumeTokens(commentTokCount);
            Assert.AreEqual(commentTokCount, toks.Count(x => x.TokenType is TokenType.Summary));
            Assert.AreEqual(TokenType.VariableDeclaration, lexer.ConsumeToken().TokenType);
            Assert.AreEqual(TokenType.Identifier, lexer.ConsumeToken().TokenType);
            Assert.AreEqual(TokenType.Assignment, lexer.ConsumeToken().TokenType);
            Assert.AreEqual(TokenType.Integer, lexer.ConsumeToken().TokenType);
            Assert.AreEqual(TokenType.EndOfStatement, lexer.ConsumeToken().TokenType);
        }

        [TestCase("x + 7;")]
        [TestCase("x+7;")]
        public static void Lexer_Test_Use_Variable(string code)
        {
            var lexer = new Lexer(code);
            var toks = lexer.ConsumeTokens(5);

            Assert.AreEqual(TokenType.Identifier, toks[0].TokenType);
            Assert.AreEqual(TokenType.Add, toks[1].TokenType);
            Assert.AreEqual(TokenType.Integer, toks[2].TokenType);
            Assert.AreEqual(TokenType.EndOfStatement, toks[3].TokenType);
            Assert.AreEqual(TokenType.EndOfFile, toks[4].TokenType);
        }

        [Test]
        public static void Lexer_Test_Assign_And_Use_Variable_No_Whitespace_EndOfStatement()
        {
            var lexer = new Lexer("var x=x+7;");
            var toks = lexer.ConsumeTokens(8);

            Assert.AreEqual(TokenType.VariableDeclaration, toks[0].TokenType);
            Assert.AreEqual(TokenType.Identifier, toks[1].TokenType);
            Assert.AreEqual(TokenType.Assignment, toks[2].TokenType);
            Assert.AreEqual(TokenType.Identifier, toks[3].TokenType);
            Assert.AreEqual(TokenType.Add, toks[4].TokenType);
            Assert.AreEqual(TokenType.Integer, toks[5].TokenType);
            Assert.AreEqual(TokenType.EndOfStatement, toks[6].TokenType);
            Assert.AreEqual(TokenType.EndOfFile, toks[7].TokenType);
        }

        [Test]
        public static void Lexer_Test_Assign_And_Use_Variable_No_Whitespace_EndOfFile()
        {
            var lexer = new Lexer("var x=x+7");
            var toks = lexer.ConsumeTokens(8);

            Assert.AreEqual(TokenType.VariableDeclaration, toks[0].TokenType);
            Assert.AreEqual(TokenType.Identifier, toks[1].TokenType);
            Assert.AreEqual(TokenType.Assignment, toks[2].TokenType);
            Assert.AreEqual(TokenType.Identifier, toks[3].TokenType);
            Assert.AreEqual(TokenType.Add, toks[4].TokenType);
            Assert.AreEqual(TokenType.Integer, toks[5].TokenType);
            Assert.AreEqual(TokenType.EndOfFile, toks[6].TokenType);
            Assert.AreEqual(TokenType.EndOfFile, toks[7].TokenType);
        }

        [TestCase("var x=x+(7-(8+2));")]
        [TestCase("var x = x + ( 7 - ( 8 + 2 ) ) ;")]
        public static void Lexer_Test_Assign_And_Use_Variable_No_Whitespace_PrecedenceOperators(string code)
        {
            var lexer = new Lexer(code);
            var toks = lexer.ConsumeTokens(16);

            Assert.AreEqual(TokenType.VariableDeclaration, toks[0].TokenType);
            Assert.AreEqual(TokenType.Identifier, toks[1].TokenType);
            Assert.AreEqual(TokenType.Assignment, toks[2].TokenType);
            Assert.AreEqual(TokenType.Identifier, toks[3].TokenType);
            Assert.AreEqual(TokenType.Add, toks[4].TokenType);
            Assert.AreEqual(TokenType.ParanthesesOpen, toks[5].TokenType);
            Assert.AreEqual(TokenType.Integer, toks[6].TokenType);
            Assert.AreEqual(TokenType.Subtract, toks[7].TokenType);
            Assert.AreEqual(TokenType.ParanthesesOpen, toks[8].TokenType);
            Assert.AreEqual(TokenType.Integer, toks[9].TokenType);
            Assert.AreEqual(TokenType.Add, toks[10].TokenType);
            Assert.AreEqual(TokenType.Integer, toks[11].TokenType);
            Assert.AreEqual(TokenType.ParanthesesClose, toks[12].TokenType);
            Assert.AreEqual(TokenType.ParanthesesClose, toks[13].TokenType);
            Assert.AreEqual(TokenType.EndOfStatement, toks[14].TokenType);
            Assert.AreEqual(TokenType.EndOfFile, toks[15].TokenType);
        }

        [Test]
        public static void Lexer_Test_Declare_Function() // Known broken, fix... ~Bart 05-10-2021
        {
            var lexer = new Lexer("def func SomeFunc() -> void {}");

            var toks = lexer.ConsumeTokens(8);
            Assert.AreEqual(TokenType.FunctionDefinition, toks[0].TokenType);
            Assert.AreEqual(TokenType.Identifier, toks[1].TokenType);
            Assert.AreEqual(TokenType.ParanthesesOpen, toks[2].TokenType);
            Assert.AreEqual(TokenType.ParanthesesClose, toks[3].TokenType);
            Assert.AreEqual(TokenType.GreaterThan, toks[4].TokenType);
            Assert.AreEqual(TokenType.AccoladesOpen, toks[5].TokenType);
            Assert.AreEqual(TokenType.AccoladesClose, toks[6].TokenType);
        }
    }
}
