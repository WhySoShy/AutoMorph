﻿using IncrementialMapper.Abstractions.Attributes;
using IncrementialMapper.Abstractions.Enums;

#pragma warning disable CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider declaring as nullable.

namespace IncrementialMapper.Test.Examples;

[Mapper<ExampleTarget>]
[Include<IExample>(MapperType.Linq)]
[Include(MapperType.IQueryable)]
public class Example
{
    public string Hello { get; set; } // Auto targets the property ExampleTarget.Hello
    
    [Property(nameof(ExampleTarget.Huh))]
    public string NewTarget { get; set; }
}

public class ExampleTarget
{
    public string Hello { get; set; }
    public string Huh { get; set; }
}

public interface IExample
{
    public string Hello { get; set; }
}