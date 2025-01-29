﻿using System.Linq;
using IncrementialMapper.Internal.Syntax.Kinds;
using IncrementialMapper.Internal.Syntax.Types;

namespace IncrementialMapper.Internal.Syntax.Tokens;

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
    
    public bool IsGeneric { get; set; }
}