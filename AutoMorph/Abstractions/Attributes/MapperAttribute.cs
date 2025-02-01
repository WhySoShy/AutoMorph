using System;
using AutoMorph.Internal;

namespace AutoMorph.Abstractions.Attributes;

/// <summary>
/// 
/// </summary>
/// <typeparam name="TTarget">Target class or interface.</typeparam>
[AttributeUsage(AttributeTargets.Class | AttributeTargets.Struct | AttributeTargets.Interface, AllowMultiple = true)]
public class MapperAttribute<TTarget> : Attribute, IAttribute, IMapperAttribute
{
    public string? Key { get; set; }
    
    /// <summary>
    /// The default name that will be applied to each generated mapper, if the mapper is not a partial mapper.  
    /// </summary>
    public string? DefaultMapperName { get; set; }
}

/// <summary>
/// This class is only used to Reference <see cref="MapperAttribute{TTarget}"/>
/// </summary>
internal abstract class MapperAttribute;

/// <summary>
/// Has no effect in the user code, only used to refer to the attribute in the generator.
/// </summary>
internal interface IMapperAttribute;