using System;
using System.Collections.Generic;
using System.Text;

namespace CustomProject
{
    public abstract class Value
    {
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
        }

        public ValueType Type { get; protected set; }

        public Value(ValueType type)
        {
            Type = type;
        }

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

        public static bool TypesMatch(Value a, Value b)
        {
            return a.Type == b.Type;
        }

        public abstract bool UncheckedEqual(Value other);

        public virtual bool Equal(Value other)
        {
            if (!TypesMatch(this, other))
            {
                return false;
            }
            return UncheckedEqual(other);
        }

        public virtual bool IsNil()
        {
            return false;
        }

        public virtual bool Boolean
        {
            get => throw new Exception("Value is not a boolean.");
        }

        public virtual float Number
        {
            get => throw new Exception("Value is not a number.");
        }

        public virtual string String
        {
            get => throw new Exception("Value is not a string.");
        }

        public virtual char Char
        {
            get => throw new Exception("Value is not a char.");
        }

        public virtual LambdaExpression Lambda
        {
            get => throw new Exception("Value is not a lambda.");
        }

        public virtual List<Value> List
        {
            get => throw new Exception("Value is not a list.");
        }

        public virtual ClassValue Class
        {
            get => throw new Exception("Value is not a class.");
        }

        public virtual InstanceValue Instance
        {
            get => throw new Exception("Value is not a class.");
        }
    }

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

    public class BooleanValue : Value
    {
        private bool value;
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

    public class NumberValue : Value
    {
        private float value;
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

    public class StringValue : Value
    {
        private string value;
        public override string String { get => value; }

        public StringValue(string value)
            : base(ValueType.String)
        {
            this.value = value;
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

    public class CharValue : Value
    {
        private char value;
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

    public class LambdaValue : Value
    {
        private LambdaExpression value;
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
                builder.Append(value.Args[i]);
                if (i + 1 < value.Args.Count)
                {
                    builder.Append(", ");
                }
            }
            builder.Append(")");
            return builder.ToString();
        }
    }

    public class ListValue : Value
    {
        private List<Value> value;
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

    public class ClassValue : Value
    {
        public string Name { get; private set; }
        public Dictionary<string, LambdaValue> Methods { get; private set; }

        public override ClassValue Class { get => this; }

        public ClassValue(string name)
            : base(ValueType.Class)
        {
            Name = name;
            Methods = new Dictionary<string, LambdaValue>();
        }

        public override bool UncheckedEqual(Value other)
        {
            return this == other;
        }

        public override string ToString()
        {
            StringBuilder builder = new StringBuilder();
            builder.AppendFormat("{0} {{\n", Name);

            foreach (var binding in Methods)
            {
                builder.AppendFormat("  {0}: {1}\n", binding.Key, binding.Value);
            }

            builder.Append("}");

            return builder.ToString();
        }
    }

    public class InstanceValue : Value
    {
        private ClassValue @class;
        public override ClassValue Class { get => @class; }
        public Dictionary<string, Value> Fields { get; private set; }
        public override InstanceValue Instance { get => this; }

        public InstanceValue(ClassValue @class)
            : base(ValueType.Instance)
        {
            this.@class = @class;
            Fields = new Dictionary<string, Value>();
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
}
