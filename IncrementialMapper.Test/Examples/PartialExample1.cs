using IncrementialMapper.Attributes;
using IncrementialMapper.Attributes.Excludes;
using IncrementialMapper.Attributes.Includes;

namespace IncrementialMapper.Test.Examples;

// Define what class you want to map to.
[SGMapper(typeof(PartialExample1Target))]
// This Auto-Includes the Linq  mapper, with a name given by the generator.
[IncludeLinq] 
// This could be an DTO for example.
public partial class PartialExample1
{
    // This is a way to specify the method, if you want to customize the names of the mapper. And gives more control for it.
    // But make sure that the return types match.
    [IncludeIQueryable]
    public partial IQueryable<PartialExample1Target> MapToPartialExample1Target();

    // This specifically targets the property PropTarget1 of class PartialExample1Target.
    [SGProperty(nameof(PartialExample1Target.PropTarget1))]
    public string Prop1 { get; set; } = string.Empty;
    
    // This auto targets PropTarget2 of class PartialExample1Target, because they share the same name.
    public string PropTarget2 { get; set; } = string.Empty;
    
    // Even though PartialExample1Target, contains a property name PropTarget3 it will not be included because it has been marked as excluded.
    [ExcludeProperty]
    public string PropTarget3 { get; set; } = string.Empty;
}


// This could be an DB Entity for example.
public class PartialExample1Target
{
    public string PropTarget1 { get; set; } = string.Empty;
    public string PropTarget2 { get; set; } = string.Empty;
    public string PropTarget3 { get; set; } = string.Empty;
}

// This is just to extend the partial method and remove the error.
public partial class PartialExample1
{
    // This method is just here to extend the partial method.
    public partial IQueryable<PartialExample1Target> MapToPartialExample1Target()
    {
        throw new NotImplementedException();
    }
}