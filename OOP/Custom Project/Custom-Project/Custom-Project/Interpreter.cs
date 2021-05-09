using System;
using System.IO;
using System.Text;

namespace CustomProject
{
    public class Interpreter
    {
        private Tokenizer tokenizer;
        private Parser parser;
        private VM vm;

        public Interpreter()
        {
            tokenizer = new Tokenizer();
            parser = new Parser();
            vm = new VM();
            vm.SetEnvironment(null, vm);
            LoadPrelude();
        }

        public void Interpret(string filepath)
        {
            string source = GetSourceCode(filepath);
            var tokens = tokenizer.Tokenize(source);
            var program = parser.Parse(tokens);
            vm.Execute(program);

            Console.WriteLine("\nVariables:");
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

        private void LoadPrelude()
        {
            const string preludeSource =
            @"
            class String
                fn class.concat(*ss)
                    var result = """"
                    for s in ss
                        result.concat(s)
                    result
            ";

            var preludeTokens = tokenizer.Tokenize(preludeSource);
            var prelude = parser.Parse(preludeTokens);
            vm.Execute(prelude);
        }
    }
}
