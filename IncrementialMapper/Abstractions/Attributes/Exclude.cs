using System;
using IncrementialMapper.Internal;

namespace IncrementialMapper.Abstractions.Attributes;

/// <summary>
/// Attribute works differently, when attached to class | struct and properties. <br />
/// Check documentation on how it works.
/// </summary>
[AttributeUsage(AttributeTargets.Property | AttributeTargets.Class | AttributeTargets.Struct)]
public class Exclude : Attribute, IAttribute
{
    public string? Key { get; set; }
}