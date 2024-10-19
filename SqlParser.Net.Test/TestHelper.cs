using System.Reflection;
using System.Text;
using SqlParser.Net.Ast.Expression;
using Xunit.Abstractions;

namespace SqlParser.Net.Test;

public class TestHelper
{
    [Fact]
    public void GenerateIAstVisitorAndBaseAstVisitor()
    {
        var sb = new StringBuilder();
        sb.Append("public interface IAstVisitor\r\n{\r\n");
        var sb2 = new StringBuilder();
        sb2.Append("public class BaseAstVisitor : IAstVisitor\r\n{\r\n");
        var expressionList = typeof(SqlExpression).Assembly.DefinedTypes.Where(it => typeof(SqlExpression).IsAssignableFrom(it)).ToList();
        foreach (var typeInfo in expressionList)
        {
            sb.Append($"    void Visit{typeInfo.Name}({typeInfo.Name} {typeInfo.Name.Substring(0, 1).ToLower() + typeInfo.Name.Substring(1)});\r\n");
            sb2.Append($"    public virtual void Visit{typeInfo.Name}({typeInfo.Name} {typeInfo.Name.Substring(0, 1).ToLower() + typeInfo.Name.Substring(1)})\r\n    {{\r\n\r\n    }}\r\n");
        }

        sb.Append("}");
        sb2.Append("}");
        var iAstVisitor = sb.ToString();
        var baseAstVisitor = sb2.ToString();

    }



}