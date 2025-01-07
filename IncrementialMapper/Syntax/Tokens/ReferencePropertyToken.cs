using IncrementialMapper.Syntax.Kinds;

namespace IncrementialMapper.Syntax.Tokens;

internal record ReferencePropertyToken(
        VisibilityKind VisibilityKind,
        string SourcePropertyName,
        string TargetPropertyName
    )
{
    public string SourcePropertyName { get; } = SourcePropertyName;
    public string TargetPropertyName { get; } = TargetPropertyName;
    
    // Unsure if i even need this..
    public VisibilityKind VisibilityKind { get; } = VisibilityKind;
}