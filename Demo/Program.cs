using SqlParser.Net;
using SqlParser.Net.Ast.Visitor;

namespace Demo
{
   
    internal class Program
    {
        static void Main(string[] args)
        {
            var c = "a;;;;".TrimEnd(';');
            var sql = "SELECT CONVERT(DATETIME2(0), '2022-03-27T01:01:00', 126)";
            var sqlAst = DbUtils.Parse(sql, SqlParser.Net.DbType.SqlServer);
            var unitTestAstVisitor = new UnitTestAstVisitor();
            sqlAst.Accept(unitTestAstVisitor);
            var result = unitTestAstVisitor.GetResult();
        }
    }
}
