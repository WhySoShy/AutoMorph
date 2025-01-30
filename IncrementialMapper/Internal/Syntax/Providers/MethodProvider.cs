using System.CodeDom.Compiler;
using System.Linq;
using IncrementialMapper.Internal.Constants;
using IncrementialMapper.Internal.Syntax.Kinds;
using IncrementialMapper.Internal.Syntax.Providers.Enums;
using IncrementialMapper.Internal.Syntax.Tokens;
using IncrementialMapper.Internal.Syntax.Types;

namespace IncrementialMapper.Internal.Syntax.Providers;

internal static class MethodProvider
{
    private const string PARAMETER_NAME = "source";
    
    public static IndentedTextWriter CreateMethods(this IndentedTextWriter writer, ClassToken classToken)
    {
        int methodCount = 0;
        foreach (MethodToken method in classToken.Methods)
        {
            GenerateMethod(method, classToken);

            if (classToken.Methods.Count < methodCount)
                continue;
            
            methodCount++;
            writer.AppendNewLine();
        }

        return writer;
    }

    private static void GenerateMethod(MethodToken token, ClassToken classToken)
    {
        IndentedTextWriter writer = classToken.Writer;

        string methodModifier = token.Modifiers[0] is ModifierKind.None ? string.Empty : token.Modifiers[0].GetModifierAsString() + " ";
        string parameterKeyword = classToken.Modifiers.Any(x => x is ModifierKind.Static) ? "this " : string.Empty;
        
        writer
            .AppendMethodHeader(token, classToken)
            .AppendNewLine().AppendFormat(FormatType.OpenCurlyBraces, IndentType.Indent).AppendNewLine()
                .AppendReturn(token, classToken)
            .AppendNewLine().AppendFormat(FormatType.ClosedCurlyBraces, IndentType.Outdent).AppendNewLine();
    }

    /// <summary>
    /// Appends the return type of <see cref="MethodType"/>
    /// </summary>
    private static IndentedTextWriter AppendReturn(this IndentedTextWriter writer, MethodToken currentMethod, ClassToken token)
    {
        // This is the reference used, to map from the source class to the target class.
        // This will always be either x or source, depending on the return type.
        // For example;
        // new ClassX() { Property = source.Property }
        // source.Select(x => new ClassX() { Property = x.Property })
        string sourceReference = currentMethod.GetReferenceSourceName();

        writer.Append("return ");

        if (currentMethod.Type is MethodType.Linq or MethodType.IQueryable)
            writer.Append($"{PARAMETER_NAME}.Select({sourceReference} => ");
        
        writer
            .Append($"new {token.TargetClass.FullPath}()")
            .AppendNewLine().AppendFormat(FormatType.OpenCurlyBraces, IndentType.Indent).AppendNewLine()
            .AppendProperties(sourceReference, currentMethod).AppendNewLine();

        writer
            .AppendFormat(FormatType.ClosedCurlyBraces, IndentType.Outdent);
            
        if (currentMethod.Type is MethodType.Linq or MethodType.IQueryable)
            writer
                .AppendFormat(FormatType.ClosedParentheses, IndentType.Passive);
            
        writer
            .Append(";");

        return writer;
    }

    private static IndentedTextWriter AppendProperties(this IndentedTextWriter writer, string sourceReference, MethodToken token)
    {
        int count = 0;
        foreach (ReferencePropertyToken property in token.Properties)
        {
            count++;

            writer.Append($"{property.TargetPropertyName} = ");

            if (property.NestedObject is not null)
                writer.Append(property.NestedObject.Type.Value.Invoke(property, sourceReference));
            else
                writer.Append($"{sourceReference}.{property.SourcePropertyName}");

            if (count < token.Properties.Count)
                writer
                    .Append(",")
                    .AppendNewLine();
        }

        return writer;
    }

    private static IndentedTextWriter AppendMethodHeader(this IndentedTextWriter writer, MethodToken methodToken, ClassToken classToken)
    {
        string methodModifier = classToken.Modifiers[0] is ModifierKind.None ? string.Empty : " " + classToken.Modifiers[0].GetModifierAsString() + " ";
        string visibility = classToken.Visibility.Value.ToReadAbleString();
        string parameterKeyword = classToken.Modifiers.Any(x => x is ModifierKind.Static) ? "this " : string.Empty;

        writer
            .Append($"{visibility}{methodModifier}")
            // It still returns the target type
            .Append(methodToken.GetMethodKindAsString(classToken.TargetClass, false) + " " + methodToken.Name);

        if (methodToken.IsGeneric)
            writer.Append($"<{AssemblyConstants.GENERIC_TYPE_NAME}>");
        
        writer
            .AppendFormat(FormatType.OpenParentheses, IndentType.Passive)
                .Append(parameterKeyword)
                .Append($"{methodToken.GetMethodKindAsString(classToken.SourceClass, methodToken.IsGeneric)} {PARAMETER_NAME}")
            .AppendFormat(FormatType.ClosedParentheses, IndentType.Passive);

        if (methodToken.IsGeneric)
            writer
                .AppendNewLine()
                .AppendFormat(FormatType.None, IndentType.Indent)
                .Append($"where {AssemblyConstants.GENERIC_TYPE_NAME} : {AssemblyConstants.GLOBAL_KEYWORD}{methodToken.GenericTypeName}")
                .AppendFormat(FormatType.None, IndentType.Outdent);
        
        return writer;
    }
}