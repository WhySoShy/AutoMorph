using System.CodeDom.Compiler;
using IncrementialMapper.Syntax.Kinds;
using IncrementialMapper.Syntax.Tokens;
using IncrementialMapper.SyntaxProviders.Enums;
using IncrementialMapper.Utilities;

namespace IncrementialMapper.SyntaxProviders;

internal static class ClassProvider
{
    private const string VISIBILITY_TYPE = "public";
    
    public static ClassToken CreateSourceClass(this ClassToken token)
    {
        IndentedTextWriter writer = token.Writer;

        string generatorClassName = $"GeneratedMapper_{token.SourceClass.Name}To{token.TargetClass.Name}";
        
        writer
            .Append(token.Visibility!.Value.ToReadAbleString() + " ")
            .AppendModifiers(token.Modifiers)
            .Append($"class {generatorClassName}").AppendLine()
            .AppendFormat(FormatType.OpenCurlyBraces, IndentType.Indent).AppendLine()
            .AppendFormat(FormatType.ClosedCurlyBraces, IndentType.Outdent).AppendLine();
        
        return token;
    }

    private static IndentedTextWriter AppendModifiers(this IndentedTextWriter writer, ModifierKind[] modifiers)
    {
        foreach (ModifierKind modifier in modifiers)
            writer.Append(modifier.ToReadableString() + " ");
        
        return writer;
    }
}