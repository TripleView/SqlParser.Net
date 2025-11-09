using SqlParser.Net.Ast.Visitor;
using SqlParser.Net.Lexer;

namespace SqlParser.Net.Ast.Expression;
/// <summary>
/// The collate clause is mainly used to specify string comparison and sorting rules.
/// collate子句主要用于指定字符串比较和排序的规则
/// </summary>
public class SqlCollateExpression : SqlExpression
{
    private SqlExpression body;
    public SqlCollateExpression()
    {
        this.Type = SqlExpressionType.Collate;
    }

    public SqlExpression Body
    {
        get => body;
        set
        {
            if (value != null)
            {
                value.Parent = this;
            }
            body = value;
        }
    }

    protected bool Equals(SqlCollateExpression other)
    {
        return Body.Equals(other.Body);
    }

    public override bool Equals(object? obj)
    {
        if (ReferenceEquals(null, obj)) return false;
        if (ReferenceEquals(this, obj)) return true;
        if (obj.GetType() != this.GetType()) return false;
        return Equals((SqlCollateExpression)obj);
    }

    public override int GetHashCode()
    {
        return Body.GetHashCode();
    }
    public override SqlExpression Accept(IAstVisitor visitor)
    {
        return visitor.VisitSqlCollateExpression(this);
    }

    public override SqlExpression InternalClone()
    {
        var result = new SqlCollateExpression()
        {
            DbType = this.DbType,
            Body = this.Body.Clone(),
        };
        return result;
    }
}