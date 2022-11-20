namespace Lexing.Tests
{
    public class LexerTests
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
            Assert.That(toks.Length, Is.EqualTo(8));
            Assert.That(toks[0].Name, Is.EqualTo("this"));
            Assert.That(toks[1].Name, Is.EqualTo("is"));
            Assert.That(toks[2].Name, Is.EqualTo("a"));
            Assert.That(toks[3].Name, Is.EqualTo("text"));
            Assert.That(toks[4].Name, Is.EqualTo("divided"));
            Assert.That(toks[5].Name, Is.EqualTo("into"));
            Assert.That(toks[6].Value, Is.EqualTo(8));
            Assert.That(toks[7].Name, Is.EqualTo("tokens"));

            lexer.PeekTokens(100);

            for (int i = 0; i < 100; i++)
            {
                lexer.PeekToken();
            }

            toks = lexer.PeekTokens(7);
            Assert.That(toks.Length, Is.EqualTo(7));

            toks = lexer.PeekTokens(9);
            Assert.That(toks.Length, Is.EqualTo(9));

            var tok = lexer.PeekToken();
            Assert.That(tok.Name, Is.EqualTo("this"));

            toks = lexer.PeekTokens(1);
            Assert.That(toks.First().Name, Is.EqualTo("this"));

            tok = lexer.ConsumeToken();
            Assert.That(tok.Name, Is.EqualTo("this"));

            toks = lexer.ConsumeTokens(1);
            Assert.That(toks.First().Name, Is.EqualTo("is"));

            toks = lexer.ConsumeTokens(6);
            Assert.IsFalse(toks.Any(t => t.TokenType == TokenType.EndOfFile));
            Assert.That(toks.Length, Is.EqualTo(6));
            Assert.That(toks[0].Name, Is.EqualTo("a"));
            Assert.That(toks[1].Name, Is.EqualTo("text"));
            Assert.That(toks[2].Name, Is.EqualTo("divided"));
            Assert.That(toks[3].Name, Is.EqualTo("into"));
            Assert.That(toks[4].Value, Is.EqualTo(8));
            Assert.That(toks[5].Name, Is.EqualTo("tokens"));

            toks = lexer.ConsumeTokens(1);
            Assert.That(toks.Length, Is.EqualTo(1));
            Assert.That(toks.First().TokenType, Is.EqualTo(TokenType.EndOfFile));

            toks = lexer.PeekTokens(1);
            Assert.That(toks.Length, Is.EqualTo(1));
            Assert.That(toks.First().TokenType, Is.EqualTo(TokenType.EndOfFile));
            Assert.Throws<ArgumentException>(() => lexer.ConsumeTokens(-1));

            tok = lexer.PeekToken();
            Assert.That(tok.TokenType, Is.EqualTo(TokenType.EndOfFile));

            tok = lexer.ConsumeToken();
            Assert.That(tok.TokenType, Is.EqualTo(TokenType.EndOfFile));
        }


        [TestCase("=", TokenType.Assignment)]
        [TestCase("==", TokenType.Equivalent)]
        [TestCase("===", TokenType.Equals)]
        [TestCase("!", TokenType.BooleanInvert)]
        [TestCase("!=", TokenType.NotEquivalent)]
        [TestCase("!==", TokenType.NotEquals)]
        [TestCase("<", TokenType.LessThan)]
        [TestCase("<=", TokenType.LessThanEqual)]
        [TestCase(">", TokenType.GreaterThan)]
        [TestCase(">=", TokenType.GreaterThanEqual)]
        [TestCase("+", TokenType.Add)]
        [TestCase("+=", TokenType.AddAssign)]
        [TestCase("-", TokenType.Subtract)]
        [TestCase("-=", TokenType.SubtractAssign)]
        [TestCase("%", TokenType.Modulo)]
        [TestCase("*", TokenType.Multiply)]
        [TestCase("*=", TokenType.MultiplyAssign)]
        [TestCase("/", TokenType.Divide)]
        [TestCase("/=", TokenType.DivideAssign)]
        [TestCase(":", TokenType.TerniaryOperatorFalse)]
        [TestCase("?", TokenType.TerniaryOperatorTrue)]
        [TestCase("??", TokenType.NullableCoalesce)]
        [TestCase("//", TokenType.Comment)]
        [TestCase("///", TokenType.Summary)]
        [TestCase("--", TokenType.SubtractSubtract)]
        [TestCase("++", TokenType.AddAdd)]
        [TestCase("<<", TokenType.BitShiftLeft)]
        [TestCase(">>", TokenType.BitShiftRight)]
        public static void Lexer_Test_SingleToken(string text, TokenType expectedTokenType)
        {
            var lexer = new Lexer(text);
            var toks = lexer.ConsumeTokens(2);
                        
            Assert.That(toks.Length, Is.EqualTo(2));
            Assert.That(toks.First().TokenType, Is.EqualTo(expectedTokenType));
            Assert.That(toks.Last().TokenType, Is.EqualTo(TokenType.EndOfFile));
        }

        [TestCase("= =", TokenType.Assignment)]
        [TestCase("== ==", TokenType.Equivalent)]
        [TestCase("=== ===", TokenType.Equals)]
        [TestCase("! !", TokenType.BooleanInvert)]
        [TestCase("!= !=", TokenType.NotEquivalent)]
        [TestCase("!== !==", TokenType.NotEquals)]
        [TestCase("< <", TokenType.LessThan)]
        [TestCase("<= <=", TokenType.LessThanEqual)]
        [TestCase("> >", TokenType.GreaterThan)]
        [TestCase(">= >=", TokenType.GreaterThanEqual)]
        [TestCase("+ +", TokenType.Add)]
        [TestCase("+= +=", TokenType.AddAssign)]
        [TestCase("- -", TokenType.Subtract)]
        [TestCase("-= -=", TokenType.SubtractAssign)]
        [TestCase("% %", TokenType.Modulo)]
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
        [TestCase("-- \r\n --", TokenType.SubtractSubtract)]
        [TestCase("++ \r\n ++", TokenType.AddAdd)]
        [TestCase("<< \n <<", TokenType.BitShiftLeft)]
        [TestCase("<< \r\n <<", TokenType.BitShiftLeft)]
        [TestCase("<< <<", TokenType.BitShiftLeft)]
        [TestCase(">> \n >>", TokenType.BitShiftRight)]
        [TestCase(">> \r\n >>", TokenType.BitShiftRight)]
        [TestCase(">> >>", TokenType.BitShiftRight)]
        public static void Lexer_Test_Two_SingleTokens(string text, TokenType expectedTokenType)
        {
            var lexer = new Lexer(text);
            var toks = lexer.ConsumeTokens(3);

            Assert.That(toks.Length, Is.EqualTo(3));
            Assert.That(toks[0].TokenType, Is.EqualTo(expectedTokenType));
            Assert.That(toks[1].TokenType, Is.EqualTo(expectedTokenType));
            Assert.That(toks[2].TokenType, Is.EqualTo(TokenType.EndOfFile));
        }

        [Test]
        public static void Lexer_Test_Minus()
        {
            var text = "x--";
            var lexer = new Lexer(text);
            var toks = lexer.ConsumeTokens(10);
            var counter = 0;
            Assert.That(toks[counter++].TokenType, Is.EqualTo(TokenType.Identifier));
            Assert.That(toks[counter++].TokenType, Is.EqualTo(TokenType.SubtractSubtract));

            counter = 0;
            text = "--x";
            lexer = new Lexer(text);
            toks = lexer.ConsumeTokens(10);
            Assert.That(toks[counter++].TokenType, Is.EqualTo(TokenType.SubtractSubtract));
            Assert.That(toks[counter++].TokenType, Is.EqualTo(TokenType.Identifier));

            counter = 0;
            text = "-1";
            lexer = new Lexer(text);
            toks = lexer.ConsumeTokens(10);
            Assert.That(toks[counter].TokenType, Is.EqualTo(TokenType.Subtract));
            counter++;
            Assert.That(toks[counter].TokenType, Is.EqualTo(TokenType.Value));
            Assert.That(toks[counter].TypeIndicator, Is.EqualTo(TypeIndicator.Integer));

            counter = 0;
            text = "x-- - 1";
            lexer = new Lexer(text);
            toks = lexer.ConsumeTokens(10);

            Assert.That(toks[counter++].TokenType, Is.EqualTo(TokenType.Identifier));
            Assert.That(toks[counter++].TokenType, Is.EqualTo(TokenType.SubtractSubtract));
            Assert.That(toks[counter++].TokenType, Is.EqualTo(TokenType.Subtract));
            Assert.That(toks[counter].TokenType, Is.EqualTo(TokenType.Value));
            Assert.That(toks[counter].TypeIndicator, Is.EqualTo(TypeIndicator.Integer));

            counter = 0;
            text = "x-- - -1";
            lexer = new Lexer(text);
            toks = lexer.ConsumeTokens(10);

            Assert.That(toks[counter++].TokenType, Is.EqualTo(TokenType.Identifier));
            Assert.That(toks[counter++].TokenType, Is.EqualTo(TokenType.SubtractSubtract));
            Assert.That(toks[counter++].TokenType, Is.EqualTo(TokenType.Subtract));
            Assert.That(toks[counter++].TokenType, Is.EqualTo(TokenType.Subtract));
            Assert.That(toks[counter].TokenType, Is.EqualTo(TokenType.Value));
            Assert.That(toks[counter].TypeIndicator, Is.EqualTo(TypeIndicator.Integer));

            counter = 0;
            text = "x-- - --x";
            lexer = new Lexer(text);
            toks = lexer.ConsumeTokens(10);

            Assert.That(toks[counter++].TokenType, Is.EqualTo(TokenType.Identifier));
            Assert.That(toks[counter++].TokenType, Is.EqualTo(TokenType.SubtractSubtract));
            Assert.That(toks[counter++].TokenType, Is.EqualTo(TokenType.Subtract));
            Assert.That(toks[counter++].TokenType, Is.EqualTo(TokenType.SubtractSubtract));
            Assert.That(toks[counter++].TokenType, Is.EqualTo(TokenType.Identifier));
        }

        [Test]
        public static void Lexer_Test_Plus()
        {
            var text = "x++";
            var lexer = new Lexer(text);
            var toks = lexer.ConsumeTokens(10);
            var counter = 0;
            Assert.That(toks[counter++].TokenType, Is.EqualTo(TokenType.Identifier));
            Assert.That(toks[counter++].TokenType, Is.EqualTo(TokenType.AddAdd));

            counter = 0;
            text = "++x";
            lexer = new Lexer(text);
            toks = lexer.ConsumeTokens(10);
            Assert.That(toks[counter++].TokenType, Is.EqualTo(TokenType.AddAdd));
            Assert.That(toks[counter++].TokenType, Is.EqualTo(TokenType.Identifier));

            counter = 0;
            text = "+1";
            lexer = new Lexer(text);
            toks = lexer.ConsumeTokens(10);
            Assert.That(toks[counter].TokenType, Is.EqualTo(TokenType.Add));
            counter++;
            Assert.That(toks[counter].TokenType, Is.EqualTo(TokenType.Value));
            Assert.That(toks[counter].TypeIndicator, Is.EqualTo(TypeIndicator.Integer));

            counter = 0;
            text = "x++ + 1";
            lexer = new Lexer(text);
            toks = lexer.ConsumeTokens(10);

            Assert.That(toks[counter++].TokenType, Is.EqualTo(TokenType.Identifier));
            Assert.That(toks[counter++].TokenType, Is.EqualTo(TokenType.AddAdd));
            Assert.That(toks[counter++].TokenType, Is.EqualTo(TokenType.Add));
            Assert.That(toks[counter].TokenType, Is.EqualTo(TokenType.Value));
            Assert.That(toks[counter].TypeIndicator, Is.EqualTo(TypeIndicator.Integer));
            
            counter = 0;
            text = "x++ + +1";
            lexer = new Lexer(text);
            toks = lexer.ConsumeTokens(10);

            Assert.That(toks[counter++].TokenType, Is.EqualTo(TokenType.Identifier));
            Assert.That(toks[counter++].TokenType, Is.EqualTo(TokenType.AddAdd));
            Assert.That(toks[counter++].TokenType, Is.EqualTo(TokenType.Add));
            Assert.That(toks[counter++].TokenType, Is.EqualTo(TokenType.Add));
            Assert.That(toks[counter].TokenType, Is.EqualTo(TokenType.Value));
            Assert.That(toks[counter].TypeIndicator, Is.EqualTo(TypeIndicator.Integer));

            counter = 0;
            text = "x++ + ++x";
            lexer = new Lexer(text);
            toks = lexer.ConsumeTokens(10);

            Assert.That(toks[counter++].TokenType, Is.EqualTo(TokenType.Identifier));
            Assert.That(toks[counter++].TokenType, Is.EqualTo(TokenType.AddAdd));
            Assert.That(toks[counter++].TokenType, Is.EqualTo(TokenType.Add));
            Assert.That(toks[counter++].TokenType, Is.EqualTo(TokenType.AddAdd));
            Assert.That(toks[counter++].TokenType, Is.EqualTo(TokenType.Identifier));
        }

        [TestCase("\"t\"c", ExpectedResult = "t")]
        [TestCase("\"t t\"c", ExpectedResult = "t t")]
        [TestCase("\"t \\n t\"c", ExpectedResult = "t \\n t")]//todo: is this even right? Answer, yes, length check can be done in parser... We shouldn't care about that here... I think
        [TestCase("\"\"c", ExpectedResult = "")]
        public static object? Lexer_Test_Character(string text)
        {
            var lexer = new Lexer(text);
            var toks = lexer.ConsumeTokens(text.Length + 1);
            Assert.IsTrue(toks.First().TokenType is TokenType.Value);
            Assert.IsTrue(toks.First().TypeIndicator is TypeIndicator.Character);
            return toks.First().Value;
        }


        [TestCase("\"t\"", ExpectedResult = "t")]
        [TestCase("\"t t\"", ExpectedResult = "t t")]
        [TestCase("\"\"", ExpectedResult = "")]
        public static object? Lexer_Test_String(string text)
        {
            var lexer = new Lexer(text);
            var toks = lexer.ConsumeTokens(2);
            Assert.That(toks.Last().TokenType, Is.EqualTo(TokenType.EndOfFile));
            return toks.First().Value;
        }

        [Test]
        public static void Lexer_Test_Code()
        {
            var code = "var variableName = 20 + 5;";
            var lexer = new Lexer(code);

            var toks = lexer.ConsumeTokens(7);
            Assert.That(toks[0].TokenType, Is.EqualTo(TokenType.Type));
            Assert.That(toks[1].TokenType, Is.EqualTo(TokenType.Identifier));
            Assert.That(toks[2].TokenType, Is.EqualTo(TokenType.Assignment));
            Assert.That(toks[3].TokenType, Is.EqualTo(TokenType.Value));
            Assert.That(toks[3].Value, Is.EqualTo(20));
            Assert.That(toks[4].TokenType, Is.EqualTo(TokenType.Add));
            Assert.That(toks[5].TokenType, Is.EqualTo(TokenType.Value));
            Assert.That(toks[5].Value, Is.EqualTo(5));
            Assert.That(toks[6].TokenType, Is.EqualTo(TokenType.EndOfStatement));
            Assert.That(lexer.PeekToken().TokenType, Is.EqualTo(TokenType.EndOfFile));
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
            Assert.That(toks.Count(x => x.TokenType is TokenType.Comment), Is.EqualTo(commentTokCount));
            Assert.That(toks.Count(x => x.TokenType is not TokenType.Comment), Is.EqualTo(toks.Length - commentTokCount));
        }

        [TestCase("// this text is to be ignored \n var x = 10;", 1)]
        [TestCase("// this text is to be ignored \n // so is this text \n var x = 10;", 2)]
        [TestCase("// this text is to be ignored \n // so is this text \r\n var x = 10;", 2)]
        public static void Lexer_Test_Comments_And_Code(string text, int commentTokCount)
        {
            var lexer = new Lexer(text);
            var toks = lexer.ConsumeTokens(commentTokCount);
            Assert.That(toks.Count(x => x.TokenType is TokenType.Comment), Is.EqualTo(commentTokCount));
            Assert.That(lexer.ConsumeToken().TokenType, Is.EqualTo(TokenType.Type));
            Assert.That(lexer.ConsumeToken().TokenType, Is.EqualTo(TokenType.Identifier));
            Assert.That(lexer.ConsumeToken().TokenType, Is.EqualTo(TokenType.Assignment));
            Assert.That(lexer.ConsumeToken().TokenType, Is.EqualTo(TokenType.Value));
            Assert.That(lexer.ConsumeToken().TokenType, Is.EqualTo(TokenType.EndOfStatement));
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
            Assert.That(toks.Count(x => x.TokenType is TokenType.Summary), Is.EqualTo(commentTokCount));
            Assert.That(toks.Count(x => x.TokenType is not TokenType.Summary), Is.EqualTo(toks.Length - commentTokCount));
        }

        [TestCase("/// this text is to be ignored \n var x = 10;", 1)]
        [TestCase("/// this text is to be ignored \n /// so is this text \n var x = 10;", 2)]
        [TestCase("/// this text is to be ignored \n /// so is this text \r\n var x = 10;", 2)]
        public static void Lexer_Test_Summaries_And_Code(string text, int commentTokCount)
        {
            var lexer = new Lexer(text);
            var toks = lexer.ConsumeTokens(commentTokCount);
            Assert.That(toks.Count(x => x.TokenType is TokenType.Summary), Is.EqualTo(commentTokCount));
            Assert.That(lexer.ConsumeToken().TokenType, Is.EqualTo(TokenType.Type));
            Assert.That(lexer.ConsumeToken().TokenType, Is.EqualTo(TokenType.Identifier));
            Assert.That(lexer.ConsumeToken().TokenType, Is.EqualTo(TokenType.Assignment));
            Assert.That(lexer.ConsumeToken().TokenType, Is.EqualTo(TokenType.Value));
            Assert.That(lexer.ConsumeToken().TokenType, Is.EqualTo(TokenType.EndOfStatement));
        }

        [TestCase("x + 7;")]
        [TestCase("x+7;")]
        public static void Lexer_Test_Use_Variable(string code)
        {
            var lexer = new Lexer(code);
            var toks = lexer.ConsumeTokens(5);
            Assert.That(toks[0].TokenType, Is.EqualTo(TokenType.Identifier));
            Assert.That(toks[1].TokenType, Is.EqualTo(TokenType.Add));
            Assert.That(toks[2].TokenType, Is.EqualTo(TokenType.Value));
            Assert.That(toks[3].TokenType, Is.EqualTo(TokenType.EndOfStatement));
            Assert.That(toks[4].TokenType, Is.EqualTo(TokenType.EndOfFile));
        }

        [TestCase("x++;")]
        [TestCase("x ++;")]
        [TestCase("x \r ++;")]
        [TestCase("x \n ++;")]
        [TestCase("x \r\n ++;")]
        public static void Lexer_Test_Use_Variable_AddAdd(string code)
        {
            var lexer = new Lexer(code);
            var toks = lexer.ConsumeTokens(4);

            Assert.That(toks[0].TokenType, Is.EqualTo(TokenType.Identifier));
            Assert.That(toks[1].TokenType, Is.EqualTo(TokenType.AddAdd));
            Assert.That(toks[2].TokenType, Is.EqualTo(TokenType.EndOfStatement));
            Assert.That(toks[3].TokenType, Is.EqualTo(TokenType.EndOfFile));
        }

        [TestCase("x--;")]
        [TestCase("x --;")]
        [TestCase("x \r --;")]
        [TestCase("x \n --;")]
        [TestCase("x \r\n --;")]
        public static void Lexer_Test_Use_Variable_SubtractSubtract(string code)
        {
            var lexer = new Lexer(code);
            var toks = lexer.ConsumeTokens(4);

            Assert.That(toks[0].TokenType, Is.EqualTo(TokenType.Identifier));
            Assert.That(toks[1].TokenType, Is.EqualTo(TokenType.SubtractSubtract));
            Assert.That(toks[2].TokenType, Is.EqualTo(TokenType.EndOfStatement));
            Assert.That(toks[3].TokenType, Is.EqualTo(TokenType.EndOfFile));
        }

        [TestCase("int", ExpectedResult = TypeIndicator.Integer)]
        [TestCase("double", ExpectedResult = TypeIndicator.Double)]
        [TestCase("float", ExpectedResult = TypeIndicator.Float)]
        [TestCase("string ", ExpectedResult = TypeIndicator.String)]
        [TestCase("char", ExpectedResult = TypeIndicator.Character)]
        [TestCase("auto", ExpectedResult = TypeIndicator.Inferred)]
        [TestCase("var", ExpectedResult = TypeIndicator.Inferred)]
        public static TypeIndicator Lexer_Test_Base_Types(string code)
        {
            var lexer = new Lexer(code);
            var tok = lexer.ConsumeToken();
            Assert.That(tok.TokenType, Is.EqualTo(TokenType.Type));
            return tok.TypeIndicator;
        }

        public static void Lexer_Test_Assign_And_Use_Variable_No_Whitespace_EndOfStatement()
        {
            var lexer = new Lexer("var x=x+7;");
            var toks = lexer.ConsumeTokens(8);

            Assert.That(toks[0].TokenType, Is.EqualTo(TokenType.Type));
            Assert.That(toks[1].TokenType, Is.EqualTo(TokenType.Identifier));
            Assert.That(toks[2].TokenType, Is.EqualTo(TokenType.Assignment));
            Assert.That(toks[3].TokenType, Is.EqualTo(TokenType.Identifier));
            Assert.That(toks[4].TokenType, Is.EqualTo(TokenType.Add));
            Assert.That(toks[5].TokenType, Is.EqualTo(TokenType.Value));
            Assert.That(toks[6].TokenType, Is.EqualTo(TokenType.EndOfStatement));
            Assert.That(toks[7].TokenType, Is.EqualTo(TokenType.EndOfFile));
        }

        [Test]
        public static void Lexer_Test_Assign_And_Use_Variable_No_Whitespace_EndOfFile()
        {
            var lexer = new Lexer("var x=x+7");
            var toks = lexer.ConsumeTokens(8);

            Assert.That(toks[0].TokenType, Is.EqualTo(TokenType.Type));
            Assert.That(toks[1].TokenType, Is.EqualTo(TokenType.Identifier));
            Assert.That(toks[2].TokenType, Is.EqualTo(TokenType.Assignment));
            Assert.That(toks[3].TokenType, Is.EqualTo(TokenType.Identifier));
            Assert.That(toks[4].TokenType, Is.EqualTo(TokenType.Add));
            Assert.That(toks[5].TokenType, Is.EqualTo(TokenType.Value));
            Assert.That(toks[6].TokenType, Is.EqualTo(TokenType.EndOfFile));
            Assert.That(toks[7].TokenType, Is.EqualTo(TokenType.EndOfFile));
        }

        [TestCase("var x=x+(7-(8+2));")]
        [TestCase("var x = x + ( 7 - ( 8 + 2 ) ) ;")]

        public static void Lexer_Test_Assign_And_Use_Variable_No_Whitespace_PrecedenceOperators(string code)
        {
            var lexer = new Lexer(code);
            var toks = lexer.ConsumeTokens(16);

            Assert.That(toks[0].TokenType, Is.EqualTo(TokenType.Type));
            Assert.That(toks[1].TokenType, Is.EqualTo(TokenType.Identifier));
            Assert.That(toks[2].TokenType, Is.EqualTo(TokenType.Assignment));
            Assert.That(toks[3].TokenType, Is.EqualTo(TokenType.Identifier));
            Assert.That(toks[4].TokenType, Is.EqualTo(TokenType.Add));
            Assert.That(toks[5].TokenType, Is.EqualTo(TokenType.ParanthesesOpen));
            Assert.That(toks[6].TokenType, Is.EqualTo(TokenType.Value));
            Assert.That(toks[7].TokenType, Is.EqualTo(TokenType.Subtract));
            Assert.That(toks[8].TokenType, Is.EqualTo(TokenType.ParanthesesOpen));
            Assert.That(toks[9].TokenType, Is.EqualTo(TokenType.Value));
            Assert.That(toks[10].TokenType, Is.EqualTo(TokenType.Add));
            Assert.That(toks[11].TokenType, Is.EqualTo(TokenType.Value));
            Assert.That(toks[12].TokenType, Is.EqualTo(TokenType.ParanthesesClose));
            Assert.That(toks[13].TokenType, Is.EqualTo(TokenType.ParanthesesClose));
            Assert.That(toks[14].TokenType, Is.EqualTo(TokenType.EndOfStatement));
            Assert.That(toks[15].TokenType, Is.EqualTo(TokenType.EndOfFile));
        }

        [TestCase("var x=x+(7-(8-2));")]
        [TestCase("var x = x + ( 7 - ( 8 - 2 ) ) ;")]
        public static void Lexer_Test_Assign_And_Use_Variable_No_Whitespace_PrecedenceOperators2(string code)
        {
            var lexer = new Lexer(code);
            var toks = lexer.ConsumeTokens(16);

            Assert.That(toks[0].TokenType, Is.EqualTo(TokenType.Type));
            Assert.That(toks[1].TokenType, Is.EqualTo(TokenType.Identifier));
            Assert.That(toks[2].TokenType, Is.EqualTo(TokenType.Assignment));
            Assert.That(toks[3].TokenType, Is.EqualTo(TokenType.Identifier));
            Assert.That(toks[4].TokenType, Is.EqualTo(TokenType.Add));
            Assert.That(toks[5].TokenType, Is.EqualTo(TokenType.ParanthesesOpen));
            Assert.That(toks[6].TokenType, Is.EqualTo(TokenType.Value));
            Assert.That(toks[7].TokenType, Is.EqualTo(TokenType.Subtract));
            Assert.That(toks[8].TokenType, Is.EqualTo(TokenType.ParanthesesOpen));
            Assert.That(toks[9].TokenType, Is.EqualTo(TokenType.Value));
            Assert.That(toks[10].TokenType, Is.EqualTo(TokenType.Subtract));
            Assert.That(toks[11].TokenType, Is.EqualTo(TokenType.Value));
            Assert.That(toks[12].TokenType, Is.EqualTo(TokenType.ParanthesesClose));
            Assert.That(toks[13].TokenType, Is.EqualTo(TokenType.ParanthesesClose));
            Assert.That(toks[14].TokenType, Is.EqualTo(TokenType.EndOfStatement));
            Assert.That(toks[15].TokenType, Is.EqualTo(TokenType.EndOfFile));
        }

        [Test]
        public static void Lexer_Test_Declare_Void_Function_No_Params()
        {
            var lexer = new Lexer("func SomeFunc() -> void {}");

            var toks = lexer.ConsumeTokens(8);
            Assert.That(toks[0].TokenType, Is.EqualTo(TokenType.FunctionDefinition));
            Assert.That(toks[1].TokenType, Is.EqualTo(TokenType.Identifier));
            Assert.That(toks[2].TokenType, Is.EqualTo(TokenType.ParanthesesOpen));
            Assert.That(toks[3].TokenType, Is.EqualTo(TokenType.ParanthesesClose));
            Assert.That(toks[4].TokenType, Is.EqualTo(TokenType.ReturnTypeIndicator));
            Assert.That(toks[5].TokenType, Is.EqualTo(TokenType.Type));
            Assert.That(toks[5].TypeIndicator, Is.EqualTo(TypeIndicator.Void));
            Assert.That(toks[6].TokenType, Is.EqualTo(TokenType.AccoladesOpen));
            Assert.That(toks[7].TokenType, Is.EqualTo(TokenType.AccoladesClose));
        }

        [Test]
        public static void Lexer_Test_Declare_Void_Function_With_Params()
        {
            var lexer = new Lexer("func SomeFunc(bool x, double y, int z, string ranOutOfAlphabet) -> void {}");

            var toks = lexer.ConsumeTokens(50);
            var counter = 0;
            Assert.That(toks[counter++].TokenType, Is.EqualTo(TokenType.FunctionDefinition));
            Assert.That(toks[counter++].TokenType, Is.EqualTo(TokenType.Identifier));
            Assert.That(toks[counter++].TokenType, Is.EqualTo(TokenType.ParanthesesOpen));
            Assert.That(toks[counter].TypeIndicator, Is.EqualTo(TypeIndicator.Boolean));
            Assert.That(toks[counter++].TokenType, Is.EqualTo(TokenType.Type));
            Assert.That(toks[counter++].TokenType, Is.EqualTo(TokenType.Identifier));
            Assert.That(toks[counter++].TokenType, Is.EqualTo(TokenType.ArgumentSeparator));
            Assert.That(toks[counter].TypeIndicator, Is.EqualTo(TypeIndicator.Double));
            Assert.That(toks[counter++].TokenType, Is.EqualTo(TokenType.Type));
            Assert.That(toks[counter++].TokenType, Is.EqualTo(TokenType.Identifier));
            Assert.That(toks[counter++].TokenType, Is.EqualTo(TokenType.ArgumentSeparator));
            Assert.That(toks[counter].TypeIndicator, Is.EqualTo(TypeIndicator.Integer));
            Assert.That(toks[counter++].TokenType, Is.EqualTo(TokenType.Type));
            Assert.That(toks[counter++].TokenType, Is.EqualTo(TokenType.Identifier));
            Assert.That(toks[counter++].TokenType, Is.EqualTo(TokenType.ArgumentSeparator));
            Assert.That(toks[counter].TypeIndicator, Is.EqualTo(TypeIndicator.String));

            Assert.That(toks[counter++].TokenType, Is.EqualTo(TokenType.Type));
            Assert.That(toks[counter++].TokenType, Is.EqualTo(TokenType.Identifier));
            Assert.That(toks[counter++].TokenType, Is.EqualTo(TokenType.ParanthesesClose));
            Assert.That(toks[counter++].TokenType, Is.EqualTo(TokenType.ReturnTypeIndicator));
            Assert.That(toks[counter].TypeIndicator, Is.EqualTo(TypeIndicator.Void));
            Assert.That(toks[counter++].TokenType, Is.EqualTo(TokenType.Type));
            Assert.That(toks[counter++].TokenType, Is.EqualTo(TokenType.AccoladesOpen));
            Assert.That(toks[counter++].TokenType, Is.EqualTo(TokenType.AccoladesClose));
        }

        [Test]
        public static void Lexer_Test_Declare_Int_Main_Function_With_Variable_Return()
        {
            var lexer = new Lexer("func Main() -> int { int x=5; return x;}");
            var toks = lexer.ConsumeTokens(50);
            var counter = 0;
            Assert.That(toks[counter++].TokenType, Is.EqualTo(TokenType.FunctionDefinition));
            Assert.That(toks[counter++].TokenType, Is.EqualTo(TokenType.Identifier));
            Assert.That(toks[counter++].TokenType, Is.EqualTo(TokenType.ParanthesesOpen));
            Assert.That(toks[counter++].TokenType, Is.EqualTo(TokenType.ParanthesesClose));
            Assert.That(toks[counter++].TokenType, Is.EqualTo(TokenType.ReturnTypeIndicator));
            Assert.That(toks[counter].TypeIndicator, Is.EqualTo(TypeIndicator.Integer));
            Assert.That(toks[counter++].TokenType, Is.EqualTo(TokenType.Type));
            Assert.That(toks[counter++].TokenType, Is.EqualTo(TokenType.AccoladesOpen));
            Assert.That(toks[counter].TypeIndicator, Is.EqualTo(TypeIndicator.Integer));
            Assert.That(toks[counter++].TokenType, Is.EqualTo(TokenType.Type));
            Assert.That(toks[counter++].TokenType, Is.EqualTo(TokenType.Identifier));
            Assert.That(toks[counter++].TokenType, Is.EqualTo(TokenType.Assignment));
            Assert.That(toks[counter++].TokenType, Is.EqualTo(TokenType.Value));
            Assert.That(toks[counter++].TokenType, Is.EqualTo(TokenType.EndOfStatement));
            Assert.That(toks[counter++].TokenType, Is.EqualTo(TokenType.Return));
            Assert.That(toks[counter++].TokenType, Is.EqualTo(TokenType.Identifier));
            Assert.That(toks[counter++].TokenType, Is.EqualTo(TokenType.EndOfStatement));


            Assert.That(toks[counter++].TokenType, Is.EqualTo(TokenType.AccoladesClose));

        }

        [Test]
        public static void Lexer_Test_Declare_Bool_Function_With_Params()
        {
            var lexer = new Lexer("func SomeFunc(bool x, double y, int z, string ranOutOfAlphabet) -> bool {}");

            var toks = lexer.ConsumeTokens(50);
            var counter = 0;
            Assert.That(toks[counter++].TokenType, Is.EqualTo(TokenType.FunctionDefinition));
            Assert.That(toks[counter++].TokenType, Is.EqualTo(TokenType.Identifier));
            Assert.That(toks[counter++].TokenType, Is.EqualTo(TokenType.ParanthesesOpen));
            Assert.That(toks[counter].TypeIndicator, Is.EqualTo(TypeIndicator.Boolean));
            Assert.That(toks[counter++].TokenType, Is.EqualTo(TokenType.Type));
            Assert.That(toks[counter++].TokenType, Is.EqualTo(TokenType.Identifier));
            Assert.That(toks[counter++].TokenType, Is.EqualTo(TokenType.ArgumentSeparator));
            Assert.That(toks[counter].TypeIndicator, Is.EqualTo(TypeIndicator.Double));
            Assert.That(toks[counter++].TokenType, Is.EqualTo(TokenType.Type));
            Assert.That(toks[counter++].TokenType, Is.EqualTo(TokenType.Identifier));
            Assert.That(toks[counter++].TokenType, Is.EqualTo(TokenType.ArgumentSeparator));
            Assert.That(toks[counter].TypeIndicator, Is.EqualTo(TypeIndicator.Integer));
            Assert.That(toks[counter++].TokenType, Is.EqualTo(TokenType.Type));
            Assert.That(toks[counter++].TokenType, Is.EqualTo(TokenType.Identifier));
            Assert.That(toks[counter++].TokenType, Is.EqualTo(TokenType.ArgumentSeparator));
            Assert.That(toks[counter].TypeIndicator, Is.EqualTo(TypeIndicator.String));
            Assert.That(toks[counter++].TokenType, Is.EqualTo(TokenType.Type));
            Assert.That(toks[counter++].TokenType, Is.EqualTo(TokenType.Identifier));
            Assert.That(toks[counter++].TokenType, Is.EqualTo(TokenType.ParanthesesClose));
            Assert.That(toks[counter++].TokenType, Is.EqualTo(TokenType.ReturnTypeIndicator));
            Assert.That(toks[counter].TypeIndicator, Is.EqualTo(TypeIndicator.Boolean));
            Assert.That(toks[counter++].TokenType, Is.EqualTo(TokenType.Type));
            Assert.That(toks[counter++].TokenType, Is.EqualTo(TokenType.AccoladesOpen));
            Assert.That(toks[counter++].TokenType, Is.EqualTo(TokenType.AccoladesClose));
        }

        [Test]
        public static void Lexer_Test_Declare_CustomDefinedType_Function_With_Params()
        {
            var lexer = new Lexer("func SomeFunc(bool x, double y, int z, string ranOutOfAlphabet) -> ImagineThisIsACustomDefinedType {}");

            var toks = lexer.ConsumeTokens(50);
            var counter = 0;
            Assert.That(toks[counter++].TokenType, Is.EqualTo(TokenType.FunctionDefinition));
            Assert.That(toks[counter++].TokenType, Is.EqualTo(TokenType.Identifier));
            Assert.That(toks[counter++].TokenType, Is.EqualTo(TokenType.ParanthesesOpen));
            Assert.That(toks[counter].TypeIndicator, Is.EqualTo(TypeIndicator.Boolean));
            Assert.That(toks[counter++].TokenType, Is.EqualTo(TokenType.Type));
            Assert.That(toks[counter++].TokenType, Is.EqualTo(TokenType.Identifier));
            Assert.That(toks[counter++].TokenType, Is.EqualTo(TokenType.ArgumentSeparator));
            Assert.That(toks[counter].TypeIndicator, Is.EqualTo(TypeIndicator.Double));
            Assert.That(toks[counter++].TokenType, Is.EqualTo(TokenType.Type));
            Assert.That(toks[counter++].TokenType, Is.EqualTo(TokenType.Identifier));
            Assert.That(toks[counter++].TokenType, Is.EqualTo(TokenType.ArgumentSeparator));
            Assert.That(toks[counter].TypeIndicator, Is.EqualTo(TypeIndicator.Integer));
            Assert.That(toks[counter++].TokenType, Is.EqualTo(TokenType.Type));
            Assert.That(toks[counter++].TokenType, Is.EqualTo(TokenType.Identifier));
            Assert.That(toks[counter++].TokenType, Is.EqualTo(TokenType.ArgumentSeparator));
            Assert.That(toks[counter].TypeIndicator, Is.EqualTo(TypeIndicator.String));
            Assert.That(toks[counter++].TokenType, Is.EqualTo(TokenType.Type));
            Assert.That(toks[counter++].TokenType, Is.EqualTo(TokenType.Identifier));
            Assert.That(toks[counter++].TokenType, Is.EqualTo(TokenType.ParanthesesClose));
            Assert.That(toks[counter++].TokenType, Is.EqualTo(TokenType.ReturnTypeIndicator));
            Assert.That(toks[counter++].TokenType, Is.EqualTo(TokenType.Identifier));
            Assert.That(toks[counter++].TokenType, Is.EqualTo(TokenType.AccoladesOpen));
            Assert.That(toks[counter++].TokenType, Is.EqualTo(TokenType.AccoladesClose));
        }

        [TestCase("double", ExpectedResult = TypeIndicator.Double)]
        [TestCase("int", ExpectedResult = TypeIndicator.Integer)]
        [TestCase("float", ExpectedResult = TypeIndicator.Float)]
        [TestCase("string", ExpectedResult = TypeIndicator.String)]
        [TestCase("char", ExpectedResult = TypeIndicator.Character)]
        [TestCase("bool", ExpectedResult = TypeIndicator.Boolean)]
        public static TypeIndicator Lexer_Test_Native_Types(string code)
        {
            var lexer = new Lexer(code);
            var tok = lexer.ConsumeToken();
            Assert.That(tok.TokenType, Is.EqualTo(TokenType.Type));
            return tok.TypeIndicator;
        }

        [TestCase("params double", ExpectedResult = TypeIndicator.Double)]
        [TestCase("params int", ExpectedResult = TypeIndicator.Integer)]
        [TestCase("params float", ExpectedResult = TypeIndicator.Float)]
        [TestCase("params string", ExpectedResult = TypeIndicator.String)]
        [TestCase("params char", ExpectedResult = TypeIndicator.Character)]
        [TestCase("params bool", ExpectedResult = TypeIndicator.Boolean)]
        public static TypeIndicator Lexer_Test_Native_Type_Params(string code)
        {
            var lexer = new Lexer(code);
            var toks = lexer.ConsumeTokens(2);
            Assert.That(toks[0].TokenType, Is.EqualTo(TokenType.Params));
            Assert.That(toks[1].TokenType, Is.EqualTo(TokenType.Type));
            return toks[1].TypeIndicator;
        }

        [TestCase("params double[]", ExpectedResult = TypeIndicator.Double)]
        [TestCase("params int[]", ExpectedResult = TypeIndicator.Integer)]
        [TestCase("params float[]", ExpectedResult = TypeIndicator.Float)]
        [TestCase("params string[]", ExpectedResult = TypeIndicator.String)]
        [TestCase("params char[]", ExpectedResult = TypeIndicator.Character)]
        [TestCase("params bool[]", ExpectedResult = TypeIndicator.Boolean)]
        public static TypeIndicator Lexer_Test_Native_Type_Params_Array(string code)
        {
            var lexer = new Lexer(code);
            var toks = lexer.ConsumeTokens(4);
            Assert.That(toks[0].TokenType, Is.EqualTo(TokenType.Params));
            Assert.That(toks[1].TokenType, Is.EqualTo(TokenType.Type));
            Assert.That(toks[2].TokenType, Is.EqualTo(TokenType.Array));
            Assert.That(toks[3].TokenType, Is.EqualTo(TokenType.EndOfFile));
            return toks[1].TypeIndicator;
        }


        [Test]
        public static void Lexer_Test_Declare_CustomDefinedType_Function_With_Params_ParamsArrayOfStrings()
        {
            var lexer = new Lexer("func SomeFunc(bool x, double y, int z, params string[] ranOutOfAlphabet) -> ImagineThisIsACustomDefinedType {}");

            var toks = lexer.ConsumeTokens(50);
            var counter = 0;
            Assert.That(toks[counter++].TokenType, Is.EqualTo(TokenType.FunctionDefinition));
            Assert.That(toks[counter++].TokenType, Is.EqualTo(TokenType.Identifier));
            Assert.That(toks[counter++].TokenType, Is.EqualTo(TokenType.ParanthesesOpen));
            Assert.That(toks[counter++].TokenType, Is.EqualTo(TokenType.Type));
            Assert.That(toks[counter++].TokenType, Is.EqualTo(TokenType.Identifier));
            Assert.That(toks[counter++].TokenType, Is.EqualTo(TokenType.ArgumentSeparator));
            Assert.That(toks[counter++].TokenType, Is.EqualTo(TokenType.Type));
            Assert.That(toks[counter++].TokenType, Is.EqualTo(TokenType.Identifier));
            Assert.That(toks[counter++].TokenType, Is.EqualTo(TokenType.ArgumentSeparator));
            Assert.That(toks[counter++].TokenType, Is.EqualTo(TokenType.Type));
            Assert.That(toks[counter++].TokenType, Is.EqualTo(TokenType.Identifier));
            Assert.That(toks[counter++].TokenType, Is.EqualTo(TokenType.ArgumentSeparator));
            Assert.That(toks[counter++].TokenType, Is.EqualTo(TokenType.Params));
            Assert.That(toks[counter++].TokenType, Is.EqualTo(TokenType.Type));
            Assert.That(toks[counter++].TokenType, Is.EqualTo(TokenType.Array));
            Assert.That(toks[counter++].TokenType, Is.EqualTo(TokenType.Identifier));
            Assert.That(toks[counter++].TokenType, Is.EqualTo(TokenType.ParanthesesClose));
            Assert.That(toks[counter++].TokenType, Is.EqualTo(TokenType.ReturnTypeIndicator));
            Assert.That(toks[counter].TypeIndicator, Is.EqualTo(TypeIndicator.None));
            Assert.That(toks[counter++].TokenType, Is.EqualTo(TokenType.Identifier));
            Assert.That(toks[counter++].TokenType, Is.EqualTo(TokenType.AccoladesOpen));
            Assert.That(toks[counter++].TokenType, Is.EqualTo(TokenType.AccoladesClose));
        }



        [TestCase("if(true){}")]
        [TestCase("if ( true ) { } ")]
        [TestCase("\nif\n(\ntrue\n)\n{\n}\n")]
        public static void Lexer_Test_If_Statement(string code)
        {
            var lexer = new Lexer(code);

            var toks = lexer.ConsumeTokens(7);
            Assert.That(toks[0].TokenType, Is.EqualTo(TokenType.If));
            Assert.That(toks[1].TokenType, Is.EqualTo(TokenType.ParanthesesOpen));
            Assert.That(toks[2].TokenType, Is.EqualTo(TokenType.Value));
            Assert.That(toks[3].TokenType, Is.EqualTo(TokenType.ParanthesesClose));
            Assert.That(toks[4].TokenType, Is.EqualTo(TokenType.AccoladesOpen));
            Assert.That(toks[5].TokenType, Is.EqualTo(TokenType.AccoladesClose));
            Assert.That(toks[6].TokenType, Is.EqualTo(TokenType.EndOfFile));
        }

        [TestCase("if(true){}else{}")]
        [TestCase("if ( true ) { } else { } ")]
        [TestCase("\nif\n(\ntrue\n)\n{\n}\nelse\n{\n}\n")]
        public static void Lexer_Test_If_Else_Statement(string code)
        {
            var lexer = new Lexer(code);

            var toks = lexer.ConsumeTokens(10);
            Assert.That(toks[0].TokenType, Is.EqualTo(TokenType.If));
            Assert.That(toks[1].TokenType, Is.EqualTo(TokenType.ParanthesesOpen));
            Assert.That(toks[2].TokenType, Is.EqualTo(TokenType.Value));
            Assert.That(toks[3].TokenType, Is.EqualTo(TokenType.ParanthesesClose));
            Assert.That(toks[4].TokenType, Is.EqualTo(TokenType.AccoladesOpen));
            Assert.That(toks[5].TokenType, Is.EqualTo(TokenType.AccoladesClose));
            Assert.That(toks[6].TokenType, Is.EqualTo(TokenType.Else));
            Assert.That(toks[7].TokenType, Is.EqualTo(TokenType.AccoladesOpen));
            Assert.That(toks[8].TokenType, Is.EqualTo(TokenType.AccoladesClose));
            Assert.That(toks[9].TokenType, Is.EqualTo(TokenType.EndOfFile));
        }

        [TestCase("if(true){}else if( false ){}")]
        [TestCase("if ( true ) { } else if ( false ) { } ")]
        [TestCase("\nif\n(\ntrue\n)\n{\n}\nelse\nif\n(\nfalse\n)\n{\n}\n")]
        public static void Lexer_Test_If_Else_If_Statement(string code)
        {
            var lexer = new Lexer(code);

            var toks = lexer.ConsumeTokens(14);
            Assert.That(toks[0].TokenType, Is.EqualTo(TokenType.If));
            Assert.That(toks[1].TokenType, Is.EqualTo(TokenType.ParanthesesOpen));
            Assert.That(toks[2].TokenType, Is.EqualTo(TokenType.Value));
            Assert.That(toks[3].TokenType, Is.EqualTo(TokenType.ParanthesesClose));
            Assert.That(toks[4].TokenType, Is.EqualTo(TokenType.AccoladesOpen));
            Assert.That(toks[5].TokenType, Is.EqualTo(TokenType.AccoladesClose));
            Assert.That(toks[6].TokenType, Is.EqualTo(TokenType.Else));
            Assert.That(toks[7].TokenType, Is.EqualTo(TokenType.If));
            Assert.That(toks[8].TokenType, Is.EqualTo(TokenType.ParanthesesOpen));
            Assert.That(toks[9].TokenType, Is.EqualTo(TokenType.Value));
            Assert.That(toks[10].TokenType, Is.EqualTo(TokenType.ParanthesesClose));
            Assert.That(toks[11].TokenType, Is.EqualTo(TokenType.AccoladesOpen));
            Assert.That(toks[12].TokenType, Is.EqualTo(TokenType.AccoladesClose));
            Assert.That(toks[13].TokenType, Is.EqualTo(TokenType.EndOfFile));
        }

        [TestCase("if(true){}else if(false){}else{}")]
        [TestCase("if ( true ) { } else if ( false ) { } else { } ")]
        [TestCase("\nif\n(\ntrue\n)\n{\n}\nelse\nif\n(\nfalse\n)\n{\n}\nelse\n{\n}\n")]
        public static void Lexer_Test_If_Else_If_Else_Statement(string code)
        {
            var lexer = new Lexer(code);

            var toks = lexer.ConsumeTokens(17);
            Assert.That(toks[0].TokenType, Is.EqualTo(TokenType.If));
            Assert.That(toks[1].TokenType, Is.EqualTo(TokenType.ParanthesesOpen));
            Assert.That(toks[2].TokenType, Is.EqualTo(TokenType.Value));
            Assert.That(toks[3].TokenType, Is.EqualTo(TokenType.ParanthesesClose));
            Assert.That(toks[4].TokenType, Is.EqualTo(TokenType.AccoladesOpen));
            Assert.That(toks[5].TokenType, Is.EqualTo(TokenType.AccoladesClose));
            Assert.That(toks[6].TokenType, Is.EqualTo(TokenType.Else));
            Assert.That(toks[7].TokenType, Is.EqualTo(TokenType.If));
            Assert.That(toks[8].TokenType, Is.EqualTo(TokenType.ParanthesesOpen));
            Assert.That(toks[9].TokenType, Is.EqualTo(TokenType.Value));
            Assert.That(toks[10].TokenType, Is.EqualTo(TokenType.ParanthesesClose));
            Assert.That(toks[11].TokenType, Is.EqualTo(TokenType.AccoladesOpen));
            Assert.That(toks[12].TokenType, Is.EqualTo(TokenType.AccoladesClose));
            Assert.That(toks[13].TokenType, Is.EqualTo(TokenType.Else));
            Assert.That(toks[14].TokenType, Is.EqualTo(TokenType.AccoladesOpen));
            Assert.That(toks[15].TokenType, Is.EqualTo(TokenType.AccoladesClose));
            Assert.That(toks[16].TokenType, Is.EqualTo(TokenType.EndOfFile));
        }

        [TestCase("while(true){}")]
        [TestCase("while ( true ) { } ")]
        [TestCase("\nwhile\n(\ntrue\n)\n{\n}\n")]
        public static void Lexer_Test_While_Statement(string code)
        {
            var lexer = new Lexer(code);

            var toks = lexer.ConsumeTokens(7);
            Assert.That(toks[0].TokenType, Is.EqualTo(TokenType.While));
            Assert.That(toks[1].TokenType, Is.EqualTo(TokenType.ParanthesesOpen));
            Assert.That(toks[2].TokenType, Is.EqualTo(TokenType.Value));
            Assert.That(toks[3].TokenType, Is.EqualTo(TokenType.ParanthesesClose));
            Assert.That(toks[4].TokenType, Is.EqualTo(TokenType.AccoladesOpen));
            Assert.That(toks[5].TokenType, Is.EqualTo(TokenType.AccoladesClose));
            Assert.That(toks[6].TokenType, Is.EqualTo(TokenType.EndOfFile));
        }

        [TestCase("do{}while(true)")]
        [TestCase(" do { } while ( true )")]
        [TestCase("\ndo\n{\n}\nwhile\n(\ntrue\n)\n")]
        [TestCase("\r\ndo\r\n{\r\n}\r\nwhile\r\n(\r\ntrue\r\n)\r\n")]
        public static void Lexer_Test_Do_While_Statement(string code)
        {
            var lexer = new Lexer(code);

            var toks = lexer.ConsumeTokens(8);
            Assert.That(toks[0].TokenType, Is.EqualTo(TokenType.Do));
            Assert.That(toks[1].TokenType, Is.EqualTo(TokenType.AccoladesOpen));
            Assert.That(toks[2].TokenType, Is.EqualTo(TokenType.AccoladesClose));
            Assert.That(toks[3].TokenType, Is.EqualTo(TokenType.While));
            Assert.That(toks[4].TokenType, Is.EqualTo(TokenType.ParanthesesOpen));
            Assert.That(toks[5].TokenType, Is.EqualTo(TokenType.Value));
            Assert.That(toks[6].TokenType, Is.EqualTo(TokenType.ParanthesesClose));
            Assert.That(toks[7].TokenType, Is.EqualTo(TokenType.EndOfFile));
        }

        [TestCase("for(var i =0; i<100; i++){}")]
        [TestCase("for(auto i =0; i<100; i++){}")]
        public static void Lexer_Test_For_Statement(string code)
        {
            var lexer = new Lexer(code);

            var toks = lexer.ConsumeTokens(17);
            var counter = 0;
            Assert.That(toks[counter++].TokenType, Is.EqualTo(TokenType.For));
            Assert.That(toks[counter++].TokenType, Is.EqualTo(TokenType.ParanthesesOpen));
            Assert.That(toks[counter++].TokenType, Is.EqualTo(TokenType.Type));
            Assert.That(toks[counter++].TokenType, Is.EqualTo(TokenType.Identifier));
            Assert.That(toks[counter++].TokenType, Is.EqualTo(TokenType.Assignment));
            Assert.That(toks[counter++].TokenType, Is.EqualTo(TokenType.Value));
            Assert.That(toks[counter++].TokenType, Is.EqualTo(TokenType.EndOfStatement));
            Assert.That(toks[counter++].TokenType, Is.EqualTo(TokenType.Identifier));
            Assert.That(toks[counter++].TokenType, Is.EqualTo(TokenType.LessThan));
            Assert.That(toks[counter].TypeIndicator, Is.EqualTo(TypeIndicator.Integer));
            Assert.That(toks[counter++].TokenType, Is.EqualTo(TokenType.Value));
            Assert.That(toks[counter++].TokenType, Is.EqualTo(TokenType.EndOfStatement));
            Assert.That(toks[counter++].TokenType, Is.EqualTo(TokenType.Identifier));
            Assert.That(toks[counter++].TokenType, Is.EqualTo(TokenType.AddAdd));
            Assert.That(toks[counter++].TokenType, Is.EqualTo(TokenType.ParanthesesClose));
            Assert.That(toks[counter++].TokenType, Is.EqualTo(TokenType.AccoladesOpen));
            Assert.That(toks[counter++].TokenType, Is.EqualTo(TokenType.AccoladesClose));
            Assert.That(toks[counter++].TokenType, Is.EqualTo(TokenType.EndOfFile));
        }

        [TestCase("true?true:false")]
        [TestCase("true ? true : false")]
        [TestCase("\ntrue\n ?\n true\n :\n false")]
        [TestCase("\r\ntrue\r\n ?\r\n true\r\n :\r\n false")]
        public static void Lexer_Test_Terniary_Statement(string code)
        {
            var lexer = new Lexer(code);

            var toks = lexer.ConsumeTokens(6);
            Assert.That(toks[0].TypeIndicator, Is.EqualTo(TypeIndicator.Boolean));
            Assert.That(toks[0].TokenType, Is.EqualTo(TokenType.Value));
            Assert.That(toks[1].TokenType, Is.EqualTo(TokenType.TerniaryOperatorTrue));
            Assert.That(toks[2].TypeIndicator, Is.EqualTo(TypeIndicator.Boolean));
            Assert.That(toks[2].TokenType, Is.EqualTo(TokenType.Value));
            Assert.That(toks[3].TokenType, Is.EqualTo(TokenType.TerniaryOperatorFalse));
            Assert.That(toks[4].TypeIndicator, Is.EqualTo(TypeIndicator.Boolean));
            Assert.That(toks[4].TokenType, Is.EqualTo(TokenType.Value));
            Assert.That(toks[5].TokenType, Is.EqualTo(TokenType.EndOfFile));
        }

        [Test]
        public static void Lexer_Test_Boolean_Statement()
        {
            var lexer = new Lexer("true && false || true");

            var toks = lexer.ConsumeTokens(6);
            Assert.That(toks[0].TypeIndicator, Is.EqualTo(TypeIndicator.Boolean));
            Assert.That(toks[0].TokenType, Is.EqualTo(TokenType.Value));
            Assert.That(toks[1].TokenType, Is.EqualTo(TokenType.ConditionalAnd));
            Assert.That(toks[2].TypeIndicator, Is.EqualTo(TypeIndicator.Boolean));
            Assert.That(toks[2].TokenType, Is.EqualTo(TokenType.Value));
            Assert.That(toks[3].TokenType, Is.EqualTo(TokenType.ConditionalOr));
            Assert.That(toks[4].TypeIndicator, Is.EqualTo(TypeIndicator.Boolean));
            Assert.That(toks[4].TokenType, Is.EqualTo(TokenType.Value));
            Assert.That(toks[5].TokenType, Is.EqualTo(TokenType.EndOfFile));
        }

        [Test]
        public void Lexer_Test_Import_Statement()
        {
            var lexer = new Lexer("import \"this\\is\\a\\file\\path\";");
            var toks = lexer.ConsumeTokens(3);
            Assert.That(toks[0].TokenType, Is.EqualTo(TokenType.ImportStatement));
            Assert.That(toks[1].TypeIndicator, Is.EqualTo(TypeIndicator.String));
            Assert.That(toks[1].TokenType, Is.EqualTo(TokenType.Value));
            Assert.That(toks[1].Value, Is.EqualTo("this\\is\\a\\file\\path"));
            Assert.That(toks[2].TokenType, Is.EqualTo(TokenType.EndOfStatement));
        }

        [Test]
        public void Lexer_Test_Member_Access()
        {
            var lexer = new Lexer("parent.member");
            var toks = lexer.ConsumeTokens(3);
            Assert.That(toks[0].TokenType, Is.EqualTo(TokenType.Identifier));
            Assert.That(toks[1].TokenType, Is.EqualTo(TokenType.Dot));
            Assert.That(toks[2].TokenType, Is.EqualTo(TokenType.Identifier));
        }

        [Test]
        public void Lexer_Test_Context()
        {
            var code = "context this.is.a.test;";
            var lexer = new Lexer(code);
            var toks = lexer.ConsumeTokens(3);
            var counter = -1;
            Assert.That(toks[++counter].TokenType, Is.EqualTo(TokenType.ContextStatement));
            Assert.That(toks[++counter].TokenType, Is.EqualTo(TokenType.Identifier));
            Assert.That(toks[++counter].TokenType, Is.EqualTo(TokenType.EndOfFile));

        }

        [Test]
        public void Class_Test_Context()
        {
            var code = "class ThisIsAClass";
            var lexer = new Lexer(code);
            var toks = lexer.ConsumeTokens(2);
            var counter = -1;
            Assert.That(toks[++counter].TokenType, Is.EqualTo(TokenType.Class));
            Assert.That(toks[++counter].TokenType, Is.EqualTo(TokenType.Identifier));

        }
    }
}
