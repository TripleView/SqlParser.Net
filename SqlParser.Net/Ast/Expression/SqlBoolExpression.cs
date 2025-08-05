using SqlParser.Net.Ast.Visitor;
using SqlParser.Net.Lexer;

namespace SqlParser.Net.Ast.Expression;

public class SqlBoolExpression : SqlExpression
{
    public override void Accept(IAstVisitor visitor)
    {
        visitor.VisitSqlBoolExpression(this);
    }
    public SqlBoolExpression()
    {
        this.Type = SqlExpressionType.Bool;
    }

    public SqlBoolExpressionTokenContext TokenContext { get; set; } 

    public bool Value { get; set; }

    protected bool Equals(SqlBoolExpression other)
    {
        return Value == other.Value;
    }

    public override bool Equals(object? obj)
    {
        if (ReferenceEquals(null, obj)) return false;
        if (ReferenceEquals(this, obj)) return true;
        if (obj.GetType() != this.GetType()) return false;
        return Equals((SqlBoolExpression)obj);
    }

    public override int GetHashCode()
    {
        throw new System.NotImplementedException();
    }

    public override SqlExpression Clone()
    {
        var result = new SqlBoolExpression()
        {
            DbType = this.DbType,
            Value = Value
        };
        return result;
    }
}

public class SqlBoolExpressionTokenContext
{
    public Token? Bool { get; set; }
}