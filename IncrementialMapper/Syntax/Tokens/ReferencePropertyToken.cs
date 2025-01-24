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
    /// The type of the source property.
    /// </summary>
    public PropertyKind SourcePropertyType { get; set; }
    
    /// <summary>
    /// If the <see cref="SourcePropertyName"/> is an object, it should create a new mapper, that equals that type and append the mapper into <see cref="NestedObjectMapperMethod"/> for use when generating.
    /// </summary>
    public string? NestedObjectMapperMethod { get; set; }
}