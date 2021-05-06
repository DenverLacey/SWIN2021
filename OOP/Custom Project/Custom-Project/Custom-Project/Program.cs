using System;

namespace CustomProject
{
    class MainClass
    {
        public static void Main(string[] args)
        {
            string filepath = "source.txt";
            var interpreter = new Interpreter();
            interpreter.Interpret(filepath);
        }
    }
}
