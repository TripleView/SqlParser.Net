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
    /// Time interval��ʱ����
    /// </summary>
    Interval,
    /// <summary>
    /// Time Unit;ʱ�䵥λ
    /// </summary>
    TimeUnit,
    /// <summary>
    /// The collate clause is mainly used to specify string comparison and sorting rules.
    /// collate�Ӿ���Ҫ����ָ���ַ����ȽϺ�����Ĺ���
    /// </summary>
    Collate,
    /// <summary>
    /// Regular Expressions
    /// ������ʽ
    /// </summary>
    Regex,
    /// <summary>
    /// Returning expression, allowing the return of field values after an insert in PostgreSQL and Oracle
    /// Returning���ʽ�� pgsql��oracle��insert���������ֶε�ֵ
    /// </summary>
    Returning,
}