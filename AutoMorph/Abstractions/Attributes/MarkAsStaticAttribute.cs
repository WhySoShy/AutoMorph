using System;
using System.Diagnostics;
using AutoMorph.Internal;
using AutoMorph.Internal.Constants;

namespace AutoMorph.Abstractions.Attributes;

/// <summary>
/// <para>
///     Forces the generated class of the marked class, struct or record to be generated as a static class with the use of extension methods. 
/// </para>
/// </summary>
[AttributeUsage(AttributeTargets.Class | AttributeTargets.Struct)]
[Conditional(AssemblyConstants.EXCLUDED_CONDITIONAL_NAME)]
public class MarkAsStaticAttribute : Attribute, IAttribute, IMarkAsStaticAttribute
{
    string? _key;
    /// <summary>
    /// Not yet implemented.
    /// </summary>
    public string? Key
    {
        get => _key; 
        set => throw new NotImplementedException("There is no support for force marking mapper classes as static, yet.");
    }
}

internal interface IMarkAsStaticAttribute;