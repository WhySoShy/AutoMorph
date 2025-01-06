namespace IncrementialMapper.Syntax.Tokens;

internal record ReferenceClassToken(string Name, string FullPath)
{
    internal string Name { get; } = Name;
    
    internal string FullPath { get; } = FullPath;
}