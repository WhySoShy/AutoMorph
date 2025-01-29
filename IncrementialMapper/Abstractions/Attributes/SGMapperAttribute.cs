using System;
using System.Diagnostics;

namespace IncrementialMapper.Abstractions.Attributes;

[AttributeUsage(AttributeTargets.Class | AttributeTargets.Struct, AllowMultiple = true)]
[Conditional("EXCLUDE_RUNTIME")]
public class SGMapperAttribute(
        Type targetType
    ) : Attribute
{
    internal Type TargetClassType { get; } = targetType;
}