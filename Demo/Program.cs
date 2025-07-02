using SqlParser.Net;
using SqlParser.Net.Ast.Expression;
using SqlParser.Net.Ast.Visitor;

namespace Demo
{
    internal class Program
    {
        static void Main(string[] args)
        {
            var sql = "SELECT ARRAY(SELECT generate_series(1, 10))";
            
            sql = "select * from test where :f is null";
            sql = "select * from test3 c WHERE a='a' AND CASE WHEN to_char(c.b) ='b' THEN TO_NUMBER( c.c ) END >=240";
            //SELECT ARRAY(SELECT generate_series(1, 10));
            //SELECT ARRAY[[1, 2], [3, 4]] AS matrix;

            //SELECT ARRAY[[[1, 2], [3, 4]], [[5, 6], [7, 8]]];

            //SELECT* FROM unnest(ARRAY[10, 20, 30])


            //SELECT ARRAY(SELECT a FROM test3 t);

            //SELECT ARRAY[1, 2, 3] AS int_array;
            //sql = "SELECT * FROM unnest(ARRAY[10, 20, 30])";
            sql = "SELECT __t.* FROM test3 __t ";
            var sqlAst = DbUtils.Parse(sql, DbType.MySql);
            
            var result = sqlAst.ToFormat();
            var newSql= sqlAst.ToSql();


        }
    }
}
