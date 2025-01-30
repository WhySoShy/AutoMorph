using AutoMorph.Abstractions.Attributes;
using AutoMorph.Abstractions.Enums;

#pragma warning disable CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider declaring as nullable.

namespace IncrementialMapper.Test.Examples;

[Mapper<ExampleTarget>]
[Include<IExample>(MapperType.Linq)]
[Include(MapperType.IQueryable)]
public class Example
{
    public decimal Hello { get; set; } // Auto targets the property ExampleTarget.Hello
    
    [Property(nameof(ExampleTarget.Huh))]
    public string NewTarget { get; set; }
    
    public decimal Hello2 { get; set; }
    public string NewTarget2 { get; set; }
}

public class ExampleTarget
{
    public int Hello { get; set; }
    public decimal Huh { get; set; }
    public decimal Hello2 { get; set; }
    public string NewTarget2 { get; set; }
}

public interface IExample
{
    public string Hello { get; set; }
}