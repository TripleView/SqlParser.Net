using SqlParser.Net;
using SqlParser.Net.Ast.Visitor;

namespace Demo
{
   
    internal class Program
    {
        static void Main(string[] args)
        {
            var sql = "select to_char(NAME) from TEST5";
            var sqlAst = DbUtils.Parse(sql, DbType.Oracle);
            var unitTestAstVisitor = new UnitTestAstVisitor();
            sqlAst.Accept(unitTestAstVisitor);
            var result = unitTestAstVisitor.GetResult();
        }
    }
}
