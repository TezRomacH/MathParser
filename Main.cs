using System;
using System.Globalization;

namespace Parser_C_Sharp
{
    static class Main
    {
        static void Main()
        {
            Parser p = new Parser("data.txt");
            Console.WriteLine(p.Parse("-3 + 4.323 * -2.03 / (-1.15 - 5)^6") + "\n\n");

            p.PrintAst();

            Console.WriteLine("\n" + p.Solve().ToString(CultureInfo.InvariantCulture) + "\n");

            Console.ReadKey();
        }
    }
}
