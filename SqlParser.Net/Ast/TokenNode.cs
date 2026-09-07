using System;
using System.Collections.Generic;

namespace SqlParser.Net.Ast;

public abstract class TokenNode
{
}

public class OrTokenNode(List<TokenNode> tokenNodes) : TokenNode
{
    public List<TokenNode> TokenNodes = tokenNodes;
}

public class AndTokenNode(List<TokenNode> tokenNodes) : TokenNode
{
    public List<TokenNode> TokenNodes = tokenNodes;
}

public class ValueTokenNode : TokenNode
{
    public TokenWrapper TokenWrapper { get; set; }  
}

public class FieldTypeTokenNode : TokenNode
{
}

public class BoolTokenNode : TokenNode
{
    public BoolTokenNode(Func<bool> func)
    {
        Func = func;
    }
    public Func<bool> Func { get; set; }
}