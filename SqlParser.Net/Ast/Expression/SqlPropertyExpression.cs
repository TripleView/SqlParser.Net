using SqlParser.Net.Ast.Visitor;

namespace SqlParser.Net.Ast.Expression;

public class SqlPropertyExpression : SqlExpression, ICollateExpression
{
    private SqlIdentifierExpression name;
    private SqlExpression table;
    /// <summary>
    /// The collate clause is mainly used to specify string comparison and sorting rules.
    /// collate子句主要用于指定字符串比较和排序的规则
    /// </summary>

    private SqlCollateExpression collate;

    public override void Accept(IAstVisitor visitor)
    {
        visitor.VisitSqlPropertyExpression(this);
    }
    public SqlPropertyExpression()
    {
        this.Type = SqlExpressionType.Property;
    }

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
    /// <summary>
    /// property name
    /// 属性名称
    /// </summary>
    public SqlIdentifierExpression Name
    {
        get => name;
        set
        {
            if (value != null)
            {
                value.Parent = this;
            }
            name = value;
        }
    }

    public SqlExpression Table
    {
        get => table;
        set
        {
            if (value != null)
            {
                value.Parent = this;
            }
            table = value;
        }
    }

    protected bool Equals(SqlPropertyExpression other)
    {
        if (!CompareTwoSqlExpression(Name, other.Name))
        {
            return false;
        }

        if (!CompareTwoSqlExpression(Table, other.Table))
        {
            return false;
        }

        if (!CompareTwoSqlExpression(Collate, other.Collate))
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
        return Equals((SqlPropertyExpression)obj);
    }

    public override int GetHashCode()
    {
        unchecked
        {
            return (Name.GetHashCode() * 397) ^ Table.GetHashCode();
        }
    }

    public override SqlExpression InternalClone()
    {
        var result = new SqlPropertyExpression()
        {
            DbType = this.DbType,
            Name = this.Name.Clone(),
            Table= this.Table.Clone(),
            Collate= this.Collate.Clone(),
        };
        return result;
    }
}