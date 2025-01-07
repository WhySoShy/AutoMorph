using IncrementialMapper.Attributes;
using IncrementialMapper.Attributes.Excludes;

namespace IncrementialMapper.Test;

[SGMapper(typeof(ATargetClass))]
public class ASourceClass
{
    [SGProperty(nameof(ATargetClass.ThisCanBeReachedFromSource))]
    public string ThisCanReachTarget { get; set; }

    // This will be auto found, beacuse the target class has a Property with the exact name.
    public string Hello { get; set; }
    
    // This will be excluded beacuse it was never found inside the target class.
    public string ThisWillbeExcluded { get; set; }
}

public class ATargetClass
{
    [ExcludeProperty]
    public string ThisCanBeReachedFromSource { get; set; }
    
    public string Hello { get; set; }
}