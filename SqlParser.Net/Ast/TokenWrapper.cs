using SqlParser.Net.Lexer;

namespace SqlParser.Net.Ast;

public class TokenWrapper
{
    public Token Token { get; set; }
    /// <summary>
    /// Whether It Contains a Numeric Modifier;是否包含数字修饰符
    /// </summary>
    public bool HasOptionalNumberModifiers { get; set; }
    /// <summary>
    /// Whether It Is Optional;是否可选
    /// </summary>
    public bool IsOptional { get; set; }
}