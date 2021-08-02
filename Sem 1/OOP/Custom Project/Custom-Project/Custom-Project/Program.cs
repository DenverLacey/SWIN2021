using System;

namespace CustomProject
{
    class MainClass
    {
        public static void Main(string[] args)
        {
            if (args.Length > 0)
            {
                string filepath = args[0];
                var interpreter = new Interpreter();
                interpreter.Interpret(filepath);
            }
            else
            {
                Console.WriteLine("No filepath given!");
            }
        }
    }
}
