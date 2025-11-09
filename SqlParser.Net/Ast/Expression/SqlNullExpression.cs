using SqlParser.Net.Ast.Visitor;

namespace SqlParser.Net.Ast.Expression;

public class SqlNullExpression : SqlExpression
{
    public override SqlExpression Accept(IAstVisitor visitor)
    {
        return visitor.VisitSqlNullExpression(this);
    }
    public SqlNullExpression()
    {
        this.Type = SqlExpressionType.Null;
    }

    protected bool Equals(SqlNullExpression other)
    {
        return true;
    }

    public override bool Equals(object? obj)
    {
        if (ReferenceEquals(null, obj)) return false;
        if (ReferenceEquals(this, obj)) return true;
        if (obj.GetType() != this.GetType()) return false;
        return Equals((SqlNullExpression)obj);
    }

    public override int GetHashCode()
    {
        throw new System.NotImplementedException();
    }
    public override SqlExpression InternalClone()
    {
        var result = new SqlNullExpression()
        {
            DbType = this.DbType,
        };
        return result;
    }
}