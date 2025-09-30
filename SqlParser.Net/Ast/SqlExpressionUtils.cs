using SqlParser.Net.Ast.Expression;

namespace SqlParser.Net.Ast;

public static class SqlExpressionUtils
{
    public static bool HasValue(this SqlOrderByExpression sqlOrderByExpression)
    {
        return sqlOrderByExpression != null && sqlOrderByExpression.Items != null &&
               sqlOrderByExpression.Items.Count > 0;
    }

    public static bool HasValue(this SqlGroupByExpression sqlGroupByExpression)
    {
        return sqlGroupByExpression != null && sqlGroupByExpression.Items != null &&
               sqlGroupByExpression.Items.Count > 0;
    }

    public static bool HasValue(this SqlReturningExpression sqlReturningExpression)
    {
        return sqlReturningExpression != null && sqlReturningExpression.Items != null &&
               sqlReturningExpression.Items.Count > 0;
    }

    public static bool HasValue(this SqlArrayExpression sqlArrayExpression)
    {
        return sqlArrayExpression != null && sqlArrayExpression.Items != null &&
               sqlArrayExpression.Items.Count > 0;
    }
}