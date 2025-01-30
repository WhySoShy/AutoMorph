using System;

namespace AutoMorph.Abstractions.Attributes;

[AttributeUsage(AttributeTargets.Property)]
public class PropertyAttribute(string nameOfTargetProperty) : Attribute;
