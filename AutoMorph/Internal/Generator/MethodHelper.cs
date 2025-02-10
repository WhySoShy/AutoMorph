using System.Collections.Generic;
using System.Linq;
using AutoMorph.Abstractions.Attributes;
using AutoMorph.Abstractions.Enums;
using AutoMorph.Internal.Generator.Helpers;
using AutoMorph.Internal.Syntax.Kinds;
using AutoMorph.Internal.Syntax.Tokens;
using AutoMorph.Internal.Syntax.Types;
using Microsoft.CodeAnalysis;

namespace AutoMorph.Internal.Generator;

internal static class MethodHelper
{
    internal static List<MethodToken> GetMethods(
            INamedTypeSymbol sourceClass, 
            ClassToken classToken,
            Compilation compilation, 
            out HashSet<string> nameSpaces
        )
    {
        List<MethodToken> methods = [];
        nameSpaces = [];
        
        foreach (var attribute in GetValidMethods(classToken.Modifiers, sourceClass))
        {
            AttributeData attributeData = attribute.AttributeData;
            
            if (attributeData is not { AttributeClass: not null } || GetMethodType(attributeData) is var methodType && methodType is MethodType.None)
                continue;
            
            // It shouldn't continue if there is no empty constructors, because the generator does not support parameter filled constructors yet.
            if (attributeData.AttributeClass is not { InstanceConstructors.IsEmpty: false } ||
                attributeData.AttributeClass.TypeArguments[0] is not INamedTypeSymbol targetClass)
                return [];

            nameSpaces = GetMappingStrategy(attributeData) switch
            {
                MappingStrategy.Both => [
                    ..nameSpaces, 
                    ..CreateMethod(sourceClass, targetClass, classToken, methodType, attribute, methods, compilation, MappingStrategy.Normal),
                    ..CreateMethod(targetClass, sourceClass, classToken, methodType, attribute, methods, compilation, MappingStrategy.Reverse),
                ],
                MappingStrategy.Reverse => [
                    ..nameSpaces,
                ],
                _ => [
                    ..nameSpaces, 
                    ..CreateMethod(sourceClass, targetClass, classToken, methodType, attribute, methods, compilation, MappingStrategy.Normal)
                ],
            };
        }

        return methods;
    }

    /// <summary>
    /// Generates 
    /// </summary>
    /// <returns>Hashset of Namespaces, that needs to be added as usings.</returns>
    static HashSet<string> CreateMethod(
        INamedTypeSymbol sourceClass, 
        INamedTypeSymbol targetClass,
        ClassToken classToken,
        MethodType methodType, 
        ValidMethod validMethod, 
        List<MethodToken> generatedMethods, 
        Compilation compilation,
        MappingStrategy mappingStrategy
    )
    {
        AttributeData attributeData = validMethod.AttributeData;
        
        string? methodKey = attributeData.GetValueOfNamedArgument<string>("Key");

        string nameOfMapper = validMethod.IsExternal
            ? validMethod.MethodName
            : attributeData.GetValueOfNamedArgument<string>("MapperName") ?? sourceClass.GetNameOfMapper<IMapperAttribute>(targetClass, "MapperName");
            
        MethodToken generatedToken = new MethodToken(
            GetMethodModifiers(validMethod.IsExternal, classToken.Modifiers), 
            methodType, 
            nameOfMapper
        )
        {
            // There is a difference in how the mapper should type cast, it cannot use the TryParse() method if it is an expression tree for example,
            // therefor it should default to the Parse() method instead.
            IsExpressionTree = methodType is MethodType.IQueryable,
            TargetClass = targetClass.TransformClass(),
            SourceClass = sourceClass.TransformClass(),
            MappingStrategy = mappingStrategy
        };

        var namespacesToAppend = HandleGenerics(generatedToken, sourceClass, targetClass, attributeData, compilation, methodKey);
            
        if (!generatedToken.Properties.Any())
            return [];
            
        generatedMethods.Add(generatedToken);
        
        return namespacesToAppend;
    }
    
    /// <returns>All relevant attributes attached to methods or the source class.</returns>
    static IEnumerable<ValidMethod> GetValidMethods(List<ModifierKind> classKinds, INamedTypeSymbol sourceSymbol)
        => [
            // Get the methods that have Included attributes on them and is Partial if the class is marked as partial.
            // If the class has not been marked as partial, then it makes no sense to get methods that is maybe included.
            .. (classKinds.Any(x => x == ModifierKind.Partial) ? 
                sourceSymbol 
                    .GetMembers()
                    // We only need to iterate on the Methods, nothing else.
                    .Where(x => x.Kind == SymbolKind.Method && x.ContainsAttributeInterface<IIncludeAttribute>())
                    .Select(x => x
                            .GetAttributes()
                            .Where(y => y.IsAttributeOfInterface<IIncludeAttribute>())
                            .Select(y => new ValidMethod(y, true, x.Name))
                            .FirstOrDefault()
                    ) : []!)!,
            
            .. sourceSymbol
                .GetAttributes()
                .Where(x => x.IsAttributeOfInterface<IIncludeAttribute>())
                .Select(y => new ValidMethod(y, false, string.Empty))!
            
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
        => (MethodType)(attribute?.ConstructorArguments.FirstOrDefault(x => x.Type?.Name == nameof(MapperType)).Value ?? MethodType.None)+1;
    
    static MappingStrategy GetMappingStrategy(AttributeData? attribute)
        => (MappingStrategy)(attribute?.ConstructorArguments.FirstOrDefault(x => x.Type?.Name == nameof(MappingStrategy)).Value ?? MappingStrategy.Normal);
    
    static HashSet<string> HandleGenerics(MethodToken generatedToken, INamedTypeSymbol sourceClass, INamedTypeSymbol targetClass, AttributeData attribute, Compilation compilation, string? methodKey)
    {
        if (sourceClass.IsAbstract || attribute.GetValueOfNamedArgument<bool>("IsGeneric"))
            generatedToken.Generic = new MethodToken.GenericType(sourceClass.ToDisplayString());

        generatedToken.Properties = PropertyHelper.GetValidProperties(generatedToken, sourceClass, targetClass, compilation, methodKey, out var newNamespaces);
        
        return newNamespaces;
    }

    static string GetNameOfMapper<T>(this INamedTypeSymbol sourceSymbol, INamedTypeSymbol targetSymbol, string propertyName)
    {
        return sourceSymbol.GetAttributeFromInterface<T>() is not { AttributeClass: not null} foundAttribute ? 
            $"MapTo{targetSymbol.Name}" : foundAttribute.GetNameOfMapper(targetSymbol, propertyName);
    }
    
    static string GetNameOfMapper(this AttributeData attribute, INamedTypeSymbol targetSymbol, string propertyName)
        => attribute.NamedArguments.FirstOrDefault(x => x.Key.Equals(propertyName)) is { Value.Value: not null } property ? 
            property.Value.Value.ToString() : $"MapTo{targetSymbol.Name}";
    
    /// <summary>
    /// This is just used for a clear visualization of the attached attributes that are relevant for the generator.
    /// </summary>
    record ValidMethod(AttributeData AttributeData, bool IsExternal, string MethodName);
}