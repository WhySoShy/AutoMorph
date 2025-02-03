using AutoMorph.Abstractions.Attributes;
using AutoMorph.Abstractions.Enums;

#pragma warning disable CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider declaring as nullable.

namespace AutoMorph.Test.Examples;

[Mapper]
[Include<ExampleTarget>(MapperType.Linq)]
public class Example
{
    public string Hello { get; set; }

    [Property(nameof(ExampleTarget.IntTarget), Key = "ExampleTarget")]
    public decimal SourceDecimal { get; set; }
}


public class ExampleTarget
{
    public string Hello { get; set; }

    public int IntTarget { get; set; }
    public decimal DecimalTarget { get; set; }
    
    public string StringTarget { get; set; }
}