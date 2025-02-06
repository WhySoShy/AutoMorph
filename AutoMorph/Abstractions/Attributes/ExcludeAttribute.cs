using System;
using System.Diagnostics;
using System.Reflection;
using AutoMorph.Internal;
using AutoMorph.Internal.Constants;

namespace AutoMorph.Abstractions.Attributes;

/// <summary>
/// Attribute works differently, when attached to class | struct and properties. <br />
/// Check documentation on how it works.
/// </summary>
[AttributeUsage(AttributeTargets.Property)]
[Conditional(AssemblyConstants.EXCLUDED_CONDITIONAL_NAME)]
public class ExcludeAttribute : Attribute, IExcludeAttribute
{
    public string? Key { get; set; }
}

/// <summary>
/// Has no effect in the user code, only used to refer to the attribute in the generator.
/// </summary>
internal interface IExcludeAttribute;