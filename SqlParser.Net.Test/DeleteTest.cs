using SqlParser.Net.Ast.Expression;
using System.Xml.Linq;

namespace SqlParser.Net.Test;

public class DeleteTest
{
    [Fact]
    public void TestDelete()
    {
        var sql = "delete from test where name=4";
        var sqlAst = DbUtils.Parse(sql, DbType.MySql);
        var expect = new SqlDeleteExpression()
        {
            Table = new SqlTableExpression()
            {
                Name = new SqlIdentifierExpression()
                {
                    Value = "test"
                }
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
                    Value = 4
                }
            }
        };

        Assert.True(sqlAst.Equals(expect));
    }


}