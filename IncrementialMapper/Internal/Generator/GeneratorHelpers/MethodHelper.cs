using System;
using System.Collections.Generic;
using System.Linq;
using IncrementialMapper.Abstractions.Attributes;
using IncrementialMapper.Internal.Constants;
using IncrementialMapper.Internal.Generator.Validators;
using IncrementialMapper.Internal.Syntax.Kinds;
using IncrementialMapper.Internal.Syntax.Tokens;
using IncrementialMapper.Internal.Syntax.Types;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace IncrementialMapper.Internal.Generator.GeneratorHelpers;

internal static class MethodHelper
{
    internal static List<MethodToken> GetMethods(INamedTypeSymbol sourceSymbol, List<ModifierKind> classKinds, string targetClassName)
    {
        List<MethodToken> methods = [];
        
        // Why isn't it returning anything???
        var availableMethods = GetValidMethods(classKinds, sourceSymbol).Where(x => x.symbol is not null);

        var attrClass = sourceSymbol.GetAttributes().Last().AttributeClass;
        var qualifiedName = nameof(Include).AttributeAsQualifiedName();
        var attrNameEquals = attrClass.ToDisplayString() == qualifiedName;
        var baseAttrNameEquals = attrClass.BaseType.ToDisplayString() == qualifiedName;
        var contains = attrClass.ContainsAttribute(nameof(Include).AttributeAsQualifiedName());
        
        // This is the default method name, that is being used when a method is not partial.
        string defaultMethodName = $"MapTo{targetClassName}";
        
        // Go through each attribute, and check if it is a valid attribute.
        foreach (var attribute in availableMethods)
        {
            MethodType methodType = GetMethodType(attribute.symbol);
            
            if (methodType is MethodType.None)
                continue;

            MethodToken generatedToken = new MethodToken(GetMethodModifiers(attribute.isExternal, classKinds), methodType,
                attribute.isExternal ? attribute.methodName : $"MapTo{targetClassName}");
            
            methods.Add(generatedToken);
        }
        
        // Always include the standard mapper unless the user has explicitly told not to.
        if (!sourceSymbol.ContainsAttribute(nameof(Exclude).AttributeAsQualifiedName()))
            methods.Add(new MethodToken([classKinds.Contains(ModifierKind.None) ? ModifierKind.Static : ModifierKind.None], MethodType.Standard, defaultMethodName));
        
        return methods;
    }
    
    static IEnumerable<(AttributeData symbol, bool isExternal, string methodName)> GetValidMethods(List<ModifierKind> classKinds, INamedTypeSymbol sourceSymbol)
        => [
            // Get the methods that have Included attributes on them and is Partial if the class is marked as partial.
            // If the class has not been marked as partial, then it makes no sense to get methods that is maybe included.
            .. (classKinds.Any(x => x == ModifierKind.Partial) ? 
                sourceSymbol 
                    .GetMembers()
                    // We only need to iterate on the Methods, nothing else.
                    .Where(x => x.Kind == SymbolKind.Method && x.ContainsAttribute(nameof(Include).AttributeAsQualifiedName()))
                    .Select(x => (x
                            .GetAttributes()
                            .FirstOrDefault(y => y.AttributeClass.ContainsAttribute(nameof(Include).AttributeAsQualifiedName()))!, true, x.Name)
                    ) : []!)!,
            
            .. sourceSymbol
                .GetAttributes()
                .Where(x => x.AttributeClass.ContainsAttribute(nameof(Include).AttributeAsQualifiedName()))
                .Select(y => (y, false, string.Empty))!
            
            // TODO: Find a way to make this less hard-coded.
        ];

    static ModifierKind[] GetMethodModifiers(bool isExternal, List<ModifierKind> classKinds)
    {
        List<ModifierKind> modifierKinds = [];

        if (isExternal)
            modifierKinds.Add(ModifierKind.Partial);
        
        if (classKinds.Contains(ModifierKind.Static))
            modifierKinds.Add(ModifierKind.Static);
        
        return !modifierKinds.Any() ? [ModifierKind.None] : [.. modifierKinds];
    }

    static MethodType GetMethodType(AttributeData? attribute)
        // You need to increment the value with 1, else it will give an incorrect value.
        => (MethodType)(attribute?.ConstructorArguments.FirstOrDefault(x => x.Type?.Name == "MapperType").Value ?? MethodType.None)+1;
}