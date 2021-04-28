using System;
using System.Collections.Generic;

namespace CustomProject
{
    public class VM
    {
        public Dictionary<string, Value> Variables { get; private set; }

        public VM()
        {
            Variables = new Dictionary<string, Value>();
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
                    return;
                }
            }
        }
    }
}
