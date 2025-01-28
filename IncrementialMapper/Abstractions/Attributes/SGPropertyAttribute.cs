using System;
using System.Diagnostics;

namespace IncrementialMapper.Abstractions.Attributes;

[AttributeUsage(AttributeTargets.Property)]
[Conditional("EXCLUDE_RUNTIME")]
public class SGPropertyAttribute(
        string nameOfTargetProperty
    ) : Attribute
{
    internal string NameOfTargetProperty { get; private set; } = nameOfTargetProperty;
}
