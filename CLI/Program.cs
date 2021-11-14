using Compiling;
using System;
using System.Diagnostics;
using System.IO;

namespace CLI
{

    public class Program
    {
        public static int Test()
        {
            var x = 5;
            return x = x + 1;
        }
        public static void Main(params string[] args)
        {
           Console.WriteLine(Test());
            CleanUpPrevRun();

            //var code = "2.0 + 5.0 / 5.0;";
            //var code = "10d*(10d-5d);";
            //var code = "func main() -> int {if(false){return 69;} else { if(false) {return 666;} else {return 1337;}}}";
            //var code = "func main() -> int {if(true) {return 10;} return 69;}";

            //var code = @"func main() -> int {
            //                var x = 1;
            //                if(0 > x) { x =69; } else {x = 1337;}

            //                return x;
            //           }";

            //var code = @" func main() -> int 
            //            {
            //                var x = 0;
            //                for(var i = 0; i < 42; i = i + 1)
            //                {
            //                    x = x + 1;
            //                }

            //                return x;
            //            }";
            var code = "func main() -> int { var x = 1337; return x %= 3;}";
            //var code = "func test(int someValue) -> int {return someValue;} func main() -> int {return test(42);}";
            //var code = "func main() -> int   {      var x =\"test\";      } return 5;";
            //var code = " func main() -> int                  {                                printf(\"%f\\n\", 5);                            if(true){return 420;} else {return 1337;}                        }";
            Driver.RunLLVM(code, isExecutable: true, useClangCompiler: false);

            //todo: add support for order independent code? required 2 separate parse phases
            if (File.Exists("output.exe"))
            {
                var proc = Process.Start("output.exe");
                proc.WaitForExit();
                Console.WriteLine($"Process ExitCode: {proc.ExitCode}.");
            }
            else
            {
                Console.WriteLine($"Executable was not generated.");

            }

            //code = "2.0 * 7.0;";
            //Driver.RunLLVM(code);

            //code = "2.0 / 6.0;";
            //Driver.RunLLVM(code);

            Console.WriteLine("Press to exit.");
            Console.ReadKey();
        }

        private static void CleanUpPrevRun()
        {
            if (File.Exists("output.exe"))
            {
                File.Delete("output.exe");
            }

            if (File.Exists("output.bc"))
            {
                File.Delete("output.bc");
            }

            if (File.Exists("output.obj"))
            {
                File.Delete("output.obj");
            }
        }
    }
}
