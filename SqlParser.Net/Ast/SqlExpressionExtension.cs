using SqlParser.Net.Ast.Expression;
using System.Collections.Generic;
using System.Linq;

namespace SqlParser.Net.Ast;

/// <summary>
/// sql expression 扩展方法
/// SQL Expression Extension Method
/// </summary>
public static class SqlExpressionExtension
{
    public static T Clone<T>(this T t) where T : SqlExpression
    {
        if (t is null)
        {
            return t;
        }

        return (T)t.InternalClone();
    }

    /// <summary>
    /// Determines whether a set has a value (that is, at least one element);
    /// 判断集合是否有值（即至少有一个元素）
    /// </summary>
    public static bool HasValue<T>(this IEnumerable<T> source)
    {
        // 先判空，再判断是否有元素
        return source != null && source.Any();
    }
}