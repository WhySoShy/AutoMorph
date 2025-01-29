using System;
using System.Diagnostics;

namespace IncrementialMapper.Abstractions.Attributes;

[AttributeUsage(AttributeTargets.Property)]
public class SGPropertyAttribute(string nameOfTargetProperty) : Attribute;
