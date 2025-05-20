using SqlParser.Net;
using SqlParser.Net.Ast.Expression;
using SqlParser.Net.Ast.Visitor;

namespace Demo
{
    internal class Program
    {
        static void Main(string[] args)
        {
     
            var sql = "select TIMESTAMP '2023-01-01'::VARCHAR(10) ";
            sql = "select * from test3 t where t.a not ilike '%a%'";

            sql = "SELECT 5 # 3";
       
            var sqlAst = DbUtils.Parse(sql, DbType.Pgsql);
            var result = sqlAst.ToFormat();
            var newSql= sqlAst.ToSql();
        }
    }
}
