using Compiling;

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
            var code = "func main() -> int {if(false) {return 10;} else if(true){return 42;}}";
            Driver.RunLLVM(code, isExecutable: true, useClangCompiler: false);
            //code = "2.0 * 7.0;";
            //Driver.RunLLVM(code);

            //code = "2.0 / 6.0;";
            //Driver.RunLLVM(code);

            Console.WriteLine("Press to exit.");
            Console.ReadKey();
        }
    }
}
