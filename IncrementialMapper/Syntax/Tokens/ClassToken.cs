using System.CodeDom.Compiler;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using IncrementialMapper.Syntax.Kinds;

namespace IncrementialMapper.Syntax.Tokens;

internal sealed record ClassToken(
        ReferenceClassToken SourceClass,
        ReferenceClassToken TargetClass,
        HashSet<ReferencePropertyToken> Properties,
        HashSet<MethodToken> Methods,
        VisibilityKind? Visibility,
        ModifierKind[] Modifiers
    )
{
    /// <summary>
    /// What the generator should create the mappers to map from.
    /// </summary>
    public ReferenceClassToken SourceClass { get; } = SourceClass;
    
    /// <summary>
    /// What the generator should create the mappers to map to.
    /// </summary>
    public ReferenceClassToken TargetClass { get; } = TargetClass;
    
    /// <summary>
    /// Class visibility, this defaults to public if null.
    /// </summary>
    public VisibilityKind? Visibility { get; } = Visibility ?? Kinds.VisibilityKind.Public;

    /// <summary>
    /// <para>
    ///     Modifiers that should be added to the class. <br />
    ///     Auto orders, to keep the 'heaviest' modifiers first,
    ///     this just means that it appears in the correct order when assembling the code. 
    /// </para>
    /// </summary>
    public ModifierKind[] Modifiers { get; } = Modifiers.OrderBy(x => x).ToArray();

    /// <summary>
    /// <para>
    ///     Contains the properties that should be mapped, and their target property. <br />
    ///     This is being used by all the methods.
    /// </para>
    /// </summary>
    public HashSet<ReferencePropertyToken> Properties { get; } = Properties;
    
    /// <summary>
    /// <para>
    ///     Contains what method types should be generated.
    /// </para>
    /// </summary>
    public HashSet<MethodToken> Methods { get; } = Methods;
    
    /// <summary>
    /// Writer that is being used to generate each file.
    /// </summary>
    internal IndentedTextWriter Writer { get; } = new IndentedTextWriter(new StringWriter());
}