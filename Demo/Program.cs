using SqlParser.Net;
using SqlParser.Net.Ast.Visitor;

namespace Demo
{
   
    internal class Program
    {
        static void Main(string[] args)
        {
            var sql = @"select city '表' from Address ";
            sql = @"select city '表1' from Address 表";
            var sqlAst = DbUtils.Parse(sql, DbType.SqlServer);
            var unitTestAstVisitor = new UnitTestAstVisitor();
            sqlAst.Accept(unitTestAstVisitor);
            var result = unitTestAstVisitor.GetResult();
        }
    }
}
