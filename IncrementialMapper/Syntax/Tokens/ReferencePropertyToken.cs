using System;
using System.Collections.Generic;
using IncrementialMapper.Syntax.Kinds;

namespace IncrementialMapper.Syntax.Tokens;

internal record ReferencePropertyToken(
        string SourcePropertyName,
        string TargetPropertyName
    )
{
    public string SourcePropertyName { get; } = SourcePropertyName;
    public string TargetPropertyName { get; } = TargetPropertyName;
    
    /// <summary>
    /// If the <see cref="SourcePropertyName"/> is an object, it should create a new mapper, that equals that type and append the mapper into <see cref="NestedObjectMapperMethod"/> for use when generating.
    /// </summary>
    public NestedObjectToken? NestedObject { get; set; }
    
    internal record NestedObjectToken(MethodToken MethodToken, KeyValuePair<string, Func<ReferencePropertyToken, string, string>> Type, PropertyTypeKind Kind)
    {
        public MethodToken MethodToken { get; } = MethodToken;

        /// <summary>
        /// Name of the object or collection, ex -> <c>System.Collections.Generic.Queue&lt;T&gt;</c>
        /// </summary>
        public KeyValuePair<string, Func<ReferencePropertyToken, string, string>> Type { get; } = Type;

        public PropertyTypeKind Kind { get; } = Kind;
    }
}