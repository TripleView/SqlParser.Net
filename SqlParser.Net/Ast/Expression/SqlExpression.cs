using System;
using System.Collections.Generic;
using SqlParser.Net.Ast.Visitor;

namespace SqlParser.Net.Ast.Expression;

public class SqlExpression : IAcceptVisitor
{
    public virtual void Accept(IAstVisitor visitor)
    {
        
    }
    public virtual SqlExpressionType Type { get; protected set; }
    /// <summary>
    /// Parent Expression
    /// 父级表达式
    /// </summary>
    public SqlExpression Parent { get; set; }
    /// <summary>
    /// Database Type
    /// 数据库类型
    /// </summary>
    public DbType? DbType { set; get; }
    
    public SqlExpression()
    {

    }
    /// <summary>
    /// Compare 2 SqlExpressions for equality
    /// 比较2个SqlExpression是否相等
    /// </summary>
    /// <param name="exp1"></param>
    /// <param name="exp2"></param>
    /// <returns></returns>
    protected bool CompareTwoSqlExpression<T>(T exp1,T exp2) where T:class
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

    public string ToSql()
    {
        
        if (DbType == null)
        {
            throw new Exception("dbType can not be null");
        }
        var sqlGenerationAstVisitor = new SqlGenerationAstVisitor(DbType.Value);
        this.Accept(sqlGenerationAstVisitor);
        var generationSql = sqlGenerationAstVisitor.GetResult();
        return generationSql;
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
}