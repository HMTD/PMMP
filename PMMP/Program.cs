using System;

namespace PMMP
{
    class Program
    {
        static void Main(string[] args)
        {
            for (int i = 0; i < args.Length; i = i + 1)
                Console.WriteLine(args[i]);
            Console.ReadLine();

        }
    }
}
