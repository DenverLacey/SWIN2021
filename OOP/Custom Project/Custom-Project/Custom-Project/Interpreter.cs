﻿using System;
using System.IO;
using System.Text;

namespace CustomProject
{
    public class Interpreter
    {
        private Tokeniser tokeniser;
        private Parser parser;
        private VM vm;

        public Interpreter(string filepath)
        {
            tokeniser = new Tokeniser();
            parser = new Parser();
            vm = new VM();
            vm.SetEnvironment(null, vm);

            string source = GetSourceCode(filepath);

            var tokens = tokeniser.Tokenize(source);
            var program = parser.Parse(tokens);

            vm.Execute(program);

            Console.WriteLine("Variables:");
            foreach (var binding in vm.Variables)
            {
                Console.WriteLine("{0}: {1} = {2}", binding.Key, binding.Value.Type, binding.Value);
            }

            Console.WriteLine("\nConstants:");
            foreach (var binding in vm.Constants)
            {
                Console.WriteLine("{0}: {1} = {2}", binding.Key, binding.Value.Type, binding.Value);
            }
        }

        private static string GetSourceCode(string filepath)
        {
            FileStream fstream = File.OpenRead(filepath);
            byte[] bytes = new byte[fstream.Length];
            fstream.Read(bytes, 0, (int)fstream.Length);
            return Encoding.ASCII.GetString(bytes);
        }
    }
}