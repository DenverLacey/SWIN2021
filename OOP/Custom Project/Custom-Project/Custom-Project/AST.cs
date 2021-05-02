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

    public class SuperStatement : Block
    {
        public override Value Execute(VM vm)
        {
            if (!vm.Parent.Constants.ContainsKey("self") ||
                !(vm.Parent.Constants["self"] is InstanceValue))
            {
                throw new Exception("Can only call 'super()' inside a class.");
            }

            InstanceValue self = vm.Parent.Constants["self"].Instance;
            ClassValue selfClass = self.UpCast();

            if (!selfClass.Methods.ContainsKey("<SUPER>"))
            {
                throw new Exception(string.Format("Cannot call 'super()'. '{0}' is not a subclass.", selfClass.Name));
            }

            LambdaExpression super = selfClass.Methods["<SUPER>"].Lambda;

            if (Expressions.Count != super.Args.Count)
            {
                throw new Exception(string.Format(
                    "Argument Mistmatch! {0}.super takes {1} argument(s) but was given {2}.",
                    selfClass.Name, super.Args.Count, Expressions.Count));
            }

            VM @new = new VM(null, vm.Global);
            @new.Constants.Add("self", self);

            for (int i = 0; i < super.Args.Count; i++)
            {
                string argId = super.Args[i];
                Value arg = Expressions[i].Execute(vm);
                @new.Variables.Add(argId, arg);
            }

            try
            {
                super.Body.Execute(@new);
            }
            catch (ReturnStatement.Signal sig)
            {
                if (!sig.Value.IsNil())
                {
                    throw new Exception("Cannot return a value from class initializer.");
                }
            }

            self.Cast(selfClass);

            return new NilValue();
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

    public class MemberReferenceAssignment : IAST
    {
        IAST instance;
        string member;
        IAST assigner;

        public MemberReferenceAssignment(IAST instance, string member, IAST assigner)
        {
            this.instance = instance;
            this.member = member;
            this.assigner = assigner;
        }

        public Value Execute(VM vm)
        {
            Value instValue = instance.Execute(vm);
            Value.AssertType(Value.ValueType.Instance, instValue,
                "First operand of '.' expected to be an instance of a class but was given '{0}'.", instValue.Type);
            InstanceValue inst = instValue.Instance;
            inst.Fields[member] = assigner.Execute(vm);
            return new NilValue();
        }
    }

    public class BoundMethod : IAST
    {
        public IAST Receiver { get; private set; }
        public string Method { get; private set; }

        public BoundMethod(IAST receiver, string method)
        {
            Receiver = receiver;
            Method = method;
        }

        public Value Execute(VM vm)
        {
            throw new Exception("Internal: Should not execute a BoundMethod directly.");
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

    public class ForStatement : IAST
    {
        string iter;
        string counter;
        IAST iterable;
        Block body;

        public ForStatement(string iter, string counter, IAST iterable, Block body)
        {
            this.iter = iter;
            this.counter = counter;
            this.iterable = iterable;
            this.body = body;
        }

        public Value Execute(VM vm)
        {
            Value iterableValue = iterable.Execute(vm);
            switch (iterableValue.Type)
            {
                case Value.ValueType.List:
                    IterateOverList(vm, iterableValue.List);
                    break;

                case Value.ValueType.String:
                    IterateOverString(vm, iterableValue as StringValue);
                    break;

                case Value.ValueType.Range:
                    IterateOverRange(vm, iterableValue.Range);
                    break;

                default:
                    throw new Exception(string.Format("Cannot iterate over something of type '{0}'.", iterableValue.Type));
            }
            return new NilValue();
        }

        private void IterateOverList(VM vm, List<Value> list)
        {
            int count = 0;
            while (true)
            {
                if (count >= list.Count)
                {
                    break;
                }

                Value it = list[count];

                VM @new = new VM(vm, vm.Global);
                if (DoExecution(@new, it, count))
                {
                    break;
                }

                list[count] = @new.Variables[iter];
                count++;
            }
        }

        private void IterateOverString(VM vm, StringValue str)
        {
            char[] chars = str.String.ToCharArray();
            int count = 0;
            while (true)
            {
                if (count >= chars.Length)
                {
                    break;
                }

                Value it = new CharValue(chars[count]);

                VM @new = new VM(vm, vm.Global);
                if (DoExecution(@new, it, count))
                {
                    break;
                }

                chars[count] = @new.Variables[iter].Char;
                count++;
            }
            str.ReplaceString(new string(chars));
        }

        private void IterateOverRange(VM vm, RangeValue range)
        {
            Value it = range.Start;
            int count = 0;

            while (true)
            {
                Value end = range.End;
                switch (it.Type)
                {
                    case Value.ValueType.Number:
                        {
                            bool @break;
                            if (range.Inclusive)
                            {
                                @break = it.Number > end.Number;
                            }
                            else
                            {
                                @break = it.Number >= end.Number;
                            }

                            if (@break) return;
                            break;
                        }

                    case Value.ValueType.Char:
                        {
                            bool @break;
                            if (range.Inclusive)
                            {
                                @break = it.Char > end.Char;
                            }
                            else
                            {
                                @break = it.Char >= end.Char;
                            }

                            if (@break) return;
                            break;
                        }

                    default:
                        throw new Exception(string.Format("Internal Error: Range<{0}>.", it.Type));
                }

                VM @new = new VM(vm, vm.Global);
                if (DoExecution(@new, it, count))
                {
                    break;
                }

                count++;

                switch (it.Type)
                {
                    case Value.ValueType.Number:
                        it = new NumberValue(it.Number + 1);
                        break;

                    case Value.ValueType.Char:
                        it = new CharValue((char)(it.Char + 1));
                        break;

                    default:
                        throw new Exception(string.Format("Internal Error: Range<{0}>.", it.Type));
                }
            }
        }

        private bool DoExecution(VM vm, Value it, int count)
        {
            vm.Variables.Add(iter, it);
            if (counter != null) vm.Variables.Add(counter, new NumberValue(count));

            try
            {
                body.Execute(vm);
            }
            catch (BreakStatement.Signal)
            {
                return true;
            }
            catch (ContinueStatement.Signal)
            {
            }

            return false;
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

    public class ClassDeclaration : IAST
    {
        string name;
        string superClass;
        List<LambdaExpression> methods;

        public ClassDeclaration(string name, string superClass, List<LambdaExpression> methods)
        {
            this.name = name;
            this.superClass = superClass;
            this.methods = methods;
        }

        public Value Execute(VM vm)
        {
            ClassValue super = null;
            if (superClass != null)
            {
                if (!vm.Constants.ContainsKey(superClass))
                {
                    throw new Exception(string.Format("Unresolved identifier '{0}'.", superClass));
                }

                Value superClassValue = vm.Constants[superClass];
                Value.AssertType(Value.ValueType.Class, superClassValue,
                    "Cannot inherit from something of type '{0}'.", superClassValue.Type);
                super = superClassValue.Class;
            }

            var @class = new ClassValue(name, super);

            if (super != null)
            {
                foreach (var method in super.Methods)
                {
                    string methodName = method.Key;
                    if (methodName == "<SUPER>") continue;
                    if (method.Key == "init") methodName = "<SUPER>";
                    @class.Methods[methodName] = method.Value;
                }
            }

            foreach (var method in methods)
            {
                string methodName = method.Id;
                LambdaValue methodVal = method.Execute(vm) as LambdaValue;
                @class.Methods[methodName] = methodVal;
            }

            vm.Constants.Add(name, @class);
            return new NilValue();
        }
    }

    public class MemberReference : IAST
    {
        public string Member { get; private set; }
        public IAST Instance { get; private set; }

        public MemberReference(IAST instance, string member)
        {
            Member = member;
            Instance = instance;
        }

        public Value Execute(VM vm)
        {
            Value inst = Instance.Execute(vm);
            switch (inst.Type)
            {
                case Value.ValueType.Instance:
                    return MemberReferenceInstance(inst as InstanceValue);

                case Value.ValueType.String:
                    return MemberReferenceString(inst as StringValue);

                case Value.ValueType.List:
                    return MemberReferenceList(inst as ListValue);

                default:
                    throw new Exception(string.Format("Cannot refer to members of something of type '{0}'.", inst.Type));
            }
        }

        private Value MemberReferenceInstance(InstanceValue value)
        {
            return value.Fields[Member];
        }

        private Value MemberReferenceString(StringValue value)
        {
            switch (Member)
            {
                case "length":
                    return new NumberValue(value.String.Length);

                default:
                    throw new Exception(string.Format("'{0}' is not a member of 'String'.", Member));
            }
        }

        private Value MemberReferenceList(ListValue value)
        {
            switch (Member)
            {
                case "capacity":
                    return new NumberValue(value.List.Capacity);

                case "length":
                    return new NumberValue(value.List.Count);

                default:
                    throw new Exception(string.Format("'{0}' is not a member of 'List'.", Member));
            }
        }
    }
}
