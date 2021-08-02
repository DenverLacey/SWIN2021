using System;
using System.Collections.Generic;

namespace CustomProject
{
    /// <summary>
    /// Interface for every node in the Abstract Syntax Tree.
    /// </summary>
    public interface IAST
    {
        /// <summary>
        /// Executes the node and its children and returns the result.
        /// </summary>
        /// <param name="vm">
        /// If the node needs to access any variables or constants; or
        /// add some. It can do so via this parameter.
        /// </param>
        /// <returns>The resulting <see cref="Value"/> after executing the node.</returns>
        Value Execute(VM vm);
    }

    /// <summary>
    /// Represents a literal value in the AST.
    /// </summary>
    public class Literal : IAST
    {
        Value value;

        public Literal(Value value)
        {
            this.value = value;
        }

        /// <summary>
        /// Gets the literal value.
        /// </summary>
        /// <param name="vm">Not needed other than to conform to the <see cref="IAST"/> interface.</param>
        /// <returns>The literal value.</returns>
        public Value Execute(VM vm)
        {
            return value;
        }
    }

    /// <summary>
    /// Represents a block of code in the AST.
    /// </summary>
    public class Block : IAST
    {
        /// <summary>
        /// List of all AST nodes that are apart of the block.
        /// </summary>
        public List<IAST> Nodes { get; private set; }

        public Block()
        {
            Nodes = new List<IAST>();
        }

        /// <summary>
        /// Adds an AST node to <see cref="Nodes"/>.
        /// </summary>
        /// <param name="node">Node to add.</param>
        public void AddNode(IAST node)
        {
            Nodes.Add(node);
        }

        /// <summary>
        /// Executes all nodes in the block under a new scope and returns the result
        /// of the last node in the block.
        /// </summary>
        /// <param name="vm">The parent scope of this block.</param>
        /// <returns>Result of the last node of the block.</returns>
        public virtual Value Execute(VM vm)
        {
            VM scope = new VM(vm, vm.Global);
            Value ret = new NilValue();
            foreach (IAST expr in Nodes)
            {
                ret = expr.Execute(scope);
            }
            return ret;
        }
    }

    /// <summary>
    /// Represents a list expression in the AST.
    /// </summary>
    public class ListExpression : Block
    {
        /// <summary>
        /// Executes all element expressions of the list expression and returns them
        /// as a <see cref="ListValue"/>.
        /// </summary>
        /// <param name="vm">Environment to execute the list's element nodes in.</param>
        /// <returns>The resulting <see cref="ListValue"/>.</returns>
        public override Value Execute(VM vm)
        {
            List<Value> values = new List<Value>();
            foreach (IAST element in Nodes)
            {
                values.Add(element.Execute(vm));
            }
            return new ListValue(values);
        }
    }

    /// <summary>
    /// Represents a super statement in the AST.
    /// </summary>
    /// <remarks>A super statement is used to call a superclass's 'init()' method.</remarks>
    public class SuperStatement : Block
    {
        /// <summary>
        /// Calls the init method of an instance's superclass.
        /// </summary>
        /// <remarks>
        /// Expects a constant called 'self' to be present in the VM.
        /// Expects it to have a superclass with an 'init()' method.
        /// </remarks>
        /// <param name="vm">Used to access global scope.</param>
        /// <returns>A <see cref="NilValue"/>.</returns>
        /// <exception cref="Exception">If an expectation is not met or another runtime error occurs.</exception>
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

            if (Nodes.Count != super.Args.Count)
            {
                throw new Exception(string.Format(
                    "Argument Mistmatch! {0}.super takes {1} argument(s) but was given {2}.",
                    selfClass.Name, super.Args.Count, Nodes.Count));
            }

            VM @new = new VM(null, vm.Global);
            @new.Constants.Add("self", self);

