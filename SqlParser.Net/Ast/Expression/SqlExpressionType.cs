namespace SqlParser.Net.Ast.Expression;

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
    TimeUnit
}