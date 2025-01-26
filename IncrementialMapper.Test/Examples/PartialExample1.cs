using IncrementialMapper.Attributes;
using IncrementialMapper.Attributes.Excludes;
using IncrementialMapper.Attributes.Includes;
// ReSharper disable CollectionNeverUpdated.Global
#pragma warning disable CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider declaring as nullable.

namespace IncrementialMapper.Test.Examples;

[SGMapper(typeof(ExampleTarget))]
[IncludeLinq]
// [MarkAsStatic]
public partial class Example
{
    public string Hello { get; set; } // Auto targets the property ExampleTarget.Hello
    
    [SGProperty(nameof(ExampleTarget.RandomName))]
    public string NotFoundName { get; set; } // This targets the ExampleTarget.RandomName because of the attached attribute.

    public List<NestedObject> ListMapping { get; set; }
    public NestedObject[] ArrayMapping { get; set; } = [];
    public NestedObject NestedObject { get; set; }
}

public class ExampleTarget
{
    public string Hello { get; set; } // This will be mapped to Example.Hello
    
    public string RandomName { get; set; } // This will be mapped to Example.NotFoundName

    public List<NestedObject2> ListMapping { get; set; } = [];
    public NestedObject2[] ArrayMapping { get; set; }
    public NestedObject2 NestedObject { get; set; }
}

[SGMapper(typeof(NestedObject2))]
[IncludeLinq]
public class NestedObject
{
    public string Okay { get; set; }
}

public class NestedObject2
{
    public string Okay { get; set; }
}