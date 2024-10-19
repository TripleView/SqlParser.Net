using SqlParser.Net.Ast.Expression;
using SqlParser.Net.Ast.Visitor;
using Xunit.Abstractions;
using Xunit.Sdk;

namespace SqlParser.Net.Test;

public class UnitTestAstVisitorTest
{
    [Fact]
    public void TestSelectAll()
    {
        var sql = "select * from RouteData";
        var sqlAst = new SqlExpression();
        sqlAst = DbUtils.Parse(sql, DbType.SqlServer);
        var unitTestAstVisitor = new UnitTestAstVisitor();
        sqlAst.Accept(unitTestAstVisitor);
        var result = unitTestAstVisitor.GetAst();
        var expect = new SqlSelectExpression()
        {
            Query = new SqlSelectQueryExpression()
            {
                Columns = new List<SqlSelectItemExpression>()
                {
                    new SqlSelectItemExpression()
                    {
                        Body = new SqlAllColumnExpression()
                    }
                },
                From = new SqlTableExpression()
                {
                    Name = new SqlIdentifierExpression()
                    {
                        Value = "RouteData"
                    }
                }
            }
        };
        var sqlSelect = new SqlSelectExpression()
        {
            Query = new SqlSelectQueryExpression()
            {
                Columns = new List<SqlSelectItemExpression>()
                {
                    new SqlSelectItemExpression()
                    {
                        Body = new SqlAllColumnExpression()
                    },
                },
                From = new SqlTableExpression()
                {
                    Name = new SqlIdentifierExpression()
                    {
                        Value = "RouteData"
                    }
                }
            }
        };

        Assert.True(sqlSelect.Equals(expect));
        Assert.True(sqlAst.Equals(expect));
    }


}