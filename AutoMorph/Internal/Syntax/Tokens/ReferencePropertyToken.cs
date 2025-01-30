using System;
using System.Collections.Generic;
using AutoMorph.Internal.Syntax.Kinds;

namespace AutoMorph.Internal.Syntax.Tokens;

internal record ReferencePropertyToken(
        ReferencePropertyToken.Property SourceProperty,
        ReferencePropertyToken.Property TargetProperty
    )
{
    internal Property SourceProperty { get; } = SourceProperty;
    internal Property TargetProperty { get; } = TargetProperty;
    
    /// <summary>
    /// If the <see cref="SourceProperty"/> is an object, it should create a new mapper, that equals that type and append the mapper into <see cref="NestedObjectMapperMethod"/> for use when generating.
    /// </summary>
    internal NestedObjectToken? NestedObject { get; set; }
    
    internal record NestedObjectToken(MethodToken MethodToken, KeyValuePair<string, Func<ReferencePropertyToken, string, string>> Type)
    {
        internal MethodToken MethodToken { get; } = MethodToken;

        /// <summary>
        /// Name of the object or collection, ex -> <c>System.Collections.Generic.Queue&lt;T&gt;</c>
        /// </summary>
        internal KeyValuePair<string, Func<ReferencePropertyToken, string, string>> Type { get; } = Type;
    }

    internal record Property(string Name, string ValueType, CastingKind Casting)
    {
        internal string Name { get; } = Name;

        /// <summary>
        /// The type of the property as a string.
        /// </summary>
        internal string ValueType { get; } = ValueType;

        internal CastingKind Casting { get; } = Casting;
        
        internal bool AllowedAsNull { get; }
    }
}