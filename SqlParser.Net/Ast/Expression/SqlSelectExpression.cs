using SqlParser.Net.Ast.Visitor;
using System.Collections.Generic;

namespace SqlParser.Net.Ast.Expression;

public class SqlSelectExpression : SqlExpression, ILateralExpression,IAliasExpression
{
    private SqlExpression query;
    private SqlIdentifierExpression alias;
    private SqlOrderByExpression orderBy;
    private SqlLimitExpression limit;
    /// <summary>
    /// The LATERAL Modifier in PostgreSQL
    /// pgsql÷–µƒlateral–ﬁ Œ
    /// </summary>
    public bool? IsLateral { set; get; }
    public override SqlExpression Accept(IAstVisitor visitor, VisitContext context = null)
    {
        return visitor.VisitSqlSelectExpression(this, context);
    }
    public SqlSelectExpression()
    {
        this.Type = SqlExpressionType.Select;
    }

    public SqlExpression Query
    {
        get => query;
        set
        {
            query = value;
        }
    }

    public SqlIdentifierExpression Alias
    {
        get => alias;
        set
        {
            alias = value;
        }
    }


    public SqlOrderByExpression OrderBy
    {
        get => orderBy;
        set
        {
            orderBy = value;
        }
    }

    public SqlLimitExpression Limit
    {
        get => limit;
        set
        {
            limit = value;
        }
    }

    public List<string> Comments { get; set; }

    protected bool Equals(SqlSelectExpression other)
    {
        if (IsLateral != other.IsLateral)
        {
            return false;
        }
        if (!CompareTwoSqlExpression(Limit, other.Limit))
        {
            return false;
        }

        if (!CompareTwoSqlExpression(OrderBy, other.OrderBy))
        {
            return false;
        }

        if (!CompareTwoSqlExpression(Alias, other.Alias))
        {
            return false;
        }

        if (!CompareTwoSqlExpression(Query, other.Query))
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
        return Equals((SqlSelectExpression)obj);
    }

    public override int GetHashCode()
    {
        return Query.GetHashCode();
    }

    public override SqlExpression InternalClone()
    {
        var result = new SqlSelectExpression()
        {
            DbType = this.DbType,
            Limit = this.Limit.Clone(),
            OrderBy = this.OrderBy.Clone(),
            Alias = this.Alias.Clone(),
            Query = this.Query.Clone(),
            IsLateral = this.IsLateral
        };
        return result;
    }
}