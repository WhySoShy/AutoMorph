using System.CodeDom.Compiler;
using System.Collections.Generic;
using System.Linq;
using IncrementialMapper.Syntax.Kinds;
using IncrementialMapper.Syntax.Tokens;
using IncrementialMapper.SyntaxProviders.Enums;
using IncrementialMapper.Utilities;

namespace IncrementialMapper.SyntaxProviders;

internal static class ClassProvider
{
    public static ClassToken CreateSourceClass(this ClassToken token)
    {
        IndentedTextWriter writer = token.Writer;

        string generatorClassName = token.Modifiers.Any(x => x is ModifierKind.Partial) ? $"{token.SourceClass.Name}" : $"GeneratedMapper_{token.SourceClass.Name}To{token.TargetClass.Name}";
        
        writer
            .Append(token.Visibility!.Value.ToReadAbleString() + " ")
            .AppendModifiers(token.Modifiers)
            .Append($"class {generatorClassName}").AppendLine()
            .AppendFormat(FormatType.OpenCurlyBraces, IndentType.Indent).AppendLine()
                // Append all the methods, that should be generated inside the class scope.
                .CreateMethods(token)
            .AppendFormat(FormatType.ClosedCurlyBraces, IndentType.Outdent).AppendLine();
        
        return token;
    }

    private static IndentedTextWriter AppendModifiers(this IndentedTextWriter writer, List<ModifierKind> modifiers)
    {
        foreach (ModifierKind modifier in modifiers)
            writer.Append(modifier.ToReadableString() + " ");
        
        return writer;
    }
}