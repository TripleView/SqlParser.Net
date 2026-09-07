using System;

namespace SqlParser.Net.Utils;

internal static class StringUtils
{
    /// <summary>
    /// Case-Insensitive String Comparison;忽略大小写比较字符串
    /// </summary>
    /// <param name="first"></param>
    /// <param name="second"></param>
    /// <returns></returns>
    public static bool IgnoreCaseEquals(this string first, string second)
    {
        return string.Equals(first, second, StringComparison.OrdinalIgnoreCase);
    }
}