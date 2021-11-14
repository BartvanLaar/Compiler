using Compiling;
using System;
using System.Diagnostics;
using System.IO;

namespace CLI
{

    public class Program
    {
        public static void Main(params string[] args)
        {
            CleanUpPrevRun();

            var code = @"func test(int returnValue) -> int {return returnValue;}

                        func main() -> int {
                            var x = test(42);
                            return x;
                        }";


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
