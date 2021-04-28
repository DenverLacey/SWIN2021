using System;
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
}
