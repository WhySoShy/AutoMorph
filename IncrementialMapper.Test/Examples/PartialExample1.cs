﻿using IncrementialMapper.Abstractions.Attributes;
using IncrementialMapper.Abstractions.Enums;

#pragma warning disable CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider declaring as nullable.

namespace IncrementialMapper.Test.Examples;

[SGMapper(typeof(ExampleTarget))]
[Include(MapperType.Linq)]
public partial class Example
{
    [Include(MapperType.IQueryable)]
    public partial IQueryable<ExampleTarget> MapperTarget(IQueryable<Example> source);

    public string Hello { get; set; } // Auto targets the property ExampleTarget.Hello
    
    [SGProperty(nameof(ExampleTarget.Huh))]
    public string NewTarget { get; set; }
}

public class ExampleTarget
{
    public string Hello { get; set; }
    public string Huh { get; set; }
}