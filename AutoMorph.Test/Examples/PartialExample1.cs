using AutoMorph.Abstractions.Attributes;
using AutoMorph.Abstractions.Enums;

#pragma warning disable CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider declaring as nullable.

namespace AutoMorph.Test.Examples;

[Mapper]
[Include<TargetClass>(MapperType.Standard, Key = "standardMapper")]
[Include<TargetClass>(MapperType.Standard, Key = "linqMapper", MapperName = "StandardMapper2")]
public class SourceClass
{
    public string Name { get; set; }
    
    [Property(nameof(TargetClass.PersonAge), Key = "linqMapper")]
    [Exclude]
    public int Age { get; set; }
}

public class TargetClass 
{
    public string Name { get; set; }
    public int PersonAge { get; set; }
    public int Age { get; set; }
}