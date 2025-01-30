using System;

namespace IncrementialMapper.Abstractions.Attributes;

[AttributeUsage(AttributeTargets.Property)]
public class PropertyAttribute(string nameOfTargetProperty) : Attribute;
