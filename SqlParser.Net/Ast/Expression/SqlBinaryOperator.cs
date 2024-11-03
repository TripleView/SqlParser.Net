namespace SqlParser.Net.Ast.Expression;

public class SqlBinaryOperator
{
    public string Name { get; }

    public object Value { get; set; }

    public SqlBinaryOperator(string name, object value)
    {
        Name = name;
        Value = value;
    }

    public static readonly SqlBinaryOperator EqualTo = new SqlBinaryOperator("EqualTo", "=");
    public static readonly SqlBinaryOperator GreaterThen = new SqlBinaryOperator("GreaterThen", ">");
    public static readonly SqlBinaryOperator LessThen = new SqlBinaryOperator("LessThen", "<");
    public static readonly SqlBinaryOperator NotEqualTo = new SqlBinaryOperator("NotEqualTo", "!=");
    public static readonly SqlBinaryOperator GreaterThenOrEqualTo = new SqlBinaryOperator("GreaterThenOrEqualTo", ">=");
    public static readonly SqlBinaryOperator LessThenOrEqualTo = new SqlBinaryOperator("LessThenOrEqualTo", "<=");
    public static readonly SqlBinaryOperator Is = new SqlBinaryOperator("Is", "Is");
    public static readonly SqlBinaryOperator IsNot = new SqlBinaryOperator("IsNot", "Is Not");
    public static readonly SqlBinaryOperator Or = new SqlBinaryOperator("Or", "Or");
    public static readonly SqlBinaryOperator And = new SqlBinaryOperator("And", "And");
    public static readonly SqlBinaryOperator Xor = new SqlBinaryOperator("Xor", "Xor");
    public static readonly SqlBinaryOperator Like = new SqlBinaryOperator("Like", "Like");
    //math
    public static readonly SqlBinaryOperator Add = new SqlBinaryOperator("Add", "+");
    public static readonly SqlBinaryOperator Sub = new SqlBinaryOperator("Sub", "-");
    public static readonly SqlBinaryOperator Multiply = new SqlBinaryOperator("Multiply", "*");
    public static readonly SqlBinaryOperator Divide = new SqlBinaryOperator("Divide", "/");
    public static readonly SqlBinaryOperator Mod = new SqlBinaryOperator("Mod", "%");
    public static readonly SqlBinaryOperator Concat = new SqlBinaryOperator("Concat", "||");
    public static readonly SqlBinaryOperator NotLike = new SqlBinaryOperator("NotLike", "Not Like");
    public static readonly SqlBinaryOperator BitwiseOr = new SqlBinaryOperator("BitwiseOr", "|");
    public static readonly SqlBinaryOperator BitwiseAnd = new SqlBinaryOperator("BitwiseAnd", "&");
    public static readonly SqlBinaryOperator BitwiseXor = new SqlBinaryOperator("BitwiseXor", "^");
    
    protected bool Equals(SqlBinaryOperator other)
    {
        return Name == other.Name && Value.Equals(other.Value);
    }

    public override bool Equals(object? obj)
    {
        if (ReferenceEquals(null, obj)) return false;
        if (ReferenceEquals(this, obj)) return true;
        if (obj.GetType() != this.GetType()) return false;
        return Equals((SqlBinaryOperator)obj);
    }

    public override int GetHashCode()
    {
        unchecked
        {
            return (Name.GetHashCode() * 397) ^ Value.GetHashCode();
        }
    }
}