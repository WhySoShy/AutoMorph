using System;
using System.Collections.Generic;
using AutoMorph.Internal.Syntax.Kinds;

namespace AutoMorph.Internal.Syntax.Tokens;

internal record ReferencePropertyToken(
        ReferencePropertyToken.Property SourceProperty,
        ReferencePropertyToken.Property TargetProperty
    )
{
    /// <summary>
    /// If the <see cref="SourceProperty"/> is an object, it should create a new mapper, that equals that type and append the mapper into <see cref="NestedObjectMapperMethod"/> for use when generating.
    /// </summary>
    internal NestedObjectToken? NestedObject { get; init; }

    internal record NestedObjectToken(
        MethodToken MethodToken,
        KeyValuePair<string, Func<ReferencePropertyToken, string, string>> Type
    );

    internal record Property(
        string Name, 
        string ValueType, 
        CastingKind Casting
    );
}