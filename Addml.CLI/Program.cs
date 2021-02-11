using System;

namespace Addml.CLI
{
    class Program
    {
        static void Main(string[] args)
        {
            Addml.API.API.Run(args);
            Console.Read();
        }
    }
}
