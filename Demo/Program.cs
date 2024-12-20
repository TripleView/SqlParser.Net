using SqlParser.Net;
using SqlParser.Net.Ast.Visitor;

namespace Demo
{
   
    internal class Program
    {
        static void Main(string[] args)
        {
            var c = "a;;;;".TrimEnd(';');
            var sql = "select * from RouteData with(nolock) where code='abc'";
            var sqlAst = DbUtils.Parse(sql, SqlParser.Net.DbType.SqlServer);
            var unitTestAstVisitor = new UnitTestAstVisitor();
            sqlAst.Accept(unitTestAstVisitor);
            var result = unitTestAstVisitor.GetResult();
        }
    }
}
