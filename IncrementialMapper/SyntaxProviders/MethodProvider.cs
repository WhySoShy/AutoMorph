using System.CodeDom.Compiler;
using System.Linq;
using IncrementialMapper.Syntax.Kinds;
using IncrementialMapper.Syntax.Tokens;
using IncrementialMapper.SyntaxProviders.Enums;
using IncrementialMapper.Utilities;

namespace IncrementialMapper.SyntaxProviders;

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
            writer.AppendLine();
        }

        return writer;
    }

    private static void GenerateMethod(MethodToken token, ClassToken classToken)
    {
        IndentedTextWriter writer = classToken.Writer;

        string methodModifier = token.Modifiers[0] is ModifierKind.None ? string.Empty : token.Modifiers[0].GetModifierAsString() + " ";
        string parameterKeyword = classToken.Modifiers.Any(x => x is ModifierKind.Static) ? "this " : string.Empty;
        
        writer
            .Append($"{classToken.Visibility!.Value.ToReadAbleString()} {methodModifier}")
            .Append(token.GetMethodTypeAsString(classToken.TargetClass) + " " + token.Name)
            .AppendFormat(FormatType.OpenParentheses, IndentType.Passive)
                .Append(parameterKeyword)
                .Append($"{token.GetMethodTypeAsString(classToken.SourceClass)} {PARAMETER_NAME}")
            .AppendFormat(FormatType.ClosedParentheses, IndentType.Passive)
            .AppendLine().AppendFormat(FormatType.OpenCurlyBraces, IndentType.Indent).AppendLine()
                .AppendReturn(token, classToken)
            .AppendLine().AppendFormat(FormatType.ClosedCurlyBraces, IndentType.Outdent).AppendLine();
    }

    /// <summary>
    /// Appends the return type of <see cref="MethodKind"/>
    /// </summary>
    private static IndentedTextWriter AppendReturn(this IndentedTextWriter writer, MethodToken currentMethod, ClassToken token)
    {
        // This is the reference used, to map from the source class to the target class.
        // This will always be either x or source, depending on the return type.
        // For example;
        // new ClassX() { Property = source.Property }
        // source.Select(x => new ClassX() { Property = x.Property })
        string sourceReference = currentMethod.GetVariableSourceName();

        writer
            .Append("return ");

        if (currentMethod.Type is MethodKind.Linq or MethodKind.Linq_IQueryable)
            writer
                .Append($"{PARAMETER_NAME}.Select({sourceReference} => ");
        
        writer
            .Append($"new {token.TargetClass.FullPath}()")
            .AppendLine().AppendFormat(FormatType.OpenCurlyBraces, IndentType.Indent).AppendLine()
            .AppendProperties(sourceReference, token).AppendLine();

        writer
            .AppendFormat(FormatType.ClosedCurlyBraces, IndentType.Outdent);
            
        if (currentMethod.Type is MethodKind.Linq or MethodKind.Linq_IQueryable)
            writer
                // .AppendIndentation(IndentType.Outdent)
                .AppendFormat(FormatType.ClosedParentheses, IndentType.Passive);
            
        writer
            .Append(";");

        return writer;
    }

    private static IndentedTextWriter AppendProperties(this IndentedTextWriter writer, string sourceReference, ClassToken token)
    {
        int count = 0;
        foreach (ReferencePropertyToken property in token.Properties)
        {
            count++;

            writer.Append($"{property.TargetPropertyName} = ");

            if (property.NestedObject is not null)
            {
                writer.Append(property.NestedObject.Type.Value.Invoke(property, sourceReference));
                // switch (property.NestedObject.Kind)
                // {
                //     case PropertyTypeKind.Collection:
                //         writer.Append(property.NestedObject.Type.Value.Invoke(property, sourceReference));
                //         break;
                //     case PropertyTypeKind.Object:
                //         writer.Append(property.Nested);
                //         break;
                // }
            }
            else
                writer.Append($"{sourceReference}.{property.SourcePropertyName}");
            
            // writer.Append(property.NestedObject?.MethodToken.Name is not null
            //     ? $"{property.TargetPropertyName} = {sourceReference}.{property.SourcePropertyName}.{property.NestedObject?.MethodToken.Name}()"
            //     : $"{property.TargetPropertyName} = {sourceReference}.{property.SourcePropertyName}");

            if (count < token.Properties.Count)
                writer
                    .Append(",")
                    .AppendLine();
        }

        return writer;
    }
}