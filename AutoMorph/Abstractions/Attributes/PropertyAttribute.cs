using System;
#pragma warning disable CS9113 // Parameter is unread.

namespace AutoMorph.Abstractions.Attributes;

[AttributeUsage(AttributeTargets.Property)]
public class PropertyAttribute(string nameOfTargetProperty) : Attribute;
