namespace SqlParser.Net.Ast.Expression;

public interface IQualifierExpression
{
    /// <summary>
    /// Left Qualifiers
    /// 左限定符
    /// </summary>
    public string LeftQualifiers { get; set; }
    /// <summary>
    /// right Qualifiers
    /// 右限定符
    /// </summary>
    public string RightQualifiers { get; set; }
}