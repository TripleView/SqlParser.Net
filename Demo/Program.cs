using SqlParser.Net;
using SqlParser.Net.Ast.Expression;
using SqlParser.Net.Ast.Visitor;

namespace Demo
{
   
    internal class Program
    {
        static void Main(string[] args)
        {
            var sql = @"SELECT EXTRACT(DAY FROM c.order_date) as b from orders c";
            
            var sqlAst = DbUtils.Parse(sql, DbType.MySql);
            var result = sqlAst.ToFormat();
            var newSql= sqlAst.ToSql();
        }
    }
}
