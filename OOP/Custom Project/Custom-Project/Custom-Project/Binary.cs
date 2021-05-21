using System;
using System.Collections.Generic;
using System.Text;

namespace CustomProject
{
    /// <summary>
    /// Represents any binary operation in the AST.
    /// </summary>
    public abstract class Binary : IAST
    {
        /// <summary>
        /// The left hand side sub-expression of the binary operator.
        /// </summary>
        public IAST Lhs { get; private set; }

        /// <summary>
        /// The right hand side sub-expression of the binary operator.
        /// </summary>
        public IAST Rhs { get; private set; }

        public Binary(IAST lhs, IAST rhs)
        {
            Lhs = lhs;
            Rhs = rhs;
        }

        public abstract Value Execute(VM vm);
    }

    /// <summary>
    /// Represents an equality operation in the AST.
    /// </summary>
    public class Equality : Binary
    {
        public Equality(IAST lhs, IAST rhs)
            : base(lhs, rhs)
        {
        }

        /// <summary>
        /// Executes sub-expressions and checks resulting <see cref="Value"/>s for equality.
        /// </summary>
        /// <param name="vm">The environment in which to execute the operation.</param>
        /// <returns>A <see cref="BooleanValue"/>. True if equal. False if not.</returns>
        public override Value Execute(VM vm)
        {
            Value a = Lhs.Execute(vm);
            Value b = Rhs.Execute(vm);
            bool equal = a.Equal(b);
            return new BooleanValue(equal);
        }
    }

    /// <summary>
    /// Represents a numeric addition operation in the AST.
    /// </summary>
    public class Addition : Binary
    {
        public Addition(IAST lhs, IAST rhs)
            : base(lhs, rhs)
        {
        }

        /// <summary>
        /// Executes sub-expressions and returns the sum of the resulting <see cref="NumberValue"/>s.
        /// </summary>
        /// <param name="vm">The environment in which to execute the operation.</param>
        /// <returns>A <see cref="NumberValue"/>. The sum of the sub-expressions.</returns>
        /// <exception cref="Exception">If either sub-expression doesn't return a <see cref="NumberValue"/>.</exception>
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

    /// <summary>
    /// Represents a numeric subtraction operation in the AST.
    /// </summary>
    public class Subtraction : Binary
    {
        public Subtraction(IAST lhs, IAST rhs)
            : base(lhs, rhs)
        {
        }

        /// <summary>
        /// Executes sub-expressions and returns the difference between the resulting <see cref="NumberValue"/>s.
        /// </summary>
        /// <param name="vm">The environment in which to execute the operation.</param>
        /// <returns>A <see cref="NumberValue"/>. The difference between the sub-expressions.</returns>
        /// <exception cref="Exception">If either sub-expression doesn't return a <see cref="NumberValue"/>.</exception>
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

    /// <summary>
    /// Represents a numeric multiplication operation in the AST.
    /// </summary>
    public class Multiplication : Binary
    {
        public Multiplication(IAST lhs, IAST rhs)
            : base(lhs, rhs)
        {
        }

        /// <summary>
        /// Executes sub-expressions and returns the product of the resulting <see cref="NumberValue"/>s.
        /// </summary>
        /// <param name="vm">The environment in which to execute the operation.</param>
        /// <returns>A <see cref="NumberValue"/>. The product of the sub-expressions.</returns>
        /// <exception cref="Exception">If either sub-expression doesn't return a <see cref="NumberValue"/>.</exception>
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

    /// <summary>
    /// Represents a numeric division operation in the AST.
    /// </summary>
    public class Division : Binary
    {
        public Division(IAST lhs, IAST rhs)
            : base(lhs, rhs)
        {
        }

        /// <summary>
        /// Executes sub-expressions and returns the quotient of the resulting <see cref="NumberValue"/>s.
        /// </summary>
        /// <param name="vm">The environment in which to execute the operation.</param>
        /// <returns>A <see cref="NumberValue"/>. The quotient of the sub-expressions.</returns>
        /// <exception cref="Exception">If either sub-expression doesn't return a <see cref="NumberValue"/>.</exception>
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

    /// <summary>
    /// Represents a boolean or operation in the AST.
    /// </summary>
    public class Or : Binary
    {
        public Or(IAST lhs, IAST rhs)
            : base(lhs, rhs)
        {
        }

        /// <summary>
        /// Evaluates Lhs or Rhs with short-circuiting.
        /// </summary>
        /// <param name="vm">The environment in which to execute the operation.</param>
        /// <returns>A <see cref="BooleanValue"/>. True if either sub-expression is true. False if not.</returns>
        /// <exception cref="Exception">If either sub-experssion doesn't return a <see cref="BooleanValue"/>.</exception>
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

