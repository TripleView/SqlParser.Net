using SqlParser.Net;
using SqlParser.Net.Ast.Expression;
using SqlParser.Net.Ast.Visitor;

namespace Demo
{
    internal class Program
    {
        static void Main(string[] args)
        {
            var sql = "select * from test3 t where t.a ='a' or t.b ='2' and t.c ='3'";
            //sql = "select '2'::int as b";
            //sql = "select '2'::int b";
            //sql = "SELECT 5 # 3";
            //sql = "select * from test3 t where not t.a ='a'  and t.c ='3'";
            //sql = "select * from test3 t where not (t.a ='a'  and t.c ='3')";
            //sql = "select * from test3 t where  t.a ='a'  and not t.c ='3'";
            var sqlAst = DbUtils.Parse(sql, DbType.Pgsql);
            var result = sqlAst.ToFormat();
            var newSql= sqlAst.ToSql();
        }
    }
}
