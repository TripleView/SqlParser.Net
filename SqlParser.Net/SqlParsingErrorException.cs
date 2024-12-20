using System;
using System.Collections.Generic;
using SqlParser.Net.Lexer;

namespace SqlParser.Net;

public class SqlParsingErrorException:Exception
{
    public string Message { get; set; }

    public SqlParsingErrorException(string message)
    {
        Message = message;
    }
}