using System;
namespace CustomProject
{
    /// <summary>
    /// Represents any unary operation in the AST.
    /// </summary>
    public abstract class Unary : IAST
    {
        /// <summary>
        /// The sub-expression of the <see cref="Unary"/> node.
        /// </summary>
        public IAST Expr { get; private set; }

        public Unary(IAST expr)
        {
            Expr = expr;
        }

        public abstract Value Execute(VM vm);
    }

    /// <summary>
    /// Represents a boolean not operation in the AST.
    /// </summary>
    public class Not : Unary
    {
        public Not(IAST expr)
            : base(expr)
        {
        }

        /// <summary>
        /// Executes sub-expression and returns its value notted.
        /// </summary>
        /// <param name="vm">The environment in which to execute the operation.</param>
        /// <returns>A <see cref="BooleanValue"/>.</returns>
        /// <exception cref="Exception">If sub-expression doesn't return a <see cref="BooleanValue"/>.</exception>
        public override Value Execute(VM vm)
        {
            Value a = Expr.Execute(vm);

            Value.AssertType(Value.ValueType.Boolean, a,
                "Operand of '!' expected to be 'Boolean' but was given '{0}'.", a.Type);

            return new BooleanValue(!a.Boolean);
        }
    }

    /// <summary>
    /// Represents a numeric negation operation in the AST.
    /// </summary>
    public class Negation : Unary
    {
        public Negation(IAST expr)
            : base(expr)
        {
        }

        /// <summary>
        /// Executes sub-expression and returns its negation.
        /// </summary>
        /// <param name="vm">The environment in which to execute the operation.</param>
        /// <returns>A <see cref="NumberValue"/>.</returns>
        /// <exception cref="Exception">If sub-expression doesn't return a <see cref="NumberValue"/>.</exception>
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

    /// <summary>
    /// Represents a return statement in the AST.
    /// </summary>
    public class ReturnStatement : Unary
    {
        public ReturnStatement(IAST expr)
            : base(expr)
        {
        }

        /// <summary>
        /// Signal used to notify parent nodes that a return statement has been executed.
        /// </summary>
        public class Signal : Exception
        {
            /// <summary>
            /// The <see cref="CustomProject.Value"/> being returned.
            /// </summary>
            public Value Value { get; private set; }

            public Signal(Value value)
            {
                Value = value;
            }
        }

        /// <summary>
        /// Throws a <see cref="Signal"/> with wrapped return <see cref="CustomProject.Value"/>.
        /// </summary>
        /// <param name="vm">The environment in which to execute the return statements sub-expression..</param>
        /// <returns></returns>
        public override Value Execute(VM vm)
        {
            if (Expr == null)
            {
                throw new Signal(new NilValue());
            }
            throw new Signal(Expr.Execute(vm));
        }
    }

    /// <summary>
    /// Represents a print statement in the AST.
    /// </summary>
    public class PrintStatement : Unary
    {
        public PrintStatement(IAST expr)
            : base(expr)
        {
        }

        /// <summary>
        /// Prints a <see cref="Value"/>'s string representation to the console.
        /// </summary>
        /// <param name="vm">The environment in which to execute the print statement.</param>
        /// <returns>A <see cref="NilValue"/>.</returns>
        public override Value Execute(VM vm)
        {
            Value value = Expr.Execute(vm);
            Console.WriteLine(value);
            return new NilValue();
        }
    }
}
