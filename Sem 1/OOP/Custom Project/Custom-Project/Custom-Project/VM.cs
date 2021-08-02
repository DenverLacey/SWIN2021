using System;
using System.Collections.Generic;

namespace CustomProject
{
    /// <summary>
    /// Responsible for executing the AST as well as storing all variables and constants
    /// generated during execution.
    /// </summary>
    public class VM
    {
        /// <summary>
        /// Used to access variables and constants in a parent scope.
        /// </summary>
        public VM Parent { get; private set; }

        /// <summary>
        /// Used to access the global scope of the program.
        /// </summary>
        public VM Global { get; private set; }

        /// <summary>
        /// Stores all variables in this scope.
        /// </summary>
        public Dictionary<string, Value> Variables { get; private set; }

        /// <summary>
        /// Stores all constants in this scope.
        /// </summary>
        public Dictionary<string, Value> Constants { get; private set; }

        public VM()
        {
            Variables = new Dictionary<string, Value>();
            Constants = new Dictionary<string, Value>();
        }

        /// <summary>
        /// Initializes the <see cref="VM"/> and Sets its environment.
        /// </summary>
        /// <param name="parent">The parent scope.</param>
        /// <param name="global">Global scope.</param>
        public VM(VM parent, VM global)
            : this()
        {
            SetEnvironment(parent, global);
        }

        /// <summary>
        /// Sets the <see cref="VM"/>'s environment.
        /// </summary>
        /// <param name="parent">The parent scope.</param>
        /// <param name="global">Global scope.</param>
        public void SetEnvironment(VM parent, VM global)
        {
            Parent = parent;
            Global = global;
        }

        /// <summary>
        /// Executes the given program.
        /// </summary>
        /// <param name="program">Program to be executed as an AST.</param>
        public void Execute(List<IAST> program)
        {
            foreach (IAST statement in program)
            {
                try
                {
                    statement.Execute(this);
                }
                catch (Exception e)
                {
                    Console.WriteLine(e.Message);
                    break;
                }
            }
        }
    }
}
