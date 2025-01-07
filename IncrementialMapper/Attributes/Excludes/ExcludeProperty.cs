using System;
using System.Diagnostics;

namespace IncrementialMapper.Attributes.Excludes;

[AttributeUsage(AttributeTargets.Property)]
[Conditional("EXCLUDE_RUNTIME")]
public class ExcludeProperty : Attribute
{
}