            for (int i = 0; i < super.Args.Count; i++)
            {
                string argId = super.Args[i];
                Value arg = Nodes[i].Execute(vm);
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

    /// <summary>
    /// Represents an identifier in the AST.
    /// </summary>
    public class Identifier : IAST
    {
        /// <summary>
        /// string representation of the identifier.
        /// </summary>
        public string Id { get; private set; }

        public Identifier(string id)
        {
            Id = id;
        }

        /// <summary>
        /// Gets the <see cref="Value"/> currently associated with the identifier.
        /// </summary>
        /// <param name="vm">Where the <see cref="Value"/> is expected to be found.</param>
        /// <returns>The <see cref="Value"/> that corresponds with the identifier.</returns>
        /// <exception cref="Exception">If identifier cannot be resolved.</exception>
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

    /// <summary>
    /// Represents a variable assignment in the AST.
    /// </summary>
    public class VariableAssignment : IAST
    {
        protected string id;
        protected IAST assigner;

        public VariableAssignment(string id, IAST assigner)
        {
            this.id = id;
            this.assigner = assigner;
        }

        /// <summary>
        /// Assigns new <see cref="Value"/> to variable that corresponds with
        /// <see cref="id"/> by executing <see cref="assigner"/>.
        /// </summary>
        /// <param name="vm"></param>
        /// <returns>A <see cref="NilValue"/>.</returns>
        /// <exception cref="Exception">
        /// If <see cref="id"/> corresponds with a constant
        /// or couldn't be resolved at all.
        /// </exception>
        public virtual Value Execute(VM vm)
        {
            return DoExecute(vm, vm);
        }

        /// <summary>
        /// Looks for the variable that corresponds with <see cref="id"/> and
        /// changes its value to the result of executing <see cref="assigner"/>.
        /// </summary>
        /// <param name="vm">The environment to execute <see cref="assigner"/> in.</param>
        /// <param name="lookup">The environment that <see cref="id"/> is expected to exist in.</param>
        /// <returns>A <see cref="NilValue"/>.</returns>
        /// <exception cref="Exception">
        /// If <see cref="id"/> corresponds with a constant or cannot be resolved at all.
        /// </exception>
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

    /// <summary>
    /// Represents variable assignment through a subscript operation in the AST.
    /// </summary>
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

        /// <summary>
        /// Assign new value to the element at the index returned by executing <see cref="subscript"/>
        /// in the list returned by executing <see cref="list"/>. The new value is returned by
        /// executing <see cref="assigner"/>.
        /// </summary>
        /// <param name="vm">The environment in which the child nodes are executed.</param>
        /// <returns>A <see cref="NilValue"/>.</returns>
        /// <exception cref="Exception">If <see cref="list"/> is not a <see cref="ListValue"/> or
        /// if <see cref="subscript"/> is not a <see cref="NumberValue"/>.</exception>
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

    /// <summary>
    /// Represents variable assignment through a member reference in the AST.
    /// </summary>
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

        /// <summary>
        /// Assigns new value to a field of an instance.
        /// The new value is gotten by executing <see cref="assigner"/>.
        /// The instance is gotten by executing <see cref="instance"/>.
        /// <see cref="member"/> is used to access the correct field of the instance.
        /// </summary>
        /// <param name="vm">Environment to execute <see cref="instance"/> and <see cref="assigner"/> in.</param>
        /// <returns>A <see cref="NilValue"/>.</returns>
        /// <exception cref="Exception">If <see cref="instance"/> doesn't return a <see cref="InstanceValue"/>.</exception>
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

    /// <summary>
    /// Represents a bound method in the AST.
    /// </summary>
    /// <remarks>
    /// A bound method is used for calling methods with a dot-call syntax like: <c>x.f()</c>
    /// </remarks>
    public class BoundMethod : IAST
    {
        /// <summary>
        /// The instance that will be passed to the method.
        /// </summary>
        public IAST Receiver { get; private set; }

        /// <summary>
        /// Name of the method to call.
        /// </summary>
        public string Method { get; private set; }

        public BoundMethod(IAST receiver, string method)
        {
            Receiver = receiver;
            Method = method;
        }

        /// <summary>
        /// Should not be called! Always throws an exception when called.
        /// </summary>
        /// <exception cref="Exception">Shouldn't be called.</exception>
        public Value Execute(VM vm)
        {
            throw new Exception("Internal: Should not execute a BoundMethod directly.");
        }
    }

    /// <summary>
    /// Represents an if statement in the AST.
    /// </summary>
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

        /// <summary>
        /// Executes <see cref="then"/> if <see cref="cond"/> returns a true <see cref="BooleanValue"/>.
        /// </summary>
        /// <param name="vm">The environment in which to execute <see cref="cond"/> and <see cref="then"/>.</param>
        /// <returns>True if <see cref="then"/> was executed. False if not.</returns>
        /// <exception cref="Exception">If <see cref="cond"/> didn't return a <see cref="BooleanValue"/>.</exception>
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

        /// <summary>
        /// Executes <see cref="then"/> if <see cref="cond"/> returns a true <see cref="BooleanValue"/>.
        /// If not, <see cref="@else"/> is executed if it isn't null.
        /// </summary>
        /// <param name="vm">The environment is which to execute the if statement.</param>
        /// <returns>A <see cref="NilValue"/>.</returns>
        /// <exception cref="Exception">If <see cref="cond"/> doesn't return a <see cref="BooleanValue"/>.</exception>
        public Value Execute(VM vm)
        {
            if (!ExecuteThenBlock(vm) && @else != null)
            {
                @else.Execute(vm);
            }
            
            return new NilValue();
        }
    }

    /// <summary>
    /// Represents a while loop in the AST.
    /// </summary>
    public class WhileStatement : IAST
    {
        IAST cond;
        Block body;

        public WhileStatement(IAST cond, Block body)
        {
            this.cond = cond;
            this.body = body;
        }

        /// <summary>
        /// Keeps executing <see cref="body"/> while <see cref="cond"/> returns a true <see cref="BooleanValue"/>.
        /// </summary>
        /// <param name="vm">The environment in which to execute the while loop.</param>
        /// <returns>A <see cref="NilValue"/>.</returns>
        /// <exception cref="Exception">If cond doesn't return a <see cref="BooleanValue"/>.</exception>
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

    /// <summary>
    /// Represents a for loop in the AST.
    /// </summary>
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

        /// <summary>
        /// Executes <see cref="body"/> for each item in <see cref="iterable"/>.
        /// </summary>
        /// <param name="vm">The environment in which to execute the for loop.</param>
        /// <returns>A <see cref="NilValue"/>.</returns>
        /// <exception cref="Exception">
        /// If <see cref="iterable"/> returns something that
        /// can't be iterated over.
        /// </exception>
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

        /// <summary>
        /// Iterates over a list of values.
        /// </summary>
        /// <param name="vm">The environment in which to execute the for loop's body.</param>
        /// <param name="list">List to iterate over.</param>
        private void IterateOverList(VM vm, List<Value> list)
        {
            for (int count = 0; count < list.Count; count++)
            {
                Value it = list[count];

                VM @new = new VM(vm, vm.Global);
                if (DoExecution(@new, it, count))
                {
                    break;
                }

                list[count] = @new.Variables[iter];
            }
        }

        /// <summary>
        /// Iterates over a string of characters.
        /// </summary>
        /// <param name="vm">The environment in which to execute the for loop's body.</param>
        /// <param name="str"><see cref="StringValue"/> to iterate over.</param>
        private void IterateOverString(VM vm, StringValue str)
        {
            char[] chars = str.String.ToCharArray();
            for (int count = 0; count < chars.Length; count++)
            {
                Value it = new CharValue(chars[count]);

                VM @new = new VM(vm, vm.Global);
                if (DoExecution(@new, it, count))
                {
                    break;
                }

                chars[count] = @new.Variables[iter].Char;
            }
            str.ReplaceString(new string(chars));
        }

        /// <summary>
        /// Iterates over a range of values.
        /// </summary>
        /// <param name="vm">The environment in which to execute the for loop's body.</param>
        /// <param name="range">The range to iterate over.</param>
        private void IterateOverRange(VM vm, RangeValue range)
        {
            Value it = range.Start;
            Value end = range.End;
            int count = 0;

            while (true)
            {
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

        /// <summary>
        /// Executes a single iteration of the for loop.
        /// </summary>
        /// <param name="vm">The environment in which to execute the iteration.</param>
        /// <param name="it"><see cref="Value"/> of this iteration.</param>
        /// <param name="count">number of iterations already executed.</param>
        /// <returns>True if need to break loop. False if otherwise.</returns>
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

    /// <summary>
    /// Represents a break statement in the AST.
    /// </summary>
    public class BreakStatement : IAST
    {
        public BreakStatement()
        {
        }

        /// <summary>
        /// Signal used to notify parent nodes that a break statement has been executed.
        /// </summary>
        public class Signal : Exception
        {
        }

        /// <summary>
        /// Throws a <see cref="Signal"/>.
        /// </summary>
        public Value Execute(VM vm)
        {
            throw new Signal();
        }
    }

    /// <summary>
    /// Represents a continue statement in the AST.
    /// </summary>
    public class ContinueStatement : IAST
    {
        public ContinueStatement()
        {
        }

        /// <summary>
        /// Signal used to notify parent nodes that a continue statement has been executed.
        /// </summary>
        public class Signal : Exception
        {
        }

        /// <summary>
        /// Throws a <see cref="Signal"/>.
        /// </summary>
        public Value Execute(VM vm)
        {
            throw new Signal();
        }
    }

    /// <summary>
    /// Represents a lambda expression or function / method in the AST.
    /// </summary>
    public class LambdaExpression : IAST
    {
        /// <summary>
        /// Names of the lambda's arguments.
        /// </summary>
        public List<string> Args { get; private set; }

        /// <summary>
        /// Body of the lambda.
        /// </summary>
        public Block Body { get; private set; }

        /// <summary>
        /// Name of the function / method.
        /// </summary>
        public string Id { get; private set; }

        private bool varargs;

        public LambdaExpression(List<string> args, Block body, bool varargs, string id = "<LAMBDA>")
        {
            Args = args;
            Body = body;
            this.varargs = varargs;
            Id = id;
        }

        /// <summary>
        /// Returns the <see cref="LambdaExpression"/> wrapped in a <see cref="LambdaValue"/>.
        /// </summary>
        /// <returns>A <see cref="LambdaValue"/>.</returns>
        public Value Execute(VM vm)
        {
            return new LambdaValue(this);
        }

        /// <summary>
        /// Check if lambda allows for a variable number of arguments or not.
        /// </summary>
        /// <returns>True if lambda is varargs. False if not.</returns>
        public bool IsVarargs()
        {
            return varargs;
        }
    }

    /// <summary>
    /// Represents a variable declaration in the AST.
    /// </summary>
    public class VariableDeclaration : IAST
    {
        protected string id;

        public VariableDeclaration(string id)
        {
            this.id = id;
        }

        /// <summary>
        /// Adds a <see cref="NilValue"/> in <see cref="VM.Variables"/> with <see cref="id"/>
        /// as its key.
        /// </summary>
        /// <param name="vm">The environment in which to add the new variable.</param>
        /// <returns>A <see cref="NilValue"/>.</returns>
        public virtual Value Execute(VM vm)
        {
            vm.Variables.Add(id, new NilValue());
            return new NilValue();
        }
    }

    /// <summary>
    /// Represents a variable instatiation in the AST.
    /// </summary>
    public class VariableInstantiation : VariableDeclaration
    {
        protected IAST initializer;

        public VariableInstantiation(string id, IAST initializer)
            : base(id)
        {
            this.initializer = initializer;
        }

        /// <summary>
        /// Adds a new variable in <see cref="VM.Variables"/> by executing <see cref="initializer"/>
        /// with 'id' as its key.
        /// </summary>
        /// <param name="vm">The environment in which to add the new variable.</param>
        /// <returns>A <see cref="NilValue"/>.</returns>
        public override Value Execute(VM vm)
        {
            Value value = initializer.Execute(vm);
            vm.Variables.Add(id, value);
            return new NilValue();
        }
    }

    /// <summary>
    /// Represents a constant instatiation in the AST.
    /// </summary>
    public class ConstantInstantiation : VariableInstantiation
    {
        public ConstantInstantiation(string id, IAST initializer)
            : base(id, initializer)
        {
        }

        /// <summary>
        /// Adds a new constant to <see cref="VM.Constants"/> by executing 'initializer' with
        /// 'id' as its key.
        /// </summary>
        /// <param name="vm">The environment in which to add the new constant.</param>
        /// <returns>A <see cref="NilValue"/>.</returns>
        public override Value Execute(VM vm)
        {
            Value value = initializer.Execute(vm);
            vm.Constants.Add(id, value);
            return new NilValue();
        }
    }

    /// <summary>
    /// Represents a class declaration in the AST.
    /// </summary>
    public class ClassDeclaration : IAST
    {
        string name;
        string superClass;
        List<LambdaExpression> methods;
        List<LambdaExpression> classMethods;

        public ClassDeclaration(string name, string superClass, List<LambdaExpression> methods, List<LambdaExpression> classMethods)
        {
            this.name = name;
            this.superClass = superClass;
            this.methods = methods;
            this.classMethods = classMethods;
        }

        /// <summary>
        /// Adds a new <see cref="ClassValue"/> to <see cref="VM.Constants"/>.
        /// </summary>
        /// <param name="vm">The environment in which to add the new class.</param>
        /// <returns>A <see cref="NilValue"/>.</returns>
        /// <exception cref="Exception">
        /// If there is an unresolved identifier for its super class.
        /// If super class identifier doesn't correspond to a <see cref="ClassValue"/>.
        /// </exception>
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

                foreach (var method in super.ClassMethods)
                {
                    @class.ClassMethods[method.Key] = method.Value;
                }
            }

            foreach (var method in methods)
            {
                string methodName = method.Id;
                LambdaValue methodVal = method.Execute(vm) as LambdaValue;
                @class.Methods[methodName] = methodVal;
            }

            foreach (var method in classMethods)
            {
                string methodName = method.Id;
                LambdaValue methodVal = method.Execute(vm) as LambdaValue;
                @class.ClassMethods[methodName] = methodVal;
            }

            vm.Constants.Add(name, @class);
            return new NilValue();
        }
    }

    /// <summary>
    /// Represents a member reference in the AST.
    /// </summary>
    public class MemberReference : IAST
    {
        /// <summary>
        /// Name of the member.
        /// </summary>
        public string Member { get; private set; }

        /// <summary>
        /// Instance that owns the member.
        /// </summary>
        public IAST Instance { get; private set; }

        public MemberReference(IAST instance, string member)
        {
            Member = member;
            Instance = instance;
        }

        /// <summary>
        /// Executes <see cref="Instance"/> and returns the member that corresponds to <see cref="Member"/>.
        /// </summary>
        /// <param name="vm">The environment in which to execute <see cref="Instance"/>.</param>
        /// <returns>The member of the instance.</returns>
        /// <exception cref="Exception">
        /// If <see cref="Instance"/> doesn't return a <see cref="InstanceValue"/>, <see cref="StringValue"/> or
        /// <see cref="ListValue"/>.
        /// </exception>
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

        /// <summary>
        /// Returns the field that corresponds to <see cref="Member"/>.
        /// </summary>
        /// <param name="value">The <see cref="InstanceValue"/>.</param>
        /// <returns>The instance's field.</returns>
        private Value MemberReferenceInstance(InstanceValue value)
        {
            return value.Fields[Member];
        }

        /// <summary>
        /// Returns the member of the <see cref="StringValue"/> that corresponds to
        /// <see cref="Member"/>.
        /// </summary>
        /// <param name="value">The <see cref="StringValue"/>.</param>
        /// <returns>The <see cref="StringValue"/>'s member.</returns>
        /// <exception cref="Exception">If <see cref="Member"/> isn't a member of <see cref="StringValue"/>.</exception>
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

        /// <summary>
        /// Returns the member of the <see cref="ListValue"/> that corresponds to
        /// <see cref="Member"/>.
        /// </summary>
        /// <param name="value">The <see cref="ListValue"/>.</param>
        /// <returns>The <see cref="ListValue"/>'s member.</returns>
        /// <exception cref="Exception">If <see cref="Member"/> isn't a member of <see cref="ListValue"/>.</exception>
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
