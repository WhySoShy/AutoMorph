using IncrementialMapper.Abstractions;
using IncrementialMapper.Abstractions.Attributes;
using IncrementialMapper.Abstractions.Enums;

// ReSharper disable CollectionNeverUpdated.Global
#pragma warning disable CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider declaring as nullable.

namespace IncrementialMapper.Test.Examples;

[SGMapper(typeof(ExampleTarget))]
[Include(MapperType.Linq)]
public class Example
{
    public string Hello { get; set; } // Auto targets the property ExampleTarget.Hello
}

public class ExampleTarget
{
    public string Hello { get; set; }
}