    /// <summary>
    /// Represents a boolean and operation in the AST.
    /// </summary>
    public class And : Binary
    {
        public And(IAST lhs, IAST rhs)
            : base(lhs, rhs)
        {
        }

        /// <summary>
        /// Evaluates Lhs and Rhs with short-circuiting.
        /// </summary>
        /// <param name="vm">The environment in which to execute the operation.</param>
        /// <returns>A <see cref="BooleanValue"/>. True if both sub-expression are true. False if not.</returns>
        /// <exception cref="Exception">if either sub-expression doesn't return a <see cref="BooleanValue"/>.</exception>
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

    /// <summary>
    /// Represents a numeric less-than operation in the AST.
    /// </summary>
    public class LessThan : Binary
    {
        public LessThan(IAST lhs, IAST rhs)
            : base(lhs, rhs)
        {
        }

        /// <summary>
        /// Evaluates Lhs less-than Rhs and returns the result.
        /// </summary>
        /// <param name="vm">The environment in which to execute the operation.</param>
        /// <returns>A <see cref="BooleanValue"/>. True if Lhs less-than Rhs. False if not.</returns>
        /// <exception cref="Exception">If either sub-expression doesn't return a <see cref="NumberValue"/>.</exception>
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

    /// <summary>
    /// Represents a numeric greater-than operation in the AST.
    /// </summary>
    public class GreaterThan : Binary
    {
        public GreaterThan(IAST lhs, IAST rhs)
            : base(lhs, rhs)
        {
        }

        /// <summary>
        /// Evaluates Lhs > Rhs and returns the result.
        /// </summary>
        /// <param name="vm">The environment in which to execute the operation.</param>
        /// <returns>A <see cref="BooleanValue"/>. True if Lhs > Rhs. False if not.</returns>
        /// <exception cref="Exception">If either sub-expression doesn't return a <see cref="NumberValue"/>.</exception>
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

    /// <summary>
    /// Represents a List subscript operation in the AST.
    /// </summary>
    public class Subscript : Binary
    {
        public Subscript(IAST lhs, IAST rhs)
            : base(lhs, rhs)
        {
        }

        /// <summary>
        /// Returns the <see cref="Value"/> in the list gotten by Lhs at the index gotten by Rhs.
        /// </summary>
        /// <param name="vm">The environment in which to execute the operation.</param>
        /// <returns>The element in the list.</returns>
        /// <exception cref="Exception">If there was a type error.</exception>
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

    /// <summary>
    /// Represents an invocation in the AST.
    /// </summary>
    public class Invocation : Binary
    {
        public Invocation(IAST lhs, IAST rhs)
            : base(lhs, rhs)
        {
        }

        /// <summary>
        /// Invokes a value and returns the resulting <see cref="Value"/>.
        /// </summary>
        /// <param name="vm">The environment in which to execute the invocation.</param>
        /// <returns>A <see cref="BooleanValue"/>. True if Lhs less-than Rhs. False if not.</returns>
        /// <exception cref="Exception">If Lhs isn't invocable.</exception>
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

        /// <summary>
        /// Calls a lambda with a given <see cref="VM"/>.
        /// </summary>
        /// <param name="lambda">The lambda to invoke.</param>
        /// <param name="vm">The VM to invoke the lambda with.</param>
        /// <returns>The lambda's return value.</returns>
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

        /// <summary>
        /// Executes argument expressions of the invocation and puts them into '@new's Variables dictionary.
        /// </summary>
        /// <param name="vm">The environment in which to execute the arguments.</param>
        /// <param name="new">Where to put the arguments.</param>
        /// <param name="lambda">Lambda that will be invoked.</param>
        /// <param name="args">Arguments being passed to the lambda.</param>
        /// <exception cref="Exception">If there is an argument mismatch.</exception>
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

        /// <summary>
        /// Invokes a given lambda.
        /// </summary>
        /// <param name="vm">The environment in which to invoke the lambda.</param>
        /// <param name="value">The lambda to invoke.</param>
        /// <returns>The lambda's return value.</returns>
        /// <exception cref="Exception">If Rhs doesn't return a <see cref="Block"/>.</exception>
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
            SetArguments(vm, @new, lambda, args.Nodes);
            return CallWith(lambda, @new);
        }

        /// <summary>
        /// Invokes a given class.
        /// </summary>
        /// <param name="vm">The environment in which to invoke the class.</param>
        /// <param name="value">The class to invoke.</param>
        /// <returns>A new <see cref="InstanceValue"/> of the given class.</returns>
        /// <exception cref="Exception">
        /// If Rhs doesn't return a <see cref="Block"/> or
        /// if 'init()' doesn't return a <see cref="NilValue"/>.
        /// </exception>
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
                SetArguments(vm, @new, init, args.Nodes);

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

