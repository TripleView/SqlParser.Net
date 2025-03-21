using SqlParser.Net.Ast.Expression;
using SqlParser.Net.Ast.Visitor;
using System.Xml.Linq;
using Xunit.Sdk;

namespace SqlParser.Net.Test;

public class DeleteTest
{
    [Fact]
    public void TestDelete()
    {
        var sql = "delete from test where name=4";
        var sqlAst = DbUtils.Parse(sql, DbType.MySql);
        var unitTestAstVisitor = new UnitTestAstVisitor();
        sqlAst.Accept(unitTestAstVisitor);
        var result = unitTestAstVisitor.GetResult();

        var expect = new SqlDeleteExpression()
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
                Right = new SqlNumberExpression()
                {
                    Value = 4M
                },
            },
        };


        Assert.True(sqlAst.Equals(expect));
    }
    [Fact]
    public void TestDeleteCheckIfParsingIsComplete()
    {
        var sql = "delete from RouteData wher code='abc'";
        var sqlAst = new SqlExpression();
        Assert.Throws<SqlParsingErrorException>(() =>
        {
            var t = TimeUtils.TestMicrosecond((() => { sqlAst = DbUtils.Parse(sql, DbType.SqlServer); }));
        });
    }
    [Fact]
    public void TestDeleteWithoutFromKeywordInSqlServer()
    {
        var sql = "delete address where id=1";
        var sqlAst = DbUtils.Parse(sql, DbType.SqlServer);
        var unitTestAstVisitor = new UnitTestAstVisitor();
        sqlAst.Accept(unitTestAstVisitor);
        var result = unitTestAstVisitor.GetResult();

        var expect = new SqlDeleteExpression()
        {
            Table = new SqlTableExpression()
            {
                Name = new SqlIdentifierExpression()
                {
                    Value = "address",
                },
            },
            Where = new SqlBinaryExpression()
            {
                Left = new SqlIdentifierExpression()
                {
                    Value = "id",
                },
                Operator = SqlBinaryOperator.EqualTo,
                Right = new SqlNumberExpression()
                {
                    Value = 1M,
                },
            },
        };

        Assert.True(sqlAst.Equals(expect));
    }
}