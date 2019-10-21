using System;

namespace DatasetGenerator
{
    internal class Program
    {
        private static void Main(string[] args)
        {
            Generator gen = new Generator();
            gen.Generate();

            Console.WriteLine("Done");
        }
    }
}