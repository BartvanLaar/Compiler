using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Parser.Tests
{
    internal class DriverTests
    {

        [Test]
        public void Test()
        {
            Driver.Run("x = 2;");
            Driver.Run("var x = 2;");
            Driver.Run("var x = 2 + 3;");
            Driver.Run("var x = 2 / 3;");
            Driver.Run("var x = 2 * 3;");
            Driver.Run("var x = 2 - 3;");
            Driver.Run("var x = 2 - 3; var x = 2 - 3;");
            Driver.Run("var x = 2 - 3; \n var x = 2 - 3;");
            Driver.Run("var x = 2 - 3; \r var x = 2 - 3;");
            Driver.Run("var x = 2 - 3; \r\n var x = 2 - 3;");
            Driver.Run("var x = 2 - 3; x = 2 - 3;");
            Driver.Run("var x = 2 - 3; \n x = 2 - 3;");
            Driver.Run("var x = 2 - 3; \r x = 2 - 3;");
            Driver.Run("var x = 2 - 3; \r\n x = 2 - 3;");

            Driver.Run("auto x = 2;");
            Driver.Run("auto x = 2 + 3;");
            Driver.Run("auto x = 2 / 3;");
            Driver.Run("auto x = 2 * 3;");
            Driver.Run("auto x = 2 - 3;");
            Driver.Run("auto x = 2 - 3; auto x = 2 - 3;");
            Driver.Run("auto x = 2 - 3; \n auto x = 2 - 3;");
            Driver.Run("auto x = 2 - 3; \r auto x = 2 - 3;");
            Driver.Run("auto x = 2 - 3; \r\n auto x = 2 - 3;");
            Driver.Run("auto x = 2 - 3; x = 2 - 3;");
            Driver.Run("auto x = 2 - 3; \n x = 2 - 3;");
            Driver.Run("auto x = 2 - 3; \r x = 2 - 3;");
            Driver.Run("auto x = 2 - 3; \r\n x = 2 - 3;");

        }
    }
}
