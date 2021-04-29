﻿using System;
namespace CustomProject
{
    public abstract class Binary : IAST
    {
        protected IAST lhs;
        protected IAST rhs;

        public Binary(IAST lhs, IAST rhs)
        {
            this.lhs = lhs;
            this.rhs = rhs;
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
            Value a = lhs.Execute(vm);
            Value b = rhs.Execute(vm);
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
            Value a = lhs.Execute(vm);
            Value b = rhs.Execute(vm);

            Value.AssertType(Value.ValueType.Number, a,
                "First argument to '+' expected to be 'Number' but was given '{0}'.", a.Type);
            Value.AssertType(Value.ValueType.Number, b,
                "Second argument to '+' expected to be 'Number' but was given '{0}'.", b.Type);

            float numA = a.GetNumber();
            float numB = b.GetNumber();
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
            Value a = lhs.Execute(vm);
            Value b = rhs.Execute(vm);

            Value.AssertType(Value.ValueType.Number, a,
                "First argument to '-' expected to be 'Number' but was given '{0}'.", a.Type);
            Value.AssertType(Value.ValueType.Number, b,
                "Second argument to '-' expected to be 'Number' but was given '{0}'.", b.Type);

            float numA = a.GetNumber();
            float numB = b.GetNumber();
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
            Value a = lhs.Execute(vm);
            Value b = rhs.Execute(vm);

            Value.AssertType(Value.ValueType.Number, a,
                "First argument to '*' expected to be 'Number' but was given '{0}'.", a.Type);
            Value.AssertType(Value.ValueType.Number, b,
                "Second argument to '*' expected to be 'Number' but was given '{0}'.", b.Type);

            float numA = a.GetNumber();
            float numB = b.GetNumber();
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
            Value a = lhs.Execute(vm);
            Value b = rhs.Execute(vm);

            Value.AssertType(Value.ValueType.Number, a,
                "First argument to '/' expected to be 'Number' but was given '{0}'.", a.Type);
            Value.AssertType(Value.ValueType.Number, b,
                "Second argument to '/' expected to be 'Number' but was given '{0}'.", b.Type);

            float numA = a.GetNumber();
            float numB = b.GetNumber();
            float numR = numA / numB;

            return new NumberValue(numR);
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
            Value callee = lhs.Execute(vm);
            switch (callee.Type)
            {
                case Value.ValueType.Lambda:
                    return InvokeLambda(vm, callee as LambdaValue);

                default:
                    throw new Exception(string.Format("Cannot invoke something of type '{0}'.", callee.Type));
            }
        }

        private Value InvokeLambda(VM vm, LambdaValue value)
        {
            LambdaExpression lambda = value.GetLambda();
            Block args = rhs as Block;
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

            Value ret = new NilValue();
            foreach (IAST expression in lambda.Body.Expressions)
            {
                try
                {
                    ret = expression.Execute(@new);
                }
                catch (ReturnStatement.Signal sig)
                {
                    ret = sig.Value;
                    break;
                }
            }

            return ret;
        }
    }
}
