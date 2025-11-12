using SqlParser.Net.Ast.Visitor;
using System.Collections.Generic;
using System.Linq;
using System.Xml.Linq;

namespace SqlParser.Net.Ast.Expression;

public class SqlInsertExpression : SqlExpression
{
    private SqlExpression table;
    private List<SqlExpression> columns;
    private List<List<SqlExpression>> valuesList;
    private SqlSelectExpression fromSelect;
    private SqlReturningExpression returning;
    private List<SqlWithSubQueryExpression> withSubQuerys;
    public override SqlExpression Accept(IAstVisitor visitor)
    {
        return visitor.VisitSqlInsertExpression(this);
    }
    public SqlInsertExpression()
    {
        this.Type = SqlExpressionType.Insert;
        this.Columns = new List<SqlExpression>();
        this.ValuesList = new List<List<SqlExpression>>();
        this.WithSubQuerys = new List<SqlWithSubQueryExpression>();
    }

    /// <summary>
    /// cte sub query
    /// cte公共表表达式子查询
    /// </summary>
    public List<SqlWithSubQueryExpression> WithSubQuerys
    {
        get => withSubQuerys;
        set
        {
            if (value != null)
            {
                foreach (var expression in value)
                {
                    if (expression != null)
                    {
                        expression.Parent = this;
                    }
                }
            }
            withSubQuerys = value;
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
    public SqlReturningExpression Returning
    {
        get => returning;
        set
        {
            if (value != null)
            {
                value.Parent = this;
            }
            returning = value;
        }
    }
    public List<SqlExpression> Columns
    {
        get => columns;
        set
        {
            if (value != null)
            {
                foreach (var expression in value)
                {
                    if (expression != null)
                    {
                        expression.Parent = this;
                    }
                }
            }
            columns = value;
        }
    }

    /// <summary>
    /// Since MySQL, SQL Server, SQLite, and PostgreSQL support inserting multiple rows of data in an insert statement, the values ??value is a list.
    /// 由于mysql，sqlserver,SQLite,PostgreSQL支持在insert语句中插入多行数据，所以values值是列表
    /// </summary>
    public List<List<SqlExpression>> ValuesList
    {
        get => valuesList;
        set
        {
            if (value != null)
            {
                foreach (var expressions in value)
                {
                    if (expressions != null)
                    {
                        foreach (var expression in expressions)
                        {
                            if (expression != null)
                            {
                                expression.Parent = this;
                            }
                        }
                    }
                }
            }

            valuesList = value;
        }
    }

    /// <summary>
    /// INSERT INTO TEST2(name) SELECT name AS name2 FROM TEST t
    /// </summary>
    public SqlSelectExpression FromSelect
    {
        get => fromSelect;
        set
        {
            if (value != null)
            {
                value.Parent = this;
            }
            fromSelect = value;
        }
    }

    public List<string> Comments { get; set; }

    protected bool Equals(SqlInsertExpression other)
    {
        if (!CompareTwoSqlExpressionList(WithSubQuerys, other.WithSubQuerys))
        {
            return false;
        }

        if (!CompareTwoSqlExpressionList(Columns, other.Columns))
        {
            return false;
        }
        if (!CompareTwoSqlExpression(Table, other.Table))
        {
            return false;
        }

        if (!CompareTwoSqlExpression(FromSelect, other.FromSelect))
        {
            return false;
        }

        if (!CompareTwoSqlExpression(Returning, other.Returning))
        {
            return false;
        }

        if (!(ValuesList is null && other.ValuesList is { Count: 0 } ||
            (other.ValuesList is null && ValuesList is { Count: 0 })))
        {
            if (ValuesList is null ^ other.ValuesList is null)
            {
                return false;
            }
            else if (ValuesList != null && other.ValuesList != null)
            {
                if (ValuesList.Count != other.ValuesList.Count)
                {
                    return false;
                }

                for (var i = 0; i < ValuesList.Count; i++)
                {
                    var items1 = ValuesList[i];
                    var items2 = other.ValuesList[i];
                    if (items1 == null ^ items2 == null)
                    {
                        return false;
                    }

                    if (items1 != null && items2 != null)
                    {
                        for (int j = 0; j < items1.Count; j++)
                        {
                            var item = items1[i];
                            var item2 = items2[i];
                            if (!item.Equals(item2))
                            {
                                return false;
                            }
                        }
                    }
                }
            }
        }


        return true;
    }

    public override bool Equals(object? obj)
    {
        if (ReferenceEquals(null, obj)) return false;
        if (ReferenceEquals(this, obj)) return true;
        if (obj.GetType() != this.GetType()) return false;
        return Equals((SqlInsertExpression)obj);
    }

    public override int GetHashCode()
    {
        unchecked
        {
            var hashCode = Table.GetHashCode();
            hashCode = (hashCode * 397) ^ Columns.GetHashCode();
            hashCode = (hashCode * 397) ^ ValuesList.GetHashCode();
            hashCode = (hashCode * 397) ^ FromSelect.GetHashCode();
            return hashCode;
        }
    }

    public override SqlExpression InternalClone()
    {
        var result = new SqlInsertExpression()
        {
            DbType = this.DbType,
            Columns = this.Columns.Select(x => x.Clone()).ToList(),
            WithSubQuerys = this.WithSubQuerys.Select(x => x.Clone()).ToList(),
            Table = this.Table.Clone(),
            FromSelect = this.FromSelect.Clone(),
            ValuesList = this.ValuesList.Select(x => x.Select(y => y.Clone()).ToList()).ToList(),
            Returning = this.Returning.Clone(),
        };
        return result;
    }
}