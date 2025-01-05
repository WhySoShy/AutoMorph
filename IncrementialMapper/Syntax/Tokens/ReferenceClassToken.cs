namespace IncrementialMapper.Syntax.Tokens;

internal record ReferenceClassToken(string Name, string FullPath, bool IsPartial)
{
    internal string Name { get; } = Name;
    
    internal string FullPath { get; } = FullPath;

    internal bool IsPartial { get; } = IsPartial;
}