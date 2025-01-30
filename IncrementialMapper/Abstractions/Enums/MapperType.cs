namespace AutoMorph.Abstractions.Enums;

public enum MapperType
{
    /// <summary>
    /// This type should not be used, because the attached object will be ignored. 
    /// </summary>
    None,
    Linq,
    IQueryable,
}