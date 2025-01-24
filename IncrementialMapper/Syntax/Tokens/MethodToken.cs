using System.Linq;
using IncrementialMapper.Syntax.Kinds;

namespace IncrementialMapper.Syntax.Tokens;

internal sealed record MethodToken(
        ModifierKind[] Modifiers,
        MethodKind Type,
        string Name
    )
{
    /*
     * Properties will be found inside the parent ClassToken.
     */

    /// <summary>
    /// Name of the generated method.
    /// </summary>
    public string Name { get; } = Name;
    
    public string? DataType { get; set; }
    
    /// <summary>
    /// <para>
    ///     Modifiers that should be added to the method. <br />
    ///     Auto orders, to keep the 'heaviest' modifiers first,
    ///     this just means that it appears in the correct order when assembling the code. 
    /// </para>
    /// </summary>
    public ModifierKind[] Modifiers { get; } = Modifiers.OrderBy(x => x).ToArray();

    public MethodKind Type { get; } = Type;
}