using System;
namespace CustomProject
{
    public abstract class Unary : IAST
    {
        protected IAST expr;

        public Unary(IAST expr)
        {
            this.expr = expr;
        }

        public abstract Value Execute(VM vm);
    }

    public class Negation : Unary
    {
        public Negation(IAST expr)
            : base(expr)
        {
        }

        public override Value Execute(VM vm)
        {
            Value a = expr.Execute(vm);

            Value.AssertType(Value.ValueType.Number, a,
                "Argument to unary '-' expected to be 'Number' but was given '{0}'.", a.Type);

            float numA = a.GetNumber();
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
            if (expr == null)
            {
                throw new Signal(new NilValue());
            }
            throw new Signal(expr.Execute(vm));
        }
    }
}
