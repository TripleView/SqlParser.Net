using SqlParser.Net.Ast.Visitor;

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


}