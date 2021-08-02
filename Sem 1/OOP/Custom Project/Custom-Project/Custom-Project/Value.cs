using System;
using System.Collections.Generic;
using System.Text;

namespace CustomProject
{
    /// <summary>
    /// Represents any possible runtime value that can exist in the language.
    /// </summary>
    public abstract class Value
    {
        /// <summary>
        /// Enumerates every possible type of value in the language.
        /// </summary>
        public enum ValueType
        {
            Unknown = 0,
            
            Nil,
            Boolean,
            Number,
            String,
            Char,
            Lambda,
            List,
            Class,
            Instance,
            Range,
            Native,
        }

        /// <summary>
        /// The <see cref="Value"/>'s <see cref="ValueType"/>.
        /// </summary>
        public ValueType Type { get; protected set; }

        public Value(ValueType type)
        {
            Type = type;
        }

        /// <summary>
        /// Asserts that the given <see cref="Value"/> is the expected <see cref="ValueType"/>.
        /// </summary>
        /// <param name="type">Expected <see cref="ValueType"/>.</param>
        /// <param name="value"><see cref="Value"/> being tested.</param>
        /// <param name="format">Format for the error message.</param>
        /// <param name="args">Arguments to be supplied to the format.</param>
        /// <exception cref="Exception">If types don't match.</exception>
        public static void AssertType(ValueType type, Value value, string format = null, params object[] args)
        {
            if (value.Type != type)
            {
                string message;
                if (format == null)
                {
                    message = string.Format("Expected '{0}' but was given '{1}'", type, value.Type);
                }
                else
                {
                    message = string.Format(format, args);
                }
                throw new Exception(message);
            }
        }

        /// <summary>
        /// Checks that the two given <see cref="Value"/>s are the same <see cref="ValueType"/>.
        /// </summary>
        /// <param name="a">First <see cref="Value"/>.</param>
        /// <param name="b">Second <see cref="Value"/>.</param>
        /// <returns>True if types match. False if not.</returns>
        public static bool TypesMatch(Value a, Value b)
        {
            return a.Type == b.Type;
        }

        /// <summary>
        /// Used to check for equality between two <see cref="Value"/>s.
        /// Does not check that 'other' is the same type as 'this'
        /// </summary>
        /// <param name="other"><see cref="Value"/> to compare against.</param>
        /// <returns>True if equal. False if not.</returns>
        public abstract bool UncheckedEqual(Value other);

        /// <summary>
        /// Checks for equality between two <see cref="Value"/>s.
        /// </summary>
        /// <param name="other"><see cref="Value"/> to compare against.</param>
        /// <returns>True if equal. False if not.</returns>
        public virtual bool Equal(Value other)
        {
            if (!TypesMatch(this, other))
            {
                return false;
            }
            return UncheckedEqual(other);
        }

        /// <summary>
        /// Check if <see cref="Value"/> is a <see cref="NilValue"/>.
        /// </summary>
        /// <returns>True if nil. False if not.</returns>
        public virtual bool IsNil()
        {
            return false;
        }

        /// <summary>
        /// Gets underlying boolean value.
        /// </summary>
        /// <exception cref="Exception">If not a <see cref="BooleanValue"/>.</exception>
        public virtual bool Boolean
        {
            get => throw new Exception("Value is not a boolean.");
        }

        /// <summary>
        /// Gets underlying float value.
        /// </summary>
        /// <exception cref="Exception">If not a <see cref="NumberValue"/>.</exception>
        public virtual float Number
        {
            get => throw new Exception("Value is not a number.");
        }

        /// <summary>
        /// Gets underlying string value.
        /// </summary>
        /// <exception cref="Exception">If not a <see cref="StringValue"/>.</exception>
        public virtual string String
        {
            get => throw new Exception("Value is not a string.");
        }

        /// <summary>
        /// Gets underlying char value.
        /// </summary>
        /// <exception cref="Exception">If not a <see cref="CharValue"/>.</exception>
        public virtual char Char
        {
            get => throw new Exception("Value is not a char.");
        }

        /// <summary>
        /// Gets underlying <see cref="LambdaExpression"/>.
        /// </summary>
        /// <exception cref="Exception">If not a <see cref="LambdaValue"/>.</exception>
        public virtual LambdaExpression Lambda
        {
            get => throw new Exception("Value is not a lambda.");
        }

        /// <summary>
        /// Gets underlying List.
        /// </summary>
        /// <exception cref="Exception">If not a <see cref="ListValue"/>.</exception>
        public virtual List<Value> List
        {
            get => throw new Exception("Value is not a list.");
        }

        /// <summary>
        /// Gets underlying <see cref="ClassValue"/>.
        /// </summary>
        /// <exception cref="Exception">If not a <see cref="ClassValue"/>.</exception>
        public virtual ClassValue Class
        {
            get => throw new Exception("Value is not a class.");
        }

