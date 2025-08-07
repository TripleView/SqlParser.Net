using SqlParser.Net.Ast.Visitor;

namespace SqlParser.Net.Ast.Expression;

public class SqlVariableExpression : SqlExpression, ICollateExpression
{
    /// <summary>
    /// The collate clause is mainly used to specify string comparison and sorting rules.
    /// collate子句主要用于指定字符串比较和排序的规则
    /// </summary>

    private SqlCollateExpression collate;
    public override void Accept(IAstVisitor visitor)
    {
        visitor.VisitSqlVariableExpression(this);
    }
    public SqlVariableExpression()
    {
        this.Type = SqlExpressionType.Variable;
    }
    /// <summary>
    /// Prefix eg. : or @
    /// 前缀，例如:或者@
    /// </summary>
    public string Prefix { get; set; }

    public string Name { get; set; }
    /// <summary>
    /// The collate clause is mainly used to specify string comparison and sorting rules.
    /// collate子句主要用于指定字符串比较和排序的规则
    /// </summary>
    public SqlCollateExpression Collate
    {
        get => collate;
        set
        {
            if (value != null)
            {
                value.Parent = this;
            }
            collate = value;
        }
    }
    protected bool Equals(SqlVariableExpression other)
    {
        if (!CompareTwoSqlExpression(Collate, other.Collate))
        {
            return false;
        }
        return Prefix == other.Prefix && Name == other.Name;
    }

    public override bool Equals(object? obj)
    {
        if (ReferenceEquals(null, obj)) return false;
        if (ReferenceEquals(this, obj)) return true;
        if (obj.GetType() != this.GetType()) return false;
        return Equals((SqlVariableExpression)obj);
    }

    public override int GetHashCode()
    {
        unchecked
        {
            return (Prefix.GetHashCode() * 397) ^ Name.GetHashCode();
        }
    }

    public override SqlExpression InternalClone()
    {
        var result = new SqlVariableExpression()
        {
            DbType = this.DbType,
            Collate = this.Collate.Clone(),
            Prefix = Prefix,
            Name = Name
        };
        return result;
    }
}