using SqlParser.Net.Ast.Visitor;

namespace SqlParser.Net.Ast.Expression;

public class SqlReferenceTableExpression : SqlExpression
{
    public override void Accept(IAstVisitor visitor)
    {
        visitor.VisitSqlReferenceTableExpression(this);
    }
    public SqlReferenceTableExpression()
    {
        this.Type = SqlExpressionType.ReferenceTable;
    }

    public SqlFunctionCallExpression FunctionCall { get; set; }

    protected bool Equals(SqlReferenceTableExpression other)
    {
        if (!CompareTwoSqlExpression(FunctionCall, other.FunctionCall))
        {
            return false;
        }
        return true;
    }

    public override bool Equals(object? obj)
    {
        if (ReferenceEquals(null, obj)) return false;
        if (ReferenceEquals(this, obj)) return true;
        if (obj.GetType() != this.GetType()) return false;
        return Equals((SqlReferenceTableExpression)obj);
    }

    public override int GetHashCode()
    {
        throw new System.NotImplementedException();
    }
}