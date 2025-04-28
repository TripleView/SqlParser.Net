using SqlParser.Net.Ast.Expression;
using SqlParser.Net.Ast.Visitor;
using Xunit.Abstractions;

namespace SqlParser.Net.Test;

public class UpdateTest
{
    [Fact]
    public void TestUpdate()
    {
        var d = true || false && false;
        var sql = "update test set name ='4',d='2024-11-22 08:19:47.243' where name ='1'";
        var sqlAst = DbUtils.Parse(sql, DbType.MySql);
      
        var result = sqlAst.ToFormat();
        var expect = new SqlUpdateExpression()
        {
            Table = new SqlTableExpression()
            {
                Name = new SqlIdentifierExpression()
                {
                    Value = "test"
                },
            },
            Where = new SqlBinaryExpression()
            {
                Left = new SqlIdentifierExpression()
                {
                    Value = "name"
                },
                Operator = SqlBinaryOperator.EqualTo,
                Right = new SqlStringExpression()
                {
                    Value = "1"
                },
            },
            Items = new List<SqlExpression>()
            {
                new SqlBinaryExpression()
                {
                    Left = new SqlIdentifierExpression()
                    {
                        Value = "name"
                    },
                    Operator = SqlBinaryOperator.EqualTo,
                    Right = new SqlStringExpression()
                    {
                        Value = "4"
                    },
                },
                new SqlBinaryExpression()
                {
                    Left = new SqlIdentifierExpression()
                    {
                        Value = "d"
                    },
                    Operator = SqlBinaryOperator.EqualTo,
                    Right = new SqlStringExpression()
                    {
                        Value = "2024-11-22 08:19:47.243"
                    },
                },
            },
        };
        
        Assert.True(sqlAst.Equals(expect));
        var newSql = sqlAst.ToSql();
        Assert.Equal("update test set name = '4',d = '2024-11-22 08:19:47.243' where(name = '1')", newSql);
    }

    [Fact]
    public void TestUpdate2()
    {
        var sql = "update test set name =4 where name =1";
        var sqlAst = DbUtils.Parse(sql, DbType.MySql);
        var result = sqlAst.ToFormat();
        var expect = new SqlUpdateExpression()
        {
            Table = new SqlTableExpression()
            {
                Name = new SqlIdentifierExpression()
                {
                    Value = "test",
                },
            },
            Where = new SqlBinaryExpression()
            {
                Left = new SqlIdentifierExpression()
                {
                    Value = "name",
                },
                Operator = SqlBinaryOperator.EqualTo,
                Right = new SqlNumberExpression()
                {
                    Value = 1M,
                },
            },
            Items = new List<SqlExpression>()
            {
                new SqlBinaryExpression()
                {
                    Left = new SqlIdentifierExpression()
                    {
                        Value = "name",
                    },
                    Operator = SqlBinaryOperator.EqualTo,
                    Right = new SqlNumberExpression()
                    {
                        Value = 4M,
                    },
                },
            },
        };


        var newSql = sqlAst.ToSql();
        Assert.Equal("update test set name = 4 where(name = 1)", newSql);
    }

    [Fact]
    public void TestUpdateCheckIfParsingIsComplete()
    {
        var sql = "update test set name =4 wher name =1";
        var sqlAst = new SqlExpression();
        Assert.Throws<SqlParsingErrorException>(() =>
        {
            sqlAst = DbUtils.Parse(sql, DbType.SqlServer);
        });
    }

}