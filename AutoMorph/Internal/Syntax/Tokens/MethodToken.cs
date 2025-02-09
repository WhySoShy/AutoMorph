using System.Collections.Generic;
using System.Linq;
using AutoMorph.Abstractions.Enums;
using AutoMorph.Internal.Syntax.Kinds;
using AutoMorph.Internal.Syntax.Types;

namespace AutoMorph.Internal.Syntax.Tokens;

/// <summary>
/// A token that represents a mapper method, that needs to be generated. 
/// </summary>
internal sealed partial record MethodToken(
        ModifierKind[] Modifiers,
        MethodType Type,
        string Name
    )
{
    /// <summary>
    /// What the generator should create the mappers to map from.
    /// </summary>
    public ReferenceClassToken SourceClass { get; init; }
    
    /// <summary>
    /// What the generator should create the mappers to map to.
    /// </summary>
    internal ReferenceClassToken TargetClass { get; init; }
    
    internal GenericType? Generic { get; set; }
    
    /// <summary>
    /// Used to determine how type casting will be done within the method.
    /// </summary>
    internal bool IsExpressionTree { get; init; }
    
    /// <summary>
    /// Used to determine how the properties are being processed.
    /// </summary>
    internal MappingStrategy MappingStrategy { get; init; }
    
    /// <summary>
    /// <para>
    ///     Contains the properties that should be mapped, and their target property. <br />
    ///     This is being used by all the methods.
    /// </para>
    /// </summary>
    internal HashSet<ReferencePropertyToken> Properties { get; set; } = [];

    internal record GenericType(string ConstraintTypeName);
}