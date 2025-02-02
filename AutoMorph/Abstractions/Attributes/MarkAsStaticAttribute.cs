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
/// <param name="key"></param>
[AttributeUsage(AttributeTargets.Class | AttributeTargets.Struct)]
[Conditional(AssemblyConstants.EXCLUDED_CONDITIONAL_NAME)]
public class MarkAsStaticAttribute : Attribute, IAttribute, IMarkAsStaticAttribute
{
    public string? Key { get; set; }
}

internal interface IMarkAsStaticAttribute;