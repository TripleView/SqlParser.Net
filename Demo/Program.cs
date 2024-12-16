using SqlParser.Net;
using SqlParser.Net.Ast.Visitor;

namespace Demo
{
   
    internal class Program
    {
        static void Main(string[] args)
        {
            var sql = "SELECT TOP 100 * FROM [sys].[objects] ORDER BY [object_id] DESC";
            var sqlAst = DbUtils.Parse(sql, SqlParser.Net.DbType.SqlServer);
            var unitTestAstVisitor = new UnitTestAstVisitor();
            sqlAst.Accept(unitTestAstVisitor);
            var result = unitTestAstVisitor.GetResult();
        }
    }
}
