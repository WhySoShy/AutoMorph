using System;
using System.Collections.Generic;
using System.Linq;
using AutoMorph.Internal.Generator.Casting;
using AutoMorph.Internal.Syntax.Tokens;
using AutoMorph.Internal.Syntax.Types;
using Microsoft.CodeAnalysis;

namespace AutoMorph.Internal.Generator;

public static partial class PropertyHelper
{
    /// <summary>
    /// Used to handle nesting in properties, if any objects needs a mapper attached to them, they will get created here.
    /// </summary>
    static ReferencePropertyToken.NestedObjectToken? GetNestedPropertyTokens(IPropertySymbol sourceProperty, Compilation compilation, out string? newNamespace)
    {
        newNamespace = null;

        // Just return an empty object, if it is not an object or collection.
        if (!(sourceProperty.Type.TypeKind is TypeKind.Class or TypeKind.Struct or TypeKind.Array && sourceProperty.Type.SpecialType is SpecialType.None))
            return null;
        
        string name = sourceProperty.Type is IArrayTypeSymbol ? ValidCollections.ARRAY_CUSTOM : sourceProperty.Type.OriginalDefinition.ToDisplayString();

        // Check if the collection type is supported by the system.
        if (ValidCollections.SupportedCollections.FirstOrDefault(x
                    => x.Key == name) is
                { Key: not null } foundCollection)
            return HandleCollection(sourceProperty, foundCollection, compilation, out newNamespace);

        return HandleObject(sourceProperty, compilation, out newNamespace);
    }

    static ReferencePropertyToken.NestedObjectToken? HandleCollection(
            IPropertySymbol sourceProperty, 
            KeyValuePair<string, Func<ReferencePropertyToken, string, string>> allowedCollection, 
            Compilation compilation,
            out string? newNamespace
        )
    {
        // Get the TypeSymbol from the source's Type argument.
        // Right now I don't have a way to secure that I get the correct TypeSymbol, so this is the way to go
        INamedTypeSymbol? targetAsINamedTypeSymbol = sourceProperty.Type.AllInterfaces.FirstOrDefault(x => x.TypeArguments.Any())?.TypeArguments.FirstOrDefault() as INamedTypeSymbol;
        
        ClassToken? classToken = ClassHelper.GenerateClassToken(targetAsINamedTypeSymbol, compilation);
        
        // If it could not find a suitable MethodToken, then it should just default to IEnumerable.
        MethodToken? methodToken = classToken is null ? null : classToken.Methods.FirstOrDefault(x => x.GetReferenceSourceName().Equals(allowedCollection.Key)) ?? 
                                                               classToken.Methods.FirstOrDefault(x => x.Type is MethodType.Linq);

        if (methodToken is not null && classToken is not null)
            newNamespace = classToken.Namespace;
        else
            newNamespace = null;

        return methodToken is null ? null : new ReferencePropertyToken.NestedObjectToken(methodToken, allowedCollection);
    }

    static ReferencePropertyToken.NestedObjectToken? HandleObject(IPropertySymbol sourceProperty, Compilation compilation, out string? newNamespace)
    {
        ClassToken? classToken = ClassHelper.GenerateClassToken(sourceProperty.Type as INamedTypeSymbol, compilation);

        MethodToken? methodToken = classToken?.Methods.FirstOrDefault(x => x.Type is MethodType.Standard);

        if (methodToken is not null && classToken is not null)
            newNamespace = classToken.Namespace;
        else
            newNamespace = null;

        return methodToken is null ? null : new ReferencePropertyToken.NestedObjectToken(methodToken, new KeyValuePair<string, Func<ReferencePropertyToken, string, string>>(string.Empty, ObjectMapping));
    }
    
    static string ObjectMapping(ReferencePropertyToken token, string sourceReference) => $"{sourceReference}.{token.SourceProperty.Name}.{token.NestedObject!.MethodToken.Name}()";
}