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

namespace IncrementialMapper.Internal.Generator.GeneratorHelpers;

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
            var huh = attribute.symbol;
            // Check if the attribute is valid for the enabled attributes.
            if (ValidAttributes.ValidIncludeAttributes.FirstOrDefault(x => x.Key == fullAttributeName).Value is not { } methodKind)
                continue;

            MethodToken generatedToken = new MethodToken(GetMethodModifiers(attribute.isExternal, classKinds), methodKind,
                attribute.isExternal ? attribute.methodName : $"MapTo{targetClassName}");
            
            methods.Add(generatedToken);
        }
        
        // Always include the standard mapper unless the user has explicitly told not to.
        if (!sourceSymbol.ContainsAttribute<Exclude>())
            methods.Add(new MethodToken([classKinds.Contains(ModifierKind.None) ? ModifierKind.Static : ModifierKind.None], MethodType.Standard, defaultMethodName));
        
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
                    .Where(x => x.Kind == SymbolKind.Method && x.ContainsAttribute<Include>())
                    .Select(x => (x
                            .GetAttributes()
                            .FirstOrDefault(_validAttributeExpression)!
                            .AttributeClass, true, x.Name)
                    ) : []!)!,
            
            .. sourceSymbol
                .GetAttributes()
                .Where(_validAttributeExpression)
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
    
    static readonly Func<AttributeData, bool> _validAttributeExpression = x => x.AttributeClass?.ToDisplayString() == AssemblyConstants.FULLY_QUALIFIED_ATTRIBUTE_NAMESPACE + $".{nameof(Include)}";
}