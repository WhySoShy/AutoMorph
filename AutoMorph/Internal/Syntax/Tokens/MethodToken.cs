using System.Collections.Generic;
using System.Linq;
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
    /// Name of the generated method.
    /// </summary>
    internal string Name { get; } = Name;
    
    /// <summary>
    /// What the generator should create the mappers to map to.
    /// </summary>
    internal ReferenceClassToken TargetClass { get; set; }
    
    /// <summary>
    /// <para>
    ///     Modifiers that should be added to the method. <br />
    ///     Auto orders, to keep the 'heaviest' modifiers first,
    ///     this just means that it appears in the correct order when assembling the code. 
    /// </para>
    /// </summary>
    internal ModifierKind[] Modifiers { get; } = Modifiers.OrderBy(x => x).ToArray();

    internal MethodType Type { get; } = Type;
    
    internal GenericType? Generic { get; set; }
    
    /// <summary>
    /// Used to determine how type casting will be done within the method.
    /// </summary>
    internal bool IsExpressionTree { get; set; }
    
    /// <summary>
    /// <para>
    ///     Contains the properties that should be mapped, and their target property. <br />
    ///     This is being used by all the methods.
    /// </para>
    /// </summary>
    internal HashSet<ReferencePropertyToken> Properties { get; set; } = [];

    internal record GenericType(string ConstraintTypeName)
    {
        /// <summary>
        /// The constraint that will be applied on the generated method.
        /// </summary>
        internal string ConstraintTypeName { get; } = ConstraintTypeName;
    }
}