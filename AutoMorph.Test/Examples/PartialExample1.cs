using AutoMorph.Abstractions.Attributes;
using AutoMorph.Abstractions.Enums;

#pragma warning disable CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider declaring as nullable.

namespace AutoMorph.Test.Examples;

[Mapper<ExampleTarget>]
[Include(MapperType.Linq)]
[Include(MapperType.IQueryable)]
[Exclude]
public class Example
{
    public string Hello { get; set; }
    
    [Property(nameof(ExampleTarget.IntTarget))]
    public decimal SourceDecimal { get; set; } // Auto targets the property ExampleTarget.Hello
    
    [Property(nameof(ExampleTarget.DecimalTarget))]
    public int IntSource { get; set; }
    
    [Property(nameof(ExampleTarget.StringTarget))]
    public decimal DecimalSource { get; set; }
}

public class ExampleTarget
{
    public int IntTarget { get; set; }
    public decimal DecimalTarget { get; set; }
    
    public string StringTarget { get; set; }
}

public interface IExample
{
    public string Hello { get; set; }
}