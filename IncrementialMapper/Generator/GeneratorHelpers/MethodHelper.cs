using System;
using System.Collections.Generic;
using System.Linq;
using IncrementialMapper.Abstractions;
using IncrementialMapper.Generator.Utilities;
using IncrementialMapper.Syntax.Kinds;
using IncrementialMapper.Syntax.Tokens;
using Microsoft.CodeAnalysis;
using MethodKind = IncrementialMapper.Syntax.Kinds.MethodKind;

namespace IncrementialMapper.Generator.GeneratorHelpers;

internal static class MethodHelper
{
    internal static List<MethodToken> GetMethods(INamedTypeSymbol sourceSymbol, List<ModifierKind> classKinds, string targetClassName)
    {
        List<MethodToken> methods = [];

        var availableMethods = GetValidMethods(classKinds, sourceSymbol).Where(x => x.symbol is not null);

        // This is the default method name, that is being used when a method is not partial.
        string defaultMethodName = $"MapTo{targetClassName}";
        
        // Go through each attribute, and check if it is a valid attribute.
        foreach (var attribute in availableMethods)
        {
            string fullAttributeName = attribute.symbol.ToDisplayString();
            
            // Check if the attribute is valid for the enabled attributes.
            if (ValidAttributes.ValidIncludeAttributes.FirstOrDefault(x => x.Key == fullAttributeName).Value is not { } methodKind)
                continue;

            MethodToken generatedToken = new MethodToken(GetMethodModifiers(attribute.isExternal, classKinds), methodKind,
                attribute.isExternal ? attribute.methodName : $"MapTo{targetClassName}");
            
            methods.Add(generatedToken);
        }
        
        // Always include the standard mapper unless the user has explicitly told not to.
        if (!sourceSymbol.ContainsAttribute<ExcludeStandard>())
            methods.Add(new MethodToken([classKinds.Contains(ModifierKind.None) ? ModifierKind.Static : ModifierKind.None], MethodKind.Standard, defaultMethodName));
        
        return methods;
    }
    
    static IEnumerable<(ISymbol symbol, bool isExternal, string methodName)> GetValidMethods(List<ModifierKind> classKinds, INamedTypeSymbol sourceSymbol)
        => [
            // Get the methods that have Included attributes on them and is Partial if the class is marked as partial.
            // If the class has not been marked as partial, then it makes no sense to get methods that is maybe included.
            .. (classKinds.Any(x => x == ModifierKind.Partial) ? 
                sourceSymbol 
                    .GetMembers()
                    // We only need to iterate on the Methods, nothing else.
                    .Where(x => x.Kind == SymbolKind.Method && x.ContainsAttribute<IncludeLinq>() || x.ContainsAttribute<IncludeIQueryable>())
                    .Select(x => (x
                            .GetAttributes()
                            .FirstOrDefault(GetValidAttributeExpression())!
                            .AttributeClass, true, x.Name)
                    ) : []!)!,
            
            .. sourceSymbol
                .GetAttributes()
                .Where(GetValidAttributeExpression())
                .Select(y => (y.AttributeClass, false, string.Empty))!
            
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
    
    static Func<AttributeData, bool> GetValidAttributeExpression()
        => x => ValidAttributes.ValidIncludeAttributes.Any(y =>
            y.Key == $"{ValidAttributes.INCLUDE_ATTRIBUTE_NAMESPACE}.{x.AttributeClass!.Name}");
}