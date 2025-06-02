﻿namespace SqlParser.Net.Ast.Expression;

public enum SqlExpressionType
{
    Select,
    SelectItem,
    SelectQuery,
    Table,
    Where,
    Condition,
    Property,
    Identifier,
    AllColumn,
    Constant,
    JoinTable,
    ReferenceTable,
    PivotTable,
    OrderBy,
    OrderByItem,
    Binary,
    Number,
    String,
    FunctionCall,
    Null,
    GroupBy,
    BetweenAnd,
    Update,
    Insert,
    Delete,
    Variable,
    Exists,
    WithSubQuery,
    UnionQuery,
    All,
    Any,
    In,
    Case,
    CaseItem,
    Limit,
    Over,
    PartitionBy,
    Not,
    WithinGroup,
    Bool,
    ConnectBy,
    Top,
    Hint,
    AtTimeZone,
    /// <summary>
    /// Time interval；时间间隔
    /// </summary>
    Interval,
    /// <summary>
    /// Time Unit;时间单位
    /// </summary>
    TimeUnit,
    /// <summary>
    /// The collate clause is mainly used to specify string comparison and sorting rules.
    /// collate子句主要用于指定字符串比较和排序的规则
    /// </summary>
    Collate,
    /// <summary>
    /// Regular Expressions
    /// 正则表达式
    /// </summary>
    Regex,
}