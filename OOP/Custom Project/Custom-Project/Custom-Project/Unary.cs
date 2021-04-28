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
}
