using SqlParser.Net;
using SqlParser.Net.Ast.Expression;
using SqlParser.Net.Ast.Visitor;

namespace Demo
{
    internal class Program
    {
        static void Main(string[] args)
        {
     
            var sql = "select * from test3 where a regexp 'a' COLLATE utf8mb4_general_ci";

            var sqlAst = DbUtils.Parse(sql, DbType.MySql);
            var result = sqlAst.ToFormat();
            var newSql= sqlAst.ToSql();
        }
    }
}
