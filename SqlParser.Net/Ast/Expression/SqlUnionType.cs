namespace SqlParser.Net.Ast.Expression;

public enum SqlUnionType
{
    Union,
    UnionAll,
    Intersect,
    IntersectAll,
    Except,
    ExceptAll,
    /// <summary>
    /// oracle except
    /// </summary>
    Minus
}