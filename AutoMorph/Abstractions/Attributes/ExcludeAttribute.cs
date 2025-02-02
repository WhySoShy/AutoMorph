using System;
using AutoMorph.Internal;

namespace AutoMorph.Abstractions.Attributes;

/// <summary>
/// Attribute works differently, when attached to class | struct and properties. <br />
/// Check documentation on how it works.
/// </summary>
[AttributeUsage(AttributeTargets.Property)]
public class ExcludeAttribute : Attribute, IAttribute;