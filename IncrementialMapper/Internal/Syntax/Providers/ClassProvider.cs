using System.CodeDom.Compiler;
using System.Collections.Generic;
using System.Linq;
using AutoMorph.Internal.Syntax.Kinds;
using AutoMorph.Internal.Syntax.Providers.Enums;
using AutoMorph.Internal.Syntax.Tokens;

namespace AutoMorph.Internal.Syntax.Providers;

internal static class ClassProvider
{
    internal static IndentedTextWriter GenerateSourceClass(this IndentedTextWriter writer, ClassToken token)
    {
        string generatorClassName = token.Modifiers.Any(x => x is ModifierKind.Partial) ? $"{token.SourceClass.Name}" : $"GeneratedMapper_{token.SourceClass.Name}To{token.TargetClass.Name}";
        
        writer
            .Append(token.Visibility!.ToReadAbleString() + " ")
            .AppendModifiers(token.Modifiers)
            .Append($"class {generatorClassName}").AppendNewLine()
            .AppendFormat(FormatType.OpenCurlyBraces, IndentType.Indent).AppendNewLine()
                .CreateMethods(token) // Append all the methods, that should be generated inside the class scope.
            .AppendFormat(FormatType.ClosedCurlyBraces, IndentType.Outdent).AppendNewLine();
        
        return writer;
    }

    static IndentedTextWriter AppendModifiers(this IndentedTextWriter writer, List<ModifierKind> modifiers)
    {
        foreach (ModifierKind modifier in modifiers)
            writer.Append(modifier.GetModifierAsString() + " ");
        
        return writer;
    }
}