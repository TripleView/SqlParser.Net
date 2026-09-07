namespace SqlParser.Net.Ast.Expression;

/// <summary>
/// The LATERAL Keyword in PostgreSQL
/// pgsql中的关键字lateral
/// </summary>
public interface ILateralExpression
{
    /// <summary>
    /// The LATERAL Modifier in PostgreSQL
    /// pgsql中的lateral修饰
    /// </summary>
    public bool? IsLateral { set; get; }
}