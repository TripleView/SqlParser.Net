using SqlParser.Net;
using SqlParser.Net.Ast.Expression;
using SqlParser.Net.Ast.Visitor;

namespace Demo
{
   
    internal class Program
    {
        static void Main(string[] args)
        {
            var ffdsfsf = "".Split(',').Where(it=>!string.IsNullOrWhiteSpace(it)).ToList();
            var sql = @"select city ,a,b from Address ";
            var sqlAst = DbUtils.Parse(sql, DbType.SqlServer);
            if (sqlAst is SqlSelectExpression select && select.Query is SqlSelectQueryExpression ssQuery)
            {
                foreach (var selectItemExpression in ssQuery.Columns)
                {
                   var c=  selectItemExpression.ToSql(DbType.SqlServer);
                }
            }
            var unitTestAstVisitor = new UnitTestAstVisitor();
            sqlAst.Accept(unitTestAstVisitor);
            var result = unitTestAstVisitor.GetResult();
        }
    }
}
