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

            sql = @"   SELECT DISTINCT ON (stance)
     *
FROM articles
ORDER BY stance, created_at DESC;";
            sql = @"SELECT
    u.id,
    x.*
FROM users AS u
INNER JOIN LATERAL (
    SELECT *
    FROM orders
    WHERE orders.user_id = u.id
) AS x
ON TRUE;";

            sql = @"SELECT
    c.customer_id,
    c.customer_name,
    latest_order.order_id,
    latest_order.order_no,
    latest_order.amount,
    latest_order.order_time
FROM customers c
CROSS JOIN LATERAL (
    SELECT
        o.order_id,
        o.order_no,
        o.amount,
        o.order_time
    FROM orders o
    WHERE o.customer_id = c.customer_id
      AND o.order_status = 'PAID'
    ORDER BY o.order_time DESC, o.order_id DESC
    LIMIT 1
) AS latest_order
ORDER BY c.customer_id;";

            sql = "select cast('2023-10-15' as TIMESTAMP WITH TIME ZONE)";
            var sqlAst = DbUtils.Parse(sql, DbType.Pgsql);



           
            var result = sqlAst.ToFormat();

            var newSql= sqlAst.ToSql();


        }
    }
}
