using System;
using System.Collections.Generic;
using System.Text;

namespace CustomProject
{
    public abstract class Binary : IAST
    {
        public IAST Lhs { get; private set; }
        public IAST Rhs { get; private set; }

        public Binary(IAST lhs, IAST rhs)
        {
            Lhs = lhs;
            Rhs = rhs;
        }

        public abstract Value Execute(VM vm);
    }

    public class Equality : Binary
    {
        public Equality(IAST lhs, IAST rhs)
            : base(lhs, rhs)
        {
        }

        public override Value Execute(VM vm)
        {
            Value a = Lhs.Execute(vm);
            Value b = Rhs.Execute(vm);
            bool equal = a.Equal(b);
            return new BooleanValue(equal);
        }
    }

    public class Addition : Binary
    {
        public Addition(IAST lhs, IAST rhs)
            : base(lhs, rhs)
        {
        }

        public override Value Execute(VM vm)
        {
            Value a = Lhs.Execute(vm);
            Value b = Rhs.Execute(vm);

            Value.AssertType(Value.ValueType.Number, a,
                "First argument to '+' expected to be 'Number' but was given '{0}'.", a.Type);
            Value.AssertType(Value.ValueType.Number, b,
                "Second argument to '+' expected to be 'Number' but was given '{0}'.", b.Type);

            float numA = a.Number;
            float numB = b.Number;
            float numR = numA + numB;

            return new NumberValue(numR);
        }
    }

    public class Subtraction : Binary
    {
        public Subtraction(IAST lhs, IAST rhs)
            : base(lhs, rhs)
        {
        }

        public override Value Execute(VM vm)
        {
            Value a = Lhs.Execute(vm);
            Value b = Rhs.Execute(vm);

            Value.AssertType(Value.ValueType.Number, a,
                "First argument to '-' expected to be 'Number' but was given '{0}'.", a.Type);
            Value.AssertType(Value.ValueType.Number, b,
                "Second argument to '-' expected to be 'Number' but was given '{0}'.", b.Type);

            float numA = a.Number;
            float numB = b.Number;
            float numR = numA - numB;

            return new NumberValue(numR);
        }
    }

    public class Multiplication : Binary
    {
        public Multiplication(IAST lhs, IAST rhs)
            : base(lhs, rhs)
        {
        }

        public override Value Execute(VM vm)
        {
            Value a = Lhs.Execute(vm);
            Value b = Rhs.Execute(vm);

            Value.AssertType(Value.ValueType.Number, a,
                "First argument to '*' expected to be 'Number' but was given '{0}'.", a.Type);
            Value.AssertType(Value.ValueType.Number, b,
                "Second argument to '*' expected to be 'Number' but was given '{0}'.", b.Type);

            float numA = a.Number;
            float numB = b.Number;
            float numR = numA * numB;

            return new NumberValue(numR);
        }
    }

    public class Division : Binary
    {
        public Division(IAST lhs, IAST rhs)
            : base(lhs, rhs)
        {
        }

        public override Value Execute(VM vm)
        {
            Value a = Lhs.Execute(vm);
            Value b = Rhs.Execute(vm);

            Value.AssertType(Value.ValueType.Number, a,
                "First argument to '/' expected to be 'Number' but was given '{0}'.", a.Type);
            Value.AssertType(Value.ValueType.Number, b,
                "Second argument to '/' expected to be 'Number' but was given '{0}'.", b.Type);

            float numA = a.Number;
            float numB = b.Number;
            float numR = numA / numB;

            return new NumberValue(numR);
        }
    }

    public class Or : Binary
    {
        public Or(IAST lhs, IAST rhs)
            : base(lhs, rhs)
        {
        }

        public override Value Execute(VM vm)
        {
            Value a = Lhs.Execute(vm);
            Value.AssertType(Value.ValueType.Boolean, a,
                "First operand of 'or' expected to be 'Boolean' but was given '{0}'.", a.Type);
            bool boolA = a.Boolean;

            if (boolA)
            {
                return new BooleanValue(boolA);
            }


            Value b = Rhs.Execute(vm);
            Value.AssertType(Value.ValueType.Boolean, b,
                "Second operand of 'or' expected to be 'Boolean' but was given '{0}'.", b.Type);
            return new BooleanValue(b.Boolean);
        }
    }

    public class And : Binary
    {
        public And(IAST lhs, IAST rhs)
            : base(lhs, rhs)
        {
        }

