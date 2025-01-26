using IncrementialMapper.Syntax.Kinds;
using System.CodeDom.Compiler;
using System.Collections.Generic;
using System.IO;

namespace IncrementialMapper.Syntax.Tokens;

internal sealed record ClassToken
{
    /// <summary>
    /// What the generator should create the mappers to map from.
    /// </summary>
    public ReferenceClassToken SourceClass { get; set; }

    /// <summary>
    /// What the generator should create the mappers to map to.
    /// </summary>
    public ReferenceClassToken TargetClass { get; set; }

    /// <summary>
    /// The namespace that the mapper should be generated into.
    /// </summary>
    public string? Namespace { get; set; }

    /// <summary>
    /// Class visibility, this defaults to public if null.
    /// </summary>
    public VisibilityKind? Visibility { get; set; }

    /// <summary>
    /// <para>
    ///     Modifiers that should be added to the class. <br />
    ///     Auto orders, to keep the 'heaviest' modifiers first,
    ///     this just means that it appears in the correct order when assembling the code. 
    /// </para>
    /// </summary>
    public List<ModifierKind> Modifiers { get; set; } = [];

    /// <summary>
    /// <para>
    ///     Contains the properties that should be mapped, and their target property. <br />
    ///     This is being used by all the methods.
    /// </para>
    /// </summary>
    public HashSet<ReferencePropertyToken> Properties { get; set; } = [];

    /// <summary>
    /// <para>
    ///     Contains what method types should be generated.
    /// </para>
    /// </summary>
    public List<MethodToken> Methods { get; set; } = [];

    /// <summary>
    /// Namespaces that the class needs to function.
    /// </summary>
    public HashSet<string> NameSpaces { get; set; } = [];

    /// <summary>
    /// Writer that is being used to generate each file.
    /// </summary>
    internal IndentedTextWriter Writer { get; } = new IndentedTextWriter(new StringWriter());
}