﻿using System.Collections.Generic;
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
    public string Name { get; } = Name;
    
    /// <summary>
    /// <para>
    ///     Modifiers that should be added to the method. <br />
    ///     Auto orders, to keep the 'heaviest' modifiers first,
    ///     this just means that it appears in the correct order when assembling the code. 
    /// </para>
    /// </summary>
    public ModifierKind[] Modifiers { get; } = Modifiers.OrderBy(x => x).ToArray();

    public MethodType Type { get; } = Type;
    
    public GenericType? Generic { get; set; }
    
    /// <summary>
    /// <para>
    ///     Contains the properties that should be mapped, and their target property. <br />
    ///     This is being used by all the methods.
    /// </para>
    /// </summary>
    public HashSet<ReferencePropertyToken> Properties { get; set; } = [];

    internal record GenericType(string TypeName)
    {
        public string TypeName { get; } = TypeName;
    }
}