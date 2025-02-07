﻿using System.CodeDom.Compiler;
using AutoMorph.Internal.Syntax.Providers.Enums;

namespace AutoMorph.Internal.Syntax.Providers;

internal static class FormatProvider
{
    const string AUTO_GENERATED_COMMENT = "/// <auto-generated />";

    /// <summary>
    /// Appends either <c>(</c>, <c>)</c>, <c>{</c>, <c>}</c> into the TextWriter.
    /// </summary>
    internal static IndentedTextWriter AppendFormat(this IndentedTextWriter writer, FormatType formatType, IndentType indentationType)
    {
        if (indentationType is IndentType.Outdent)
            writer.Indent--;
        
        if (formatType is not FormatType.None)
            writer.Write((char)formatType);
        
        if (indentationType is IndentType.Indent)
            writer.Indent++;
        
        return writer;
    }
    
    internal static IndentedTextWriter AppendAutoGeneratedComment(this IndentedTextWriter writer)
    {
        return writer
                .Append(AUTO_GENERATED_COMMENT)
                .AppendNewLine();
    }
    
    internal static IndentedTextWriter AppendNewLine(this IndentedTextWriter writer, int newlineCount = 1)
    {
        for(var i = 0; i < newlineCount; i++)
            writer.WriteLine();
        
        return writer;
    }

    internal static IndentedTextWriter Append(this IndentedTextWriter writer, string text)
    {
        writer.Write(text);
        return writer;
    }
}
