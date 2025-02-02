using System;
using System.Diagnostics;
using AutoMorph.Internal;
using AutoMorph.Internal.Constants;

#pragma warning disable CS9113 // Parameter is unread.

namespace AutoMorph.Abstractions.Attributes;

[AttributeUsage(AttributeTargets.Property)]
[Conditional(AssemblyConstants.EXCLUDED_CONDITIONAL_NAME)]
public class PropertyAttribute(string nameOfTargetProperty) : Attribute, IAttribute, IPropertyAttribute
{
    public string? Key { get; set; }
}

/// <summary>
/// Has no effect in the user code, only used to refer to the attribute in the generator.
/// </summary>
internal interface IPropertyAttribute;
