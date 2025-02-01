using System;
using AutoMorph.Internal;

namespace AutoMorph.Abstractions.Attributes;

/// <summary>
/// Attribute works differently, when attached to class | struct and properties. <br />
/// Check documentation on how it works.
/// </summary>
[AttributeUsage(AttributeTargets.Property | AttributeTargets.Class | AttributeTargets.Struct | AttributeTargets.Interface)]
public class ExcludeAttribute : Attribute, IAttribute
{
    public string? Key { get; set; }
}