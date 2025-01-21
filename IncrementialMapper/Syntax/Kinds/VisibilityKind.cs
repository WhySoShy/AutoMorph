namespace IncrementialMapper.Syntax.Kinds;

internal enum VisibilityKind
{
    Public,
    Private,
    Protected,
    Internal,
    
    // Don't know if these are being supported.
    Protected_Private,
    Protected_Internal
}