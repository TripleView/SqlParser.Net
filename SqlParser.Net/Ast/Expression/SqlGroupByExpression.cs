﻿using SqlParser.Net.Ast.Visitor;
using System.Collections.Generic;

namespace SqlParser.Net.Ast.Expression;

public class SqlGroupByExpression : SqlExpression
{
    public override void Accept(IAstVisitor visitor)
    {
        visitor.VisitSqlGroupByExpression(this);
    }
    public SqlGroupByExpression()
    {
        this.Type = SqlExpressionType.GroupBy;
    }

    public List<SqlExpression> Items { get; set; }

    public SqlExpression Having { get; set; }

    protected bool Equals(SqlGroupByExpression other)
    {
        if (Items.Count != other.Items.Count)
        {
            return false;
        }
        for (var i = 0; i < Items.Count; i++)
        {
            var item = Items[i];
            var item2 = other.Items[i];
            if (!item.Equals(item2))
            {
                return false;
            }
        }

        if (Having == null ^ other.Having == null)
        {
            return false;
        }
        else if (Having != null && other.Having != null)
        {
            return Having.Equals(other.Having);
        }
        return true;
    }

    public override bool Equals(object? obj)
    {
        if (ReferenceEquals(null, obj)) return false;
        if (ReferenceEquals(this, obj)) return true;
        if (obj.GetType() != this.GetType()) return false;
        return Equals((SqlGroupByExpression)obj);
    }

    public override int GetHashCode()
    {
        unchecked
        {
            return (Items.GetHashCode() * 397) ^ Having.GetHashCode();
        }
    }
}

