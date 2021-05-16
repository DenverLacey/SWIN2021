using System;
using System.Collections.Generic;

namespace CustomProject
{
    public class VM
    {
        public VM Parent { get; private set; }
        public VM Global { get; private set; }
        public Dictionary<string, Value> Variables { get; private set; }
        public Dictionary<string, Value> Constants { get; private set; }

        public VM()
        {
            Variables = new Dictionary<string, Value>();
            Constants = new Dictionary<string, Value>();
        }

        public VM(VM parent, VM global)
            : this()
        {
            SetEnvironment(parent, global);
        }

        public void SetEnvironment(VM parent, VM global)
        {
            Parent = parent;
            Global = global;
        }

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