        public override Value Execute(VM vm)
        {
            Value a = Lhs.Execute(vm);
            Value.AssertType(Value.ValueType.Boolean, a,
                "First operand of 'or' expected to be 'Boolean' but was given '{0}'.", a.Type);
            bool boolA = a.Boolean;

            if (!boolA)
            {
                return new BooleanValue(boolA);
            }


            Value b = Rhs.Execute(vm);
            Value.AssertType(Value.ValueType.Boolean, b,
                "Second operand of 'or' expected to be 'Boolean' but was given '{0}'.", b.Type);
            return new BooleanValue(b.Boolean);
        }
    }

    public class LessThan : Binary
    {
        public LessThan(IAST lhs, IAST rhs)
            : base(lhs, rhs)
        {
        }

        public override Value Execute(VM vm)
        {
            Value a = Lhs.Execute(vm);
            Value b = Rhs.Execute(vm);

            Value.AssertType(Value.ValueType.Number, a,
                "First operand of '<' expected to be 'Number' but was given '{0}'.", a.Type);
            Value.AssertType(Value.ValueType.Number, b,
                "Second operand of '<' expected to be 'Number' but was given '{0}'.", b.Type);

            float numA = a.Number;
            float numB = b.Number;

            return new BooleanValue(numA < numB);
        }
    }

    public class GreaterThan : Binary
    {
        public GreaterThan(IAST lhs, IAST rhs)
            : base(lhs, rhs)
        {
        }

        public override Value Execute(VM vm)
        {
            Value a = Lhs.Execute(vm);
            Value b = Rhs.Execute(vm);

            Value.AssertType(Value.ValueType.Number, a,
                "First operand of '>' expected to be 'Number' but was given '{0}'.", a.Type);
            Value.AssertType(Value.ValueType.Number, b,
                "Second operand of '>' expected to be 'Number' but was given '{0}'.", b.Type);

            float numA = a.Number;
            float numB = b.Number;

            return new BooleanValue(numA > numB);
        }
    }

    public class Subscript : Binary
    {
        public Subscript(IAST lhs, IAST rhs)
            : base(lhs, rhs)
        {
        }

        public override Value Execute(VM vm)
        {
            Value list = Lhs.Execute(vm);
            Value idx = Rhs.Execute(vm);

            Value.AssertType(Value.ValueType.List, list,
                "First operand to '[]' expected to be 'List' but was given '{0}'.", list.Type);
            Value.AssertType(Value.ValueType.Number, idx,
                "Second operand to '[]' expected to be 'Number' but was given '{0}'.", idx.Type);

            List<Value> values = list.List;
            float i = idx.Number;

            return values[(int)i];
        }
    }

    public class Invocation : Binary
    {
        public Invocation(IAST lhs, IAST rhs)
            : base(lhs, rhs)
        {
        }

        public override Value Execute(VM vm)
        {
            if (Lhs is BoundMethod boundMethod)
            {
                return InvokeBoundMethod(vm, boundMethod);
            }

            Value callee = Lhs.Execute(vm);
            switch (callee.Type)
            {
                case Value.ValueType.Lambda:
                    return InvokeLambda(vm, callee as LambdaValue);

                case Value.ValueType.Class:
                    return InvokeClass(vm, callee as ClassValue);

                default:
                    throw new Exception(string.Format("Cannot invoke something of type '{0}'.", callee.Type));
            }
        }

        private Value CallWith(LambdaExpression lambda, VM vm)
        {
            Value ret;
            try
            {
                ret = lambda.Body.Execute(vm);
            }
            catch (ReturnStatement.Signal sig)
            {
                ret = sig.Value;
            }

            return ret;
        }

        private void SetArguments(VM vm, VM @new, LambdaExpression lambda, List<IAST> args)
        {
            bool is_varargs = lambda.IsVarargs();

            if (is_varargs)
            {
                if (args.Count < lambda.Args.Count - 1)
                {
                    throw new Exception(string.Format(
                        "Argument Mismatch! '{0}' takes at least {1} argument(s) but was given {2}.",
                        lambda.Id, lambda.Args.Count - 1, args.Count));
                }
            }
            else if (args.Count != lambda.Args.Count)
            {
                throw new Exception(string.Format(
                    "Argument Mistmatch! '{0}' takes {1} argument(s) but was given {2}.",
                    lambda.Id, lambda.Args.Count, args.Count));
            }

            if (is_varargs)
            {
                for (int i = 0; i < lambda.Args.Count - 1; i++)
                {
                    string argId = lambda.Args[i];
                    Value arg = args[i].Execute(vm);
                    @new.Variables.Add(argId, arg);
                }

                List<Value> varargs = new List<Value>();
                for (int i = lambda.Args.Count - 1; i < args.Count; i++)
                {
                    varargs.Add(args[i].Execute(vm));
                }

                string varargsId = lambda.Args[lambda.Args.Count - 1];
                @new.Variables.Add(varargsId, new ListValue(varargs));
            }
            else
            {
                for (int i = 0; i < lambda.Args.Count; i++)
                {
                    string argId = lambda.Args[i];
                    Value arg = args[i].Execute(vm);
                    @new.Variables.Add(argId, arg);
                }
            }
        }

