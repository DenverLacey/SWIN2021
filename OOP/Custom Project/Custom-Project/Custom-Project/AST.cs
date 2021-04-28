using System;
using System.Collections.Generic;

namespace CustomProject
{
    public interface IAST
    {
        Value Execute(VM vm);
    }

    public class Literal : IAST
    {
        Value value;

        public Literal(Value value)
        {
            this.value = value;
        }

        public Value Execute(VM vm)
        {
            return value;
        }
    }

    public class Block : IAST
    {
        List<IAST> exprs;

        public Block()
        {
            exprs = new List<IAST>();
        }

        public void AddExpression(IAST expr)
        {
            exprs.Add(expr);
        }

        public Value Execute(VM vm)
        {
            foreach (IAST expr in exprs)
            {
                expr.Execute(vm);
            }
            return new NilValue();
        }
    }

    public class Identifier : IAST
    {
        string id;

        public Identifier(string id)
        {
            this.id = id;
        }

        public Value Execute(VM vm)
        {
            return vm.Variables[id];
        }
    }

    public class VariableInstantiation : IAST
    {
        string id;
        IAST initializer;

        public VariableInstantiation(string id, IAST initializer)
        {
            this.id = id;
            this.initializer = initializer;
        }

        public Value Execute(VM vm)
        {
            Value value = initializer.Execute(vm);
            vm.Variables.Add(id, value);
            return new NilValue();
        }
    }

    public class IfStatement : IAST
    {
        IAST cond;
        Block then;
        IAST @else;

        public IfStatement(IAST cond, Block then, IAST @else)
        {
            this.cond = cond;
            this.then = then;
            this.@else = @else;
        }

        private bool ExecuteThenBlock(VM vm)
        {
            Value condValue = cond.Execute(vm);
            Value.AssertType(Value.ValueType.Boolean, condValue,
                "Condition of 'if' statement must be 'Boolean' but was given '{0}'.", condValue.Type);
            if (condValue.GetBoolean())
            {
                then.Execute(vm);
                return true;
            }
            return false;
        }

        public Value Execute(VM vm)
        {
            if (!ExecuteThenBlock(vm))
            {
                if (@else != null)
                {
                    @else.Execute(vm);
                }
            }
            
            return new NilValue();
        }
    }
}
