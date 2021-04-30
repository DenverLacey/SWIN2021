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
        public List<IAST> Expressions { get; private set; }

        public Block()
        {
            Expressions = new List<IAST>();
        }

        public void AddExpression(IAST expr)
        {
            Expressions.Add(expr);
        }

        public virtual Value Execute(VM vm)
        {
            VM scope = new VM(vm, vm.Global);
            Value ret = new NilValue();
            foreach (IAST expr in Expressions)
            {
                ret = expr.Execute(scope);
            }
            return ret;
        }
    }

    public class ListExpression : Block
    {
        public override Value Execute(VM vm)
        {
            List<Value> values = new List<Value>();
            foreach (IAST element in Expressions)
            {
                values.Add(element.Execute(vm));
            }
            return new ListValue(values);
        }
    }

    public class Identifier : IAST
    {
        public string Id { get; private set; }

        public Identifier(string id)
        {
            Id = id;
        }

        public Value Execute(VM vm)
        {
            if (vm.Variables.ContainsKey(Id))
            {
                return vm.Variables[Id];
            }
            else if (vm.Constants.ContainsKey(Id))
            {
                return vm.Constants[Id];
            }
            else if (vm.Parent != null)
            {
                return Execute(vm.Parent);
            }
            else if (vm.Global.Variables.ContainsKey(Id))
            {
                return vm.Global.Variables[Id];
            }
            else if (vm.Global.Constants.ContainsKey(Id))
            {
                return vm.Global.Constants[Id];
            }
            else
            {
                throw new Exception(string.Format("Unresolved identifier '{0}'.", Id));
            }
        }
    }

    public class VariableDeclaration : IAST
    {
        protected string id;

        public VariableDeclaration(string id)
        {
            this.id = id;
        }

        public virtual Value Execute(VM vm)
        {
            vm.Variables.Add(id, new NilValue());
            return new NilValue();
        }
    }

    public class VariableInstantiation : VariableDeclaration
    {
        protected IAST initializer;

        public VariableInstantiation(string id, IAST initializer)
            : base(id)
        {
            this.initializer = initializer;
        }

        public override Value Execute(VM vm)
        {
            Value value = initializer.Execute(vm);
            vm.Variables.Add(id, value);
            return new NilValue();
        }
    }

    public class ConstantInstantiation : VariableInstantiation
    {
        public ConstantInstantiation(string id, IAST initializer)
            : base(id, initializer)
        {
        }

        public override Value Execute(VM vm)
        {
            Value value = initializer.Execute(vm);
            vm.Constants.Add(id, value);
            return new NilValue();
        }
    }

    public class VariableAssignment : IAST
    {
        protected string id;
        protected IAST assigner;

        public VariableAssignment(string id, IAST assigner)
        {
            this.id = id;
            this.assigner = assigner;
        }

        public virtual Value Execute(VM vm)
        {
            return DoExecute(vm, vm);
        }

        private Value DoExecute(VM vm, VM lookup)
        {
            if (lookup.Variables.ContainsKey(id))
            {
                lookup.Variables[id] = assigner.Execute(vm);
            }
            else if (lookup.Parent != null)
            {
                return DoExecute(vm, lookup.Parent);
            }
            else if (vm.Global.Variables.ContainsKey(id))
            {
                vm.Global.Variables[id] = assigner.Execute(vm);
            }
            else
            {
                string errMessage;
                if (vm.Constants.ContainsKey(id))
                {
                    errMessage = string.Format("Attempt to assign to constant '{0}'.", id);
                }
                else
                {
                    errMessage = string.Format("Unresolved identifier '{0}'.", id);
                }
                throw new Exception(errMessage);
            }
            return new NilValue();
        }
    }

    public class SubscriptAssignment : IAST
    {
        IAST list;
        IAST subscript;
        IAST assigner;

        public SubscriptAssignment(IAST list, IAST subscript, IAST assigner)
        {
            this.list = list;
            this.subscript = subscript;
            this.assigner = assigner;
        }

        public Value Execute(VM vm)
        {
            Value listVal = list.Execute(vm);
            Value idx = subscript.Execute(vm);

            Value.AssertType(Value.ValueType.List, listVal,
                "First operand of '[]' expected to be 'List' but was given '{0}'.", listVal.Type);
            Value.AssertType(Value.ValueType.Number, idx,
                "Second operand of '[]' expected to be 'Number' but was given '{0}'.", idx.Type);

            List<Value> values = listVal.List;
            float i = idx.Number;

            values[(int)i] = assigner.Execute(vm);

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
            if (condValue.Boolean)
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

    public class WhileStatement : IAST
    {
        IAST cond;
        Block body;

        public WhileStatement(IAST cond, Block body)
        {
            this.cond = cond;
            this.body = body;
        }

        public Value Execute(VM vm)
        {
            while (true)
            {
                Value condValue = cond.Execute(vm);
                Value.AssertType(Value.ValueType.Boolean, condValue,
                    "Condition of 'while' statement must be 'Boolean' but was given '{0}'.", condValue.Type);

                if (!condValue.Boolean)
                    break;

                try
                {
                    body.Execute(vm);
                }
                catch (ContinueStatement.Signal)
                {
                    continue;
                }
                catch (BreakStatement.Signal)
                {
                    break;
                }
            }
            return new NilValue();
        }
    }

    public class BreakStatement : IAST
    {
        public BreakStatement()
        {
        }

        public class Signal : Exception
        {
        }

        public Value Execute(VM vm)
        {
            throw new Signal();
        }
    }

    public class ContinueStatement : IAST
    {
        public ContinueStatement()
        {
        }

        public class Signal : Exception
        {
        }

        public Value Execute(VM vm)
        {
            throw new Signal();
        }
    }

    public class LambdaExpression : IAST
    {
        public List<string> Args { get; private set; }
        public Block Body { get; private set; }
        public string Id { get; private set; }

        public LambdaExpression(List<string> args, Block body, string id = "<LAMBDA>")
        {
            Args = args;
            Body = body;
            Id = id;
        }

        public Value Execute(VM vm)
        {
            return new LambdaValue(this);
        }
    }
}