        private Value InvokeLambda(VM vm, LambdaValue value)
        {
            LambdaExpression lambda = value.Lambda;
            Block args = Rhs as Block;
            if (args == null)
            {
                throw new Exception("Internal: 'rhs' of 'Invocation' was not a 'Block'.");
            }

            VM @new = new VM(null, vm.Global);
            @new.Constants.Add(lambda.Id, value); // For recursion
            SetArguments(vm, @new, lambda, args.Expressions);
            return CallWith(lambda, @new);
        }

        private Value InvokeClass(VM vm, ClassValue value)
        {
            var self = new InstanceValue(value);

            if (value.Methods.ContainsKey("init"))
            {
                var init = value.Methods["init"].Lambda;

                Block args = Rhs as Block;
                if (args == null)
                {
                    throw new Exception("Internal: 'rhs' of 'Invocation' was not a 'Block'.");
                }

                VM @new = new VM(null, vm.Global);
                @new.Constants.Add(value.Name, value);
                @new.Constants.Add("self", self);
                SetArguments(vm, @new, init, args.Expressions);

                try
                {
                    init.Body.Execute(@new);
                }
                catch (ReturnStatement.Signal sig)
                {
                    if (!sig.Value.IsNil())
                    {
                        throw new Exception("Cannot return a non-nil value from class initializer.");
                    }
                }
            }

            return self;
        }

        private Value InvokeBoundMethod(VM vm, BoundMethod boundMethod)
        {
            Value receiverValue = boundMethod.Receiver.Execute(vm);

            switch (receiverValue.Type)
            {
                case Value.ValueType.Instance:
                    return InvokeBoundMethodInstance(vm, receiverValue.Instance, boundMethod.Method);

                case Value.ValueType.List:
                    return InvokeBoundMethodList(vm, receiverValue.List, boundMethod.Method);

                case Value.ValueType.String:
                    return InvokeBoundMethodString(vm, receiverValue as StringValue, boundMethod.Method);

                case Value.ValueType.Class:
                    return InvokeBoundMethodClass(vm, receiverValue.Class, boundMethod.Method);

                default:
                    throw new Exception(string.Format("Cannot invoke something of type '{0}'.", receiverValue.Type));
            }
        }

        private Value InvokeBoundMethodInstance(VM vm, InstanceValue receiver, string methodName)
        {
            ClassValue @class = receiver.Class;

            if (!@class.Methods.ContainsKey(methodName))
            {
                throw new Exception(string.Format("'{0}' is not a method of '{1}'.", methodName, @class.Name));
            }

            var method = @class.Methods[methodName].Lambda;

            Block args = Rhs as Block;
            if (args == null)
            {
                throw new Exception("Internal: 'rhs' of 'Invocation' was not a 'Block'.");
            }

            VM @new = new VM(null, vm.Global);
            @new.Constants.Add("self", receiver);
            SetArguments(vm, @new, method, args.Expressions);
            return CallWith(method, @new);
        }

