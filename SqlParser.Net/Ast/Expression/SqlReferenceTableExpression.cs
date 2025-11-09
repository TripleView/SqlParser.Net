using SqlParser.Net.Ast.Visitor;

namespace SqlParser.Net.Ast.Expression;

public class SqlReferenceTableExpression : SqlExpression, IAliasExpression
{
    private SqlFunctionCallExpression functionCall;
    private SqlIdentifierExpression alias;
    public override SqlExpression Accept(IAstVisitor visitor)
    {
        return visitor.VisitSqlReferenceTableExpression(this);
    }
    public SqlReferenceTableExpression()
    {
        this.Type = SqlExpressionType.ReferenceTable;
    }

    public SqlFunctionCallExpression FunctionCall
    {
        get => functionCall;
        set
        {
            if (value != null)
            {
                value.Parent = this;
            }
            functionCall = value;
        }
    }

    public SqlIdentifierExpression Alias
    {
        get => alias;
        set
        {
            if (value != null)
            {
                value.Parent = this;
            }
            alias = value;
        }
    }

    protected bool Equals(SqlReferenceTableExpression other)
    {
        if (!CompareTwoSqlExpression(Alias, other.Alias))
        {
            return false;
        }

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
    public override SqlExpression InternalClone()
    {
        var result = new SqlReferenceTableExpression()
        {
            DbType = this.DbType,
            FunctionCall = this.FunctionCall.Clone(),
            Alias = this.Alias.Clone(),
        };
        return result;
    }
}