using System.CodeDom.Compiler;
using System.Linq;
using AutoMorph.Internal.Constants;
using AutoMorph.Internal.Generator.GeneratorHelpers;
using AutoMorph.Internal.Syntax.Kinds;
using AutoMorph.Internal.Syntax.Providers.Enums;
using AutoMorph.Internal.Syntax.Tokens;
using AutoMorph.Internal.Syntax.Types;

namespace AutoMorph.Internal.Syntax.Providers;

internal static class MethodProvider
{
    private const string PARAMETER_NAME = "source";
    
    internal static IndentedTextWriter CreateMethods(this IndentedTextWriter writer, ClassToken classToken)
    {
        int methodCount = 0;
        foreach (MethodToken methodToken in classToken.Methods)
        {
            writer
                .AppendMethodHeader(methodToken, classToken)
                .AppendNewLine().AppendFormat(FormatType.OpenCurlyBraces, IndentType.Indent) // Open the method body
                    .AppendNewLine().AppendMethodBody(methodToken)
                .AppendNewLine().AppendFormat(FormatType.ClosedCurlyBraces, IndentType.Outdent); // Close the method body
            
            if (classToken.Methods.Count < methodCount)
                continue;
            
            methodCount++;
            writer.AppendNewLine();
        }

        return writer;
    }

    /// <summary>
    /// Appends the return type of <see cref="MethodType"/>
    /// </summary>
    static IndentedTextWriter AppendMethodBody(this IndentedTextWriter writer, MethodToken methodToken)
    {
        // This is the reference used, to map from the source class to the target class.
        // This will always be either x or source, depending on the return type.
        // For example;
        // new ClassX() { Property = source.Property }
        // source.Select(x => new ClassX() { Property = x.Property })
        string sourceReference = methodToken.GetReferenceSourceName();

        writer.Append("return ");

        if (methodToken.Type is MethodType.Linq or MethodType.IQueryable)
            writer.Append($"{PARAMETER_NAME}.Select({sourceReference} => ");
        
        writer
            .Append($"new {methodToken.TargetClass.FullPath}()")
            .AppendNewLine().AppendFormat(FormatType.OpenCurlyBraces, IndentType.Indent).AppendNewLine()
            .AppendProperties(sourceReference, methodToken).AppendNewLine();

        writer
            .AppendFormat(FormatType.ClosedCurlyBraces, IndentType.Outdent);
            
        if (methodToken.Type is MethodType.Linq or MethodType.IQueryable)
            writer
                .AppendFormat(FormatType.ClosedParentheses, IndentType.Passive);
            
        writer
            .Append(";");

        return writer;
    }

    static IndentedTextWriter AppendProperties(this IndentedTextWriter writer, string sourceReference, MethodToken methodToken)
    {
        int count = 0;
        
        foreach (ReferencePropertyToken property in methodToken.Properties)
        {
            count++;

            writer.Append($"{property.TargetProperty.Name} = ");

            if (property.NestedObject is not null)
                writer.Append(property.NestedObject.Type.Value.Invoke(property, sourceReference));
            else
                writer.Append(UtilHelper.GetCastingAsString(property.TargetProperty, property.SourceProperty, sourceReference));

            if (count < methodToken.Properties.Count)
                writer
                    .Append(",")
                    .AppendNewLine();
        }

        return writer;
    }

    static IndentedTextWriter AppendMethodHeader(this IndentedTextWriter writer, MethodToken methodToken, ClassToken classToken)
    {
        string methodModifier = classToken.Modifiers[0] is ModifierKind.None ? string.Empty : " " + methodToken.Modifiers[0].GetModifierAsString() + " ";
        string visibility = classToken.Visibility.ToReadAbleString();
        string parameterKeyword = classToken.Modifiers.Any(x => x is ModifierKind.Static) ? "this " : string.Empty;
        bool isGenericType = methodToken.Generic is not null;

        writer
            .Append($"{visibility}{methodModifier}")
            // It still returns the target type
            .Append(methodToken.GetMethodKindAsString(methodToken.TargetClass, false) + " " + methodToken.Name);
    
        if (isGenericType)
            writer.Append($"<{AssemblyConstants.GENERIC_TYPE_NAME}>");
        
        writer
            .AppendFormat(FormatType.OpenParentheses, IndentType.Passive)
                .Append(parameterKeyword)
                .Append($"{methodToken.GetMethodKindAsString(classToken.SourceClass, isGenericType)} {PARAMETER_NAME}")
            .AppendFormat(FormatType.ClosedParentheses, IndentType.Passive);

        if (isGenericType)
            writer
                .AppendNewLine()
                .AppendFormat(FormatType.None, IndentType.Indent)
                .Append($"where {AssemblyConstants.GENERIC_TYPE_NAME} : {AssemblyConstants.GLOBAL_KEYWORD}{methodToken.Generic!.ConstraintTypeName}")
                .AppendFormat(FormatType.None, IndentType.Outdent);
        
        return writer;
    }
}