        private Value InvokeBoundMethodList(VM vm, List<Value> list, string methodName)
        {
            switch (methodName)
            {
                case "add":
                    {
                        Block args = Rhs as Block;
                        if (args == null)
                        {
                            throw new Exception("Internal: 'rhs' of 'Invocation' was not a 'Block'.");
                        }

                        if (args.Expressions.Count != 1)
                        {
                            throw new Exception(string.Format("Argument Mistmatch! List.add takes 1 argument(s) but was given {0}.", args.Expressions.Count));
                        }

                        Value value = args.Expressions[0].Execute(vm);
                        list.Add(value);
                        break;
                    }
                case "insert":
                    {
                        Block args = Rhs as Block;
                        if (args == null)
                        {
                            throw new Exception("Internal: 'rhs' of 'Invocation' was not a 'Block'.");
                        }

                        if (args.Expressions.Count != 2)
                        {
                            throw new Exception(string.Format("Argument Mistmatch! List.push takes 2 argument(s) but was given {0}.", args.Expressions.Count));
                        }

                        Value idx = args.Expressions[0].Execute(vm);
                        Value.AssertType(Value.ValueType.Number, idx,
                            "Type mismatch! List.remove expects first argument to be 'Number' but was given '{0}'.", idx.Type);

                        Value value = args.Expressions[1].Execute(vm);

                        list.Insert((int)idx.Number, value);
                        break;
                    }
                case "find":
                    {
                        Block args = Rhs as Block;
                        if (args == null)
                        {
                            throw new Exception("Internal: 'rhs' of 'Invocation' was not a 'Block'.");
                        }

                        if (args.Expressions.Count != 1)
                        {
                            throw new Exception(string.Format("Argument Mistmatch! List.push takes 1 argument(s) but was given {0}.", args.Expressions.Count));
                        }

                        Value value = args.Expressions[0].Execute(vm);
                        return new NumberValue(list.FindIndex(v => v.Equal(value)));
                    }
                case "remove":
                    {
                        Block args = Rhs as Block;
                        if (args == null)
                        {
                            throw new Exception("Internal: 'rhs' of 'Invocation' was not a 'Block'.");
                        }

                        if (args.Expressions.Count != 1)
                        {
                            throw new Exception(string.Format("Argument Mistmatch! List.push takes 1 argument(s) but was given {0}.", args.Expressions.Count));
                        }

                        Value value = args.Expressions[0].Execute(vm);
                        Value.AssertType(Value.ValueType.Number, value,
                            "Type mismatch! List.remove expects first argument to be 'Number' but was given '{0}'.", value.Type);

                        list.RemoveAt((int)value.Number);
                        break;
                    }

                default:
                    throw new Exception(string.Format("'{0}' is not a method of 'List'.", methodName));
            }

            return new NilValue();
        }

        private Value InvokeBoundMethodString(VM vm, StringValue str, string methodName)
        {
            switch (methodName)
            {
                case "concat":
                    {
                        Block args = Rhs as Block;
                        if (args == null)
                        {
                            throw new Exception("Internal: 'rhs' of 'Invocation' was not a 'Block'.");
                        }

                        StringBuilder builder = new StringBuilder(str.String);
                        foreach (IAST expr in args.Expressions)
                        {
                            Value arg = expr.Execute(vm);
                            builder.Append(arg.ToString());
                        }

                        str.ReplaceString(builder.ToString());
                        break;
                    }

                default:
                    throw new Exception(string.Format("'{0}' is not a method of 'String'.", methodName));
            }

            return new NilValue();
        }

        private Value InvokeBoundMethodClass(VM vm, ClassValue value, string methodName)
        {
            if (!value.ClassMethods.ContainsKey(methodName))
            {
                if (value.Methods.ContainsKey(methodName))
                {
                    throw new Exception(string.Format(
                        "'{0}' is not a class method of '{1}'. Requires an instance of the class.",
                        methodName,
                        value.Name));
                }
                else
                {
                    throw new Exception(string.Format("'{0}' is not a method of '{1}'.", methodName, value.Name));
                }
            }

            LambdaExpression method = value.ClassMethods[methodName].Lambda;

            Block args = Rhs as Block;
            if (args == null)
            {
                throw new Exception("Internal: 'rhs' of 'Invocation' was not a 'Block'.");
            }

            VM @new = new VM(null, vm.Global);
            SetArguments(vm, @new, method, args.Expressions);
            return CallWith(method, @new);
        }
    }

    public class RangeExpression : Binary
    {
        bool inclusive;

        public RangeExpression(IAST lhs, IAST rhs, bool inclusive)
            : base(lhs, rhs)
        {
            this.inclusive = inclusive;
        }

        public override Value Execute(VM vm)
        {
            Value start = Lhs.Execute(vm);
            Value end = Rhs.Execute(vm);

            if (!Value.TypesMatch(start, end))
            {
                throw new Exception("start and end values of 'Range' must be the same type.");
            }

            if (start.Type != Value.ValueType.Number && start.Type != Value.ValueType.Char)
            {
                throw new Exception(string.Format("Cannot make a range over '{0}'.", start.Type));
            }

            if (end.Type != Value.ValueType.Number && end.Type != Value.ValueType.Char)
            {
                throw new Exception(string.Format("Cannot make a range over '{0}'.", end.Type));
            }

            return new RangeValue(start, end, inclusive);
        }
    }
}
