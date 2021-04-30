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
            List
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

        public virtual bool GetBoolean()
        {
            throw new Exception("Value is not a boolean.");
        }

        public virtual void SetBoolean(bool value)
        {
            throw new Exception("Value is not a boolean.");
        }

        public virtual float GetNumber()
        {
            throw new Exception("Value is not a number.");
        }

        public virtual void SetNumber(float value)
        {
            throw new Exception("Value is not a number.");
        }

        public virtual string GetString()
        {
            throw new Exception("Value is not a string.");
        }

        public virtual void SetString(string value)
        {
            throw new Exception("Value is not a string.");
        }

        public virtual char GetChar()
        {
            throw new Exception("Value is not a char.");
        }

        public virtual void SetChar(char value)
        {
            throw new Exception("Value is not a char.");
        }

        public virtual LambdaExpression GetLambda()
        {
            throw new Exception("Value is not a lambda.");
        }

        public virtual void SetLambda(LambdaExpression value)
        {
            throw new Exception("Value is not a lambda.");
        }

        public virtual List<Value> GetList()
        {
            throw new Exception("Value is not a list.");
        }

        public virtual void SetList(List<Value> value)
        {
            throw new Exception("Value is not a list.");
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

        public BooleanValue(bool value)
            : base(ValueType.Boolean)
        {
            this.value = value;
        }

        public override bool UncheckedEqual(Value other)
        {
            return value == other.GetBoolean();
        }

        public override bool GetBoolean()
        {
            return value;
        }

        public override void SetBoolean(bool value)
        {
            this.value = value;
        }

        public override string ToString()
        {
            return value.ToString().ToLower();
        }
    }

    public class NumberValue : Value
    {
        private float value;

        public NumberValue(float value)
            : base(ValueType.Number)
        {
            this.value = value;
        }

        public override bool UncheckedEqual(Value other)
        {
            return value == other.GetNumber();
        }

        public override float GetNumber()
        {
            return value;
        }

        public override void SetNumber(float value)
        {
            this.value = value;
        }

        public override string ToString()
        {
            return value.ToString();
        }
    }

    public class StringValue : Value
    {
        private string value;

        public StringValue(string value)
            : base(ValueType.String)
        {
            this.value = value;
        }

        public override bool UncheckedEqual(Value other)
        {
            return value == other.GetString();
        }

        public override string GetString()
        {
            return value;
        }

        public override void SetString(string value)
        {
            this.value = value;
        }

        public override string ToString()
        {
            return value.ToString();
        }
    }

    public class CharValue : Value
    {
        private char value;

        public CharValue(char value)
            : base(ValueType.Char)
        {
            this.value = value;
        }

        public override bool UncheckedEqual(Value other)
        {
            return value == other.GetChar();
        }

        public override char GetChar()
        {
            return value;
        }

        public override void SetChar(char value)
        {
            this.value = value;
        }

        public override string ToString()
        {
            return value.ToString();
        }
    }

    public class LambdaValue : Value
    {
        private LambdaExpression value;

        public LambdaValue(LambdaExpression value)
            : base(ValueType.Lambda)
        {
            this.value = value;
        }

        public override bool UncheckedEqual(Value other)
        {
            return value == other.GetLambda();
        }

        public override LambdaExpression GetLambda()
        {
            return value;
        }

        public override void SetLambda(LambdaExpression value)
        {
            this.value = value;
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

        public ListValue(List<Value> value)
            : base(ValueType.List)
        {
            this.value = value;
        }

        public override bool UncheckedEqual(Value other)
        {
            List<Value> otherList = other.GetList();
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

        public override List<Value> GetList()
        {
            return value;
        }

        public override void SetList(List<Value> value)
        {
            this.value = value;
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
}
