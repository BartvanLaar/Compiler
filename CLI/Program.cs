using Compiling;
using System.Diagnostics;

namespace CLI
{

    public class Program
    {
        public static void Main(params string[] args)
        {
            CleanUpPrevRun();

            //var code = "func main() -> int {printf(\"Hello World, % s!\r\n\", \"there\"); printf(\"Hello World!\n\");  return 42; }";
            //var code = File.ReadAllText(Path.Combine(Directory.GetCurrentDirectory(), "Program.bs"));
            var code = @"func main() -> int {if(0==true){return 69;} return 1447}";
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
