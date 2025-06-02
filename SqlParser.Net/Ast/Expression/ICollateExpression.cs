namespace SqlParser.Net.Ast.Expression;

/// <summary>
/// The collate clause is mainly used to specify string comparison and sorting rules.
/// collate子句主要用于指定字符串比较和排序的规则
/// </summary>
public interface ICollateExpression
{
    public SqlCollateExpression Collate { set; get; }
}