using IncrementialMapper.Attributes;
using IncrementialMapper.Attributes.Excludes;
using IncrementialMapper.Attributes.Includes;

namespace IncrementialMapper.Test.Examples;

[SGMapper(typeof(ExampleTarget))]
[IncludeLinq]
[MarkAsStatic]
public partial class Example
{
    public string Hello { get; set; } // Auto targets the property ExampleTarget.Hello
    
    [SGProperty(nameof(ExampleTarget.RandomName))]
    public string NotFoundName { get; set; } // This targets the ExampleTarget.RandomName because of the attached attribute.
    
    public List<IDK> Idk { get; set; }
    public IDK[] Idk2 { get; set; }
}

public class ExampleTarget
{
    public string Hello { get; set; } // This will be mapped to Example.Hello
    
    public string RandomName { get; set; } // This will be mapped to Example.NotFoundName
    
    public List<IDK> Idk { get; set; }
    public IDK[] Idk2 { get; set; }
}

public class IDK
{
    public string Okay { get; set; }
}