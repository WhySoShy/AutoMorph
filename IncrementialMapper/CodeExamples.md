### Creating mappers as Extension methods
```csharp
[SGMapper(typeof(ExampleTarget))]
public class Example
{
    public string Hello { get; set; } // Auto targets the property ExampleTarget.Hello
    
    [SGProperty(nameof(ExampleTarget.RandomName))]
    public string NotFoundName { get; set; } // This targets the ExampleTarget.RandomName because of the attached attribute.

    [ExcludeProperty]
    public string DontInclude { get; set; } // This will be excluded because of the attached attribute.
}

public class ExampleTarget
{
    public string Hello { get; set; } // This will be mapped to Example.Hello
    
    public string RandomName { get; set; } // This will be mapped to Example.NotFoundName
    
    [ExcludeProperty]
    public string DontInclude { get; set; } // This will be excluded because of the attached attribute.
}
```
#### Outcome
```csharp
public static class GeneratedMapper_ExampleToExampleTarget
{
    public static global::Examples.ExampleTarget MapToExampleTarget(this global::Examples.Example source)
    {
        return new global::Examples.ExampleTarget()
        {
            Hello = source.Hello,
            RandomName = source.NotFoundName
        };
    }
}
```

### Creating mappers as methods injected into partial class
If this is done, the generator will not generate **ANY** methods that may have been marked with an Include method.
````csharp
[SGMapper(typeof(ExampleTarget))]
public partial class Example
{
    public string Hello { get; set; } // Auto targets the property ExampleTarget.Hello
    
    [SGProperty(nameof(ExampleTarget.RandomName))]
    public string NotFoundName { get; set; } // This targets the ExampleTarget.RandomName because of the attached attribute.
}

public class ExampleTarget
{
    public string Hello { get; set; } // This will be mapped to Example.Hello
    
    public string RandomName { get; set; } // This will be mapped to Example.NotFoundName
}
````

#### Outcome
````csharp
public partial class Example
{
    public global::IncrementialMapper.Test.Examples.ExampleTarget MapToExampleTarget(global::IncrementialMapper.Test.Examples.Example source)
    {
        return new global::IncrementialMapper.Test.Examples.ExampleTarget()
        {
            Hello = source.Hello,
            RandomName = source.NotFoundName
        };
    }
}
````

#### 