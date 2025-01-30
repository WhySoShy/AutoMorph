using System;
using AutoMorph.Internal;

namespace AutoMorph.Abstractions.Attributes;

/// <summary>
/// <para>
///     Forces the generated class of the marked class, struct or record to be generated as a static class with the use of extension methods. 
/// </para>
/// </summary>
/// <param name="key"></param>
[AttributeUsage(AttributeTargets.Class | AttributeTargets.Struct)]
public class MarkAsStatic : Attribute, IAttribute
{
    public string? Key { get; set; }
}