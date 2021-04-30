using System;
namespace CustomProject
{
    public abstract class Unary : IAST
    {
        public IAST Expr { get; private set; }

        public Unary(IAST expr)
        {
            Expr = expr;
        }

        public abstract Value Execute(VM vm);
    }

    public class Not : Unary
    {
        public Not(IAST expr)
            : base(expr)
        {
        }

        public override Value Execute(VM vm)
        {
            Value a = Expr.Execute(vm);

            Value.AssertType(Value.ValueType.Boolean, a,
                "Operand of '!' expected to be 'Boolean' but was given '{0}'.", a.Type);

            return new BooleanValue(!a.Boolean);
        }
    }

    public class Negation : Unary
    {
        public Negation(IAST expr)
            : base(expr)
        {
        }

        public override Value Execute(VM vm)
        {
            Value a = Expr.Execute(vm);

            Value.AssertType(Value.ValueType.Number, a,
                "Argument to unary '-' expected to be 'Number' but was given '{0}'.", a.Type);

            float numA = a.Number;
            float numR = -numA;

            return new NumberValue(numR);
        }
    }

    public class ReturnStatement : Unary
    {
        public ReturnStatement(IAST expr)
            : base(expr)
        {
        }

        public class Signal : Exception
        {
            public Value Value { get; private set; }

            public Signal(Value value)
            {
                Value = value;
            }
        }

        public override Value Execute(VM vm)
        {
            if (Expr == null)
            {
                throw new Signal(new NilValue());
            }
            throw new Signal(Expr.Execute(vm));
        }
    }

    public class PrintStatement : Unary
    {
        public PrintStatement(IAST expr)
            : base(expr)
        {
        }

        public override Value Execute(VM vm)
        {
            Value value = Expr.Execute(vm);
            Console.WriteLine(value);
            return new NilValue();
        }
    }
}
