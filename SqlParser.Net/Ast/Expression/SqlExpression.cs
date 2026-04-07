using System;
using System.Collections.Generic;
using SqlParser.Net.Ast.Visitor;

namespace SqlParser.Net.Ast.Expression;

public class SqlExpression : IAcceptVisitor, ICloneableExpression
{
    public virtual SqlExpression Accept(IAstVisitor visitor, VisitContext context = null)
    {
        return this;
    }

    public Guid Id { get; private set; }

    public virtual SqlExpressionType Type { get; protected set; }

    /// <summary>
    /// Database Type
    /// 数据库类型
    /// </summary>
    public DbType? DbType { set; get; }

    public SqlExpression()
    {
        this.Id = Guid.NewGuid();
    }
    /// <summary>
    /// Compare 2 SqlExpressions for equality
    /// 比较2个SqlExpression是否相等
    /// </summary>
    /// <param name="exp1"></param>
    /// <param name="exp2"></param>
    /// <returns></returns>
    protected bool CompareTwoSqlExpression<T>(T exp1, T exp2) where T : class
    {
        if (exp1 == null ^ exp2 == null)
        {
            return false;
        }

        if (exp1 != null && exp2 != null)
        {
            return exp1.Equals(exp2);
        }

        return true;
    }

    /// <summary>
    /// Compare 2 SqlExpression lists for equality
    /// 比较2个SqlExpression列表是否相等
    /// </summary>
    /// <param name="exp1"></param>
    /// <param name="exp2"></param>
    /// <returns></returns>
    protected bool CompareTwoSqlExpressionList<T>(List<T> exp1, List<T> exp2) where T : class
    {
        if (exp1 == null && exp2 is { Count: 0 } || (exp2 == null && exp1 is { Count: 0 }))
        {
            return true;
        }
        if (exp1 == null ^ exp2 == null)
        {
            return false;
        }

        if (exp1 != null && exp2 != null)
        {
            if (exp1.Count != exp2.Count)
            {
                return false;
            }
            for (var i = 0; i < exp1.Count; i++)
            {
                var item = exp1[i];
                var item2 = exp2[i];
                if (!item.Equals(item2))
                {
                    return false;
                }
            }
        }

        return true;
    }

    public string ToSql(DbType? dbType = null)
    {
        var myDbType = dbType ?? DbType;
        if (myDbType == null)
        {
            throw new Exception("dbType can not be null");
        }
        var sqlGenerationAstVisitor = new SqlGenerationAstVisitor(myDbType.Value);
        this.Accept(sqlGenerationAstVisitor);
        var generationSql = sqlGenerationAstVisitor.GetResult();
        return generationSql;
    }

    public override string ToString()
    {
        if (DbType is null)
        {
            return "";
        }
        return base.ToString() + ";sql:" + ToSql();
    }

    /// <summary>
    /// Display the formatted abstract syntax tree
    /// 显示格式化后的抽象语法树
    /// </summary>
    /// <returns></returns>
    public string ToFormat()
    {
        var unitTestAstVisitor = new UnitTestAstVisitor();
        this.Accept(unitTestAstVisitor);
        var result = unitTestAstVisitor.GetResult();
        return result;
    }

    public virtual SqlExpression InternalClone()
    {
        return new SqlExpression();
    }
}