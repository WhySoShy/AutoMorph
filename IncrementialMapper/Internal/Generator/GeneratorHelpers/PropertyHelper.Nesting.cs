﻿using System;
using System.Collections.Generic;
using System.Linq;
using IncrementialMapper.Internal.Generator.Validators;
using IncrementialMapper.Internal.Syntax.Kinds;
using IncrementialMapper.Internal.Syntax.Tokens;
using IncrementialMapper.Internal.Syntax.Types;
using Microsoft.CodeAnalysis;

namespace IncrementialMapper.Internal.Generator.GeneratorHelpers;

public static partial class PropertyHelper
{
    /// <summary>
    /// Used to handle nesting in properties, if any objects needs a mapper attached to them, they will get created here.
    /// </summary>
    static ReferencePropertyToken.NestedObjectToken? GetNestedPropertyTokens(IPropertySymbol sourceProperty, out string? newNamespace)
    {
        newNamespace = null;

        // Just return an empty object, if it is not an object or collection.
        if (!(sourceProperty.Type.TypeKind is TypeKind.Class or TypeKind.Struct or TypeKind.Array &&
              sourceProperty.Type.SpecialType is SpecialType.None))
            return null;
        
        string name = sourceProperty.Type is IArrayTypeSymbol ? ValidCollections.ARRAY_CUSTOM : sourceProperty.Type.OriginalDefinition.ToDisplayString();

        // Check if the collection type is supported by the system.
        if (ValidCollections.SupportedCollections.FirstOrDefault(x
                    => x.Key == name) is
                { Key: not null } foundCollection)
            return HandleCollection(sourceProperty, foundCollection, out newNamespace);

        return HandleObject(sourceProperty, out newNamespace);
    }

    static ReferencePropertyToken.NestedObjectToken? HandleCollection(
            IPropertySymbol sourceProperty, 
            KeyValuePair<string, Func<ReferencePropertyToken, string, string>> allowedCollection, 
            out string? newNamespace
        )
    {
        // Get the TypeSymbol from the source's Type argument.
        // Right now I don't have a way to secure that I get the correct TypeSymbol, so this is the way to go
        INamedTypeSymbol? targetAsINamedTypeSymbol = sourceProperty.Type.AllInterfaces.FirstOrDefault(x => x.TypeArguments.Any())?.TypeArguments.FirstOrDefault() as INamedTypeSymbol;
        
        ClassToken? classToken = ClassHelper.GenerateClassToken(targetAsINamedTypeSymbol);
        
        // If it could not find a suitable MethodToken, then it should just default to IEnumerable.
        MethodToken? methodToken = classToken is null ? null : classToken.Methods.FirstOrDefault(x => x.GetReferenceSourceName().Equals(allowedCollection.Key)) ?? 
                                                               classToken.Methods.FirstOrDefault(x => x.Type is MethodType.Linq);

        if (methodToken is not null && classToken is not null)
            newNamespace = classToken.Namespace;
        else
            newNamespace = null;

        return methodToken is null ? null : new ReferencePropertyToken.NestedObjectToken(methodToken, allowedCollection, PropertyKind.Collection);
    }

    static ReferencePropertyToken.NestedObjectToken? HandleObject(IPropertySymbol sourceProperty, out string? newNamespace)
    {
        ClassToken? classToken = ClassHelper.GenerateClassToken(sourceProperty.Type as INamedTypeSymbol);

        MethodToken? methodToken = classToken?.Methods.FirstOrDefault(x => x.Type is MethodType.Standard);

        if (methodToken is not null && classToken is not null)
            newNamespace = classToken.Namespace;
        else
            newNamespace = null;

        string ObjectMapping(ReferencePropertyToken token, string sourceReference) => $"{sourceReference}.{token.SourcePropertyName}.{token.NestedObject!.MethodToken.Name}()";

        return methodToken is null ? null : new ReferencePropertyToken.NestedObjectToken(methodToken, new KeyValuePair<string, Func<ReferencePropertyToken, string, string>>(string.Empty, ObjectMapping), PropertyKind.Object);
    }
}