        /// <summary>
        /// Gets underlying <see cref="InstanceValue"/>.
        /// </summary>
        /// <exception cref="Exception">If not a <see cref="InstanceValue"/>.</exception>
        public virtual InstanceValue Instance
        {
            get => throw new Exception("Value is not an instance.");
        }

        /// <summary>
        /// Gets underlying <see cref="RangeValue"/>.
        /// </summary>
        /// <exception cref="Exception">If not a <see cref="RangeValue"/>.</exception>
        public virtual RangeValue Range
        {
            get => throw new Exception("Value is not a class.");
        }
    }

    /// <summary>
    /// Represents 'nil' in the language.
    /// </summary>
    public class NilValue : Value
    {
        public NilValue()
            : base(ValueType.Nil)
        {
        }

        public override bool UncheckedEqual(Value other)
        {
            return other.IsNil();
        }

        public override bool IsNil()
        {
            return true;
        }

        public override string ToString()
        {
            return "nil";
        }
    }

    /// <summary>
    /// Represents any boolean value in the langauge.
    /// </summary>
    public class BooleanValue : Value
    {
        private bool value;

        /// <summary>
        /// Gets underlying boolean value.
        /// </summary>
        public override bool Boolean { get => value; }

        public BooleanValue(bool value)
            : base(ValueType.Boolean)
        {
            this.value = value;
        }

        public override bool UncheckedEqual(Value other)
        {
            return value == other.Boolean;
        }

        public override string ToString()
        {
            return value.ToString().ToLower();
        }
    }

    /// <summary>
    /// Represents a number value in the language.
    /// </summary>
    public class NumberValue : Value
    {
        private float value;

        /// <summary>
        /// Gets underlying float value.
        /// </summary>
        public override float Number { get => value; }

        public NumberValue(float value)
            : base(ValueType.Number)
        {
            this.value = value;
        }

        public override bool UncheckedEqual(Value other)
        {
            return value == other.Number;
        }

        public override string ToString()
        {
            return value.ToString();
        }
    }

    /// <summary>
    /// Represents a string value in the langauge.
    /// </summary>
    public class StringValue : Value
    {
        private string value;

        /// <summary>
        /// Gets underlying string value.
        /// </summary>
        public override string String { get => value; }

        public StringValue(string value)
            : base(ValueType.String)
        {
            this.value = value;
        }

        /// <summary>
        /// Replaces underlying string value.
        /// </summary>
        /// <param name="newValue">The replacement string.</param>
        public void ReplaceString(string newValue)
        {
            value = newValue;
        }

        public override bool UncheckedEqual(Value other)
        {
            return value == other.String;
        }

        public override string ToString()
        {
            return value.ToString();
        }
    }

    /// <summary>
    /// Represents a char value in the language.
    /// </summary>
    public class CharValue : Value
    {
        private char value;

        /// <summary>
        /// Gets underlying char value.
        /// </summary>
        public override char Char { get => value; }

        public CharValue(char value)
            : base(ValueType.Char)
        {
            this.value = value;
        }

        public override bool UncheckedEqual(Value other)
        {
            return value == other.Char;
        }

        public override string ToString()
        {
            return value.ToString();
        }
    }

    /// <summary>
    /// Represents a lambda value in the language.
    /// </summary>
    public class LambdaValue : Value
    {
        private LambdaExpression value;

        /// <summary>
        /// Get underlying <see cref="LambdaExpression"/>.
        /// </summary>
        public override LambdaExpression Lambda { get => value; }

        public LambdaValue(LambdaExpression value)
            : base(ValueType.Lambda)
        {
            this.value = value;
        }

        public override bool UncheckedEqual(Value other)
        {
            return value == other.Lambda;
        }

        public override string ToString()
        {
            StringBuilder builder = new StringBuilder("fn(");
            for (int i = 0; i < value.Args.Count; i++)
            {
                bool vararg = i == value.Args.Count - 1 && value.IsVarargs();
                builder.AppendFormat("{0}{1}", vararg ? "*" : "", value.Args[i]);
                if (i + 1 < value.Args.Count)
                {
                    builder.Append(", ");
                }
            }
            builder.Append(")");
            return builder.ToString();
        }
    }

    /// <summary>
    /// Represents a list value in the language.
    /// </summary>
    public class ListValue : Value
    {
        private List<Value> value;

        /// <summary>
        /// Gets underlying List.
        /// </summary>
        public override List<Value> List { get => value; }

        public ListValue(List<Value> value)
            : base(ValueType.List)
        {
            this.value = value;
        }

        public override bool UncheckedEqual(Value other)
        {
            List<Value> otherList = other.List;
            if (value.Count != otherList.Count)
            {
                return false;
            }
            for (int i = 0; i < value.Count; i++)
            {
                if (!value[i].Equal(otherList[i]))
                {
                    return false;
                }
            }
            return true;
        }

        public override string ToString()
        {
            StringBuilder builder = new StringBuilder("[");
            for (int i = 0; i < value.Count; i++)
            {
                builder.Append(value[i].ToString());
                if (i + 1 < value.Count)
                {
                    builder.Append(", ");
                }
            }
            builder.Append("]");
            return builder.ToString();
        }
    }

    /// <summary>
    /// Represents a class in the language.
    /// </summary>
    public class ClassValue : Value
    {
        public string Name { get; private set; }
        public Dictionary<string, LambdaValue> Methods { get; private set; }
        public Dictionary<string, LambdaValue> ClassMethods { get; private set; }
        public ClassValue SuperClass { get; private set; }

        /// <summary>
        /// Gets underlying <see cref="ClassValue"/>.
        /// </summary>
        /// <remarks>
        /// Used to down cast a <see cref="Value"/> into a <see cref="ClassValue"/>.
        /// </remarks>
        public override ClassValue Class { get => this; }

        public ClassValue(string name, ClassValue superClass)
            : base(ValueType.Class)
        {
            Name = name;
            Methods = new Dictionary<string, LambdaValue>();
            ClassMethods = new Dictionary<string, LambdaValue>();
            SuperClass = superClass;
        }

        public override bool UncheckedEqual(Value other)
        {
            return this == other;
        }

        public override string ToString()
        {
            StringBuilder builder = new StringBuilder();
            builder.AppendFormat("{0}", Name);

            if (SuperClass != null)
            {
                builder.AppendFormat("({0})", SuperClass.Name);
            }

            builder.Append(" {\n");

            foreach (var binding in Methods)
            {
                builder.AppendFormat("  {0}: {1}\n", binding.Key, binding.Value);
            }

            foreach (var binding in ClassMethods)
            {
                builder.AppendFormat("  class.{0}: {1}\n", binding.Key, binding.Value);
            }

            builder.Append("}");

            return builder.ToString();
        }
    }

    /// <summary>
    /// Represents an instance of a class in the language.
    /// </summary>
    public class InstanceValue : Value
    {
        private ClassValue @class;
        public Dictionary<string, Value> Fields { get; private set; }

        /// <summary>
        /// Gets the instance's <see cref="ClassValue"/>.
        /// </summary>
        public override ClassValue Class { get => @class; }

        /// <summary>
        /// Gets the underlying <see cref="InstanceValue"/>.
        /// </summary>
        /// <remarks>
        /// Used to down cast a <see cref="Value"/> to a <see cref="InstanceValue"/>.
        /// </remarks>
        public override InstanceValue Instance { get => this; }

        public InstanceValue(ClassValue @class)
            : base(ValueType.Instance)
        {
            this.@class = @class;
            Fields = new Dictionary<string, Value>();
        }

        /// <summary>
        /// Casts the instance to its super class and returns its actual class.
        /// </summary>
        /// <returns>The original class of the instance.</returns>
        public ClassValue UpCast()
        {
            var classValue = @class;
            @class = @class.SuperClass;
            return classValue;
        }

        /// <summary>
        /// Casts an instance to a given class.
        /// </summary>
        /// <param name="classValue">Class to cast the instance to.</param>
        public void Cast(ClassValue classValue)
        {
            @class = classValue;
        }

        public override bool UncheckedEqual(Value other)
        {
            if (!@class.UncheckedEqual(other.Class))
            {
                return false;
            }

            var myFields = Fields.Values.GetEnumerator();
            var theirFields = other.Instance.Fields.Values.GetEnumerator();

            while (myFields.MoveNext() && theirFields.MoveNext())
            {
                var mine = myFields.Current;
                var theirs = theirFields.Current;

                if (!mine.Equal(theirs))
                {
                    return false;
                }
            }

            return true;
        }

        public override string ToString()
        {
            StringBuilder builder = new StringBuilder();
            builder.AppendFormat("{0}(", Class.Name);

            var fields = Fields.GetEnumerator();
            for (int i = 0; i < Fields.Count; i++)
            {
                fields.MoveNext();
                builder.AppendFormat("{0}: {1}", fields.Current.Key, fields.Current.Value);
                if (i + 1 < Fields.Count)
                {
                    builder.Append(", ");
                }
            }

            builder.Append(")");
            return builder.ToString();
        }
    }

    /// <summary>
    /// Represents a range value in the language.
    /// </summary>
    /// <remarks>
    /// Ranges are mainly used in for-loops.
    /// </remarks>
    public class RangeValue : Value
    {
        public Value Start { get; private set; }
        public Value End { get; private set; }
        public bool Inclusive { get; private set; }

        /// <summary>
        /// Gets underlying <see cref="RangeValue"/>.
        /// </summary>
        public override RangeValue Range { get => this; }

        public RangeValue(Value start, Value end, bool inclusive)
            : base(ValueType.Range)
        {
            Start = start;
            End = end;
            Inclusive = inclusive;
        }

        public override bool UncheckedEqual(Value other)
        {
            return Start.Equal(other.Range.Start)
                && End.Equal(other.Range.End)
                && Inclusive == other.Range.Inclusive;
        }

        public override string ToString()
        {
            string op = Inclusive ? "..=" : "..";
            return string.Format("{0}{1}{2}", Start, op, End);
        }
    }
}
