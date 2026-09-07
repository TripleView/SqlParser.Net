using System;
using System.Collections.Generic;

namespace SqlParser.Net.Ast.AnalyzeContext;
/// <summary>
/// Ordinary analysis of context
/// 普通分析上下文
/// </summary>
public class CommonContext
{
    /// <summary>
    /// The number of dimensions in an array in pgsql
    /// 在pgsql的array内的层数
    /// </summary>
    public int IsInPgSqlArrayIndex { get; set; }
    /// <summary>
    /// Determine Whether It Is Within an insert Statement
    /// 判断是否在insert语句内
    /// </summary>
    public bool IsInInsert { get; set; }
}