using System;

namespace CustomProject
{
    class MainClass
    {
        public static void Main(string[] args)
        {
            string filepath = args.Length > 1 ? args[1] : "source.txt";
            var interpreter = new Interpreter();
            interpreter.Interpret(filepath);
        }
    }
}
