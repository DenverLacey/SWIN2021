﻿using System;
using System.Collections.Generic;

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

        private Value InvokeLambda(VM vm, LambdaValue value)
        {
            LambdaExpression lambda = value.Lambda;
            Block args = Rhs as Block;
            if (args == null)
            {
                throw new Exception("Internal: 'rhs' of 'Invocation' was not a 'Block'.");
            }

            if (args.Expressions.Count != lambda.Args.Count)
            {
                throw new Exception(string.Format(
                    "Argument Mistmatch! '{0}' takes {1} argument(s) but was given {2}.",
                    lambda.Id, lambda.Args.Count, args.Expressions.Count));
            }

            VM @new = new VM(null, vm.Global);
            @new.Constants.Add(lambda.Id, value); // For recursion

            for (int i = 0; i < lambda.Args.Count; i++)
            {
                string argId = lambda.Args[i];
                Value arg = args.Expressions[i].Execute(vm);
                @new.Variables.Add(argId, arg);
            }

            Value ret;
            try
            {
                ret = lambda.Body.Execute(@new);
            }
            catch (ReturnStatement.Signal sig)
            {
                ret = sig.Value;
            }
            
            return ret;
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

                if (args.Expressions.Count != init.Args.Count)
                {
                    throw new Exception(string.Format(
                        "Argument Mistmatch! {0}'s initializer takes {1} argument(s) but was given {2}.",
                        value.Name, init.Args.Count, args.Expressions.Count));
                }

                VM @new = new VM(null, vm.Global);
                @new.Constants.Add(value.Name, value);
                @new.Constants.Add("self", self);

                for (int i = 0; i < init.Args.Count; i++)
                {
                    string argId = init.Args[i];
                    Value arg = args.Expressions[i].Execute(vm);
                    @new.Variables.Add(argId, arg);
                }

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
            Value.AssertType(Value.ValueType.Instance, receiverValue,
                "Cannot invoke something of type '{0}'.", receiverValue.Type);
            InstanceValue receiver = receiverValue.Instance;
            ClassValue @class = receiver.Class;

            if (!@class.Methods.ContainsKey(boundMethod.Method))
            {
                throw new Exception(string.Format("'{0}' is not a method of '{1}'.", boundMethod.Method, @class.Name));
            }

            var method = @class.Methods[boundMethod.Method].Lambda;

            Block args = Rhs as Block;
            if (args == null)
            {
                throw new Exception("Internal: 'rhs' of 'Invocation' was not a 'Block'.");
            }

            if (args.Expressions.Count != method.Args.Count)
            {
                throw new Exception(string.Format(
                    "Argument Mistmatch! {0}.{1} takes {2} argument(s) but was given {3}.",
                    @class.Name, method.Id, method.Args.Count, args.Expressions.Count));
            }

            VM @new = new VM(null, vm.Global);
            @new.Constants.Add("self", receiver);

            for (int i = 0; i < method.Args.Count; i++)
            {
                string argId = method.Args[i];
                Value arg = args.Expressions[i].Execute(vm);
                @new.Variables.Add(argId, arg);
            }

            Value ret;
            try
            {
                ret = method.Body.Execute(@new);
            }
            catch (ReturnStatement.Signal sig)
            {
                ret = sig.Value;
            }

            return ret;
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
