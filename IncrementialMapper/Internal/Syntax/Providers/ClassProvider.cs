using System.CodeDom.Compiler;
using System.Collections.Generic;
using System.Linq;
using IncrementialMapper.Internal.Syntax.Kinds;
using IncrementialMapper.Internal.Syntax.Providers.Enums;
using IncrementialMapper.Internal.Syntax.Tokens;

namespace IncrementialMapper.Internal.Syntax.Providers;

internal static class ClassProvider
{
    public static ClassToken GenerateSourceClass(this ClassToken token)
    {
        IndentedTextWriter writer = token.Writer;

        string generatorClassName = token.Modifiers.Any(x => x is ModifierKind.Partial) ? $"{token.SourceClass.Name}" : $"GeneratedMapper_{token.SourceClass.Name}To{token.TargetClass.Name}";
        
        writer
            .Append(token.Visibility!.Value.ToReadAbleString() + " ")
            .AppendModifiers(token.Modifiers)
            .Append($"class {generatorClassName}").AppendNewLine()
            .AppendFormat(FormatType.OpenCurlyBraces, IndentType.Indent).AppendNewLine()
                // Append all the methods, that should be generated inside the class scope.
                .CreateMethods(token)
            .AppendFormat(FormatType.ClosedCurlyBraces, IndentType.Outdent).AppendNewLine();
        
        return token;
    }

    private static IndentedTextWriter AppendModifiers(this IndentedTextWriter writer, List<ModifierKind> modifiers)
    {
        foreach (ModifierKind modifier in modifiers)
            writer.Append(modifier.GetModifierAsString() + " ");
        
        return writer;
    }
}