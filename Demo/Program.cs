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
            sql = "WITH older_users AS (\r\n    SELECT id FROM users WHERE age > 25\r\n)\r\nUPDATE users\r\nSET age = age + 1\r\nFROM users\r\nJOIN older_users ON users.id = older_users.id;";
            sql =
                @"UPDATE u
SET u.name = o.name
FROM users AS u
JOIN orders AS o ON o.user_id = u.id
WHERE o.id = 100;";
            var sqlAst = DbUtils.Parse(sql, DbType.SqlServer);
           
            var result = sqlAst.ToFormat();

            var newSql= sqlAst.ToSql();


        }
    }
}