        /// <summary>
        /// Invokes a method with a receiver as an implicit first argument.
        /// </summary>
        /// <param name="vm">The environment in which to invoke the method.</param>
        /// <param name="boundMethod">The bound method with the method's name and the receiver.</param>
        /// <returns>The method's return value.</returns>
        /// <exception cref="Exception">If type error.</exception>
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

        /// <summary>
        /// Invokes a method with an <see cref="InstanceValue"/> as the receiver.
        /// </summary>
        /// <param name="vm">The environment in which to invoke the method.</param>
        /// <param name="receiver">The receiver of the method.</param>
        /// <param name="methodName">The name of the method.</param>
        /// <returns>The method's return value.</returns>
        /// <exception cref="Exception">
        /// If the methodName isn't a method of the class or
        /// if Rhs isn't a <see cref="Block"/>.
        /// </exception>
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
            SetArguments(vm, @new, method, args.Nodes);
            return CallWith(method, @new);
        }

        /// <summary>
        /// Invokes a method with a <see cref="ListValue"/> as the receiver.
        /// </summary>
        /// <param name="vm">The environment in which to invoke the method.</param>
        /// <param name="list">The receiver of the method.</param>
        /// <param name="methodName">The name of the method.</param>
        /// <returns>A <see cref="NilValue"/>.</returns>
        /// <exception cref="Exception">
        /// If the methodName isn't a method of the class or
        /// if Rhs isn't a <see cref="Block"/> or
        /// if there was an argument mismatch.
        /// </exception>
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

                        if (args.Nodes.Count != 1)
                        {
                            throw new Exception(string.Format("Argument Mistmatch! List.add takes 1 argument(s) but was given {0}.", args.Nodes.Count));
                        }

                        Value value = args.Nodes[0].Execute(vm);
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

                        if (args.Nodes.Count != 2)
                        {
                            throw new Exception(string.Format("Argument Mistmatch! List.push takes 2 argument(s) but was given {0}.", args.Nodes.Count));
                        }

                        Value idx = args.Nodes[0].Execute(vm);
                        Value.AssertType(Value.ValueType.Number, idx,
                            "Type mismatch! List.remove expects first argument to be 'Number' but was given '{0}'.", idx.Type);

                        Value value = args.Nodes[1].Execute(vm);

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

                        if (args.Nodes.Count != 1)
                        {
                            throw new Exception(string.Format("Argument Mistmatch! List.push takes 1 argument(s) but was given {0}.", args.Nodes.Count));
                        }

                        Value value = args.Nodes[0].Execute(vm);
                        return new NumberValue(list.FindIndex(v => v.Equal(value)));
                    }
                case "remove":
                    {
                        Block args = Rhs as Block;
                        if (args == null)
                        {
                            throw new Exception("Internal: 'rhs' of 'Invocation' was not a 'Block'.");
                        }

                        if (args.Nodes.Count != 1)
                        {
                            throw new Exception(string.Format("Argument Mistmatch! List.push takes 1 argument(s) but was given {0}.", args.Nodes.Count));
                        }

                        Value value = args.Nodes[0].Execute(vm);
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

        /// <summary>
        /// Invokes a method with a <see cref="StringValue"/> as the receiver.
        /// </summary>
        /// <param name="vm">The environment in which to invoke the method.</param>
        /// <param name="str">The receiver of the method.</param>
        /// <param name="methodName">The name of the method.</param>
        /// <returns>A <see cref="NilValue"/>.</returns>
        /// <exception cref="Exception">
        /// If the methodName isn't a method of the class or
        /// if Rhs isn't a <see cref="Block"/>.
        /// </exception>
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
                        foreach (IAST expr in args.Nodes)
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

        /// <summary>
        /// Invokes a class method that doesn't take an instance as a receiver.
        /// </summary>
        /// <param name="vm">The environment in which to invoke the method.</param>
        /// <param name="value">The class the method belongs to.</param>
        /// <param name="methodName">The name of the method.</param>
        /// <returns>The method's return value.</returns>
        /// <exception cref="Exception">
        /// If the methodName isn't a method of the class or
        /// if Rhs isn't a <see cref="Block"/>.
        /// </exception>
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
            SetArguments(vm, @new, method, args.Nodes);
            return CallWith(method, @new);
        }
    }

    /// <summary>
    /// Represents a range expression in the AST.
    /// </summary>
    public class RangeExpression : Binary
    {
        bool inclusive;

        public RangeExpression(IAST lhs, IAST rhs, bool inclusive)
            : base(lhs, rhs)
        {
            this.inclusive = inclusive;
        }

        /// <summary>
        /// Executes sub-expressions and returns a <see cref="RangeValue"/>.
        /// </summary>
        /// <param name="vm">The environment in which to execute the sub-expressions.</param>
        /// <returns>A <see cref="RangeValue"/>. A range between Lhs and Rhs.</returns>
        /// <exception cref="Exception">If type error.</exception>
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
