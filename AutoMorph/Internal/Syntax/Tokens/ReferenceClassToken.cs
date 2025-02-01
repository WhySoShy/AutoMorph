namespace AutoMorph.Internal.Syntax.Tokens;

/// <summary>
/// Reference to a class, with the name and Fully qualified name.
/// </summary>
internal record ReferenceClassToken(string Name, string FullPath, bool CanCreateInstance)
{
    internal string Name { get; } = Name;
    
    internal string FullPath { get; } = FullPath;
    
    internal bool CanCreateInstance { get; } = CanCreateInstance;
}