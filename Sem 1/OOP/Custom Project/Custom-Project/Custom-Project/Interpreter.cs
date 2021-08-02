using System;
using System.IO;
using System.Text;

namespace CustomProject
{
    /// <summary>
    /// the main class for interpreting code.
    /// Is made of a tokenizer, parser and virtual machine.
    /// </summary>
    public class Interpreter
    {
        /// <summary>
        /// Responsible for turning raw source code into a list of tokens.
        /// </summary>
        private Tokenizer tokenizer;

        /// <summary>
        /// Responsible for turning a list of tokens into an AST.
        /// </summary>
        private Parser parser;

        /// <summary>
        /// Is used to execute AST and store its outputs.
        /// </summary>
        private VM vm;

        public Interpreter()
        {
            tokenizer = new Tokenizer();
            parser = new Parser();
            vm = new VM();
            vm.SetEnvironment(null, vm);
            LoadPrelude();
        }

        /// <summary>
        /// Interprets a single file of source code and executes it.
        /// </summary>
        /// <param name="filepath">filepath to source code.</param>
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

        /// <summary>
        /// Opens and reads the given file and returns it as a single string.
        /// </summary>
        /// <param name="filepath">File to read.</param>
        /// <returns>Source code written in the file.</returns>
        private static string GetSourceCode(string filepath)
        {
            FileStream fstream = File.OpenRead(filepath);
            byte[] bytes = new byte[fstream.Length];
            fstream.Read(bytes, 0, (int)fstream.Length);
            return Encoding.ASCII.GetString(bytes);
        }

        /// <summary>
        /// Interprets source code for the languages prelude.
        /// </summary>
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
