using Compiling;
using System.Diagnostics;

namespace CLI
{

    public class Program
    {

        public static void Main(params string[] args)
        {
            Console.WriteLine("Press to start.");
            Console.ReadKey();
            //var code = "2.0 + 5.0 / 5.0;";
            //var code = "10d*(10d-5d);";
            //var code = "func main() -> int {if(false){return 69;} else { if(false) {return 666;} else {return 1337;}}}";
            //var code = "func main() -> int {if(true) {return 10;} return 69;}";

            //var code = @"func main() -> int {
            //                var x = 1;
            //                if(0 > x) { x =69; } else if(false) {x = 420;} else {x = 1337;}

            //                return x;
            //           }";

            var code = @" func main() -> int 
                        {
                            var x = 0;
                            for(var i = 0; i < 42; i = i + 1)
                            {
                                x = x + 1;
                            }

                            return x;
                        }";

            Driver.RunLLVM(code, isExecutable: true, useClangCompiler: false);
            var proc = Process.Start("output.exe");
            proc.WaitForExit();
            Console.WriteLine($"Process ExitCode: {proc.ExitCode}.");
            //code = "2.0 * 7.0;";
            //Driver.RunLLVM(code);

            //code = "2.0 / 6.0;";
            //Driver.RunLLVM(code);

            Console.WriteLine("Press to exit.");
            Console.ReadKey();
        }
    }
}
