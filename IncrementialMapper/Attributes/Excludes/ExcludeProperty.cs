using System;
using System.Diagnostics;
using IncrementialMapper.Attributes.Base;

namespace IncrementialMapper.Attributes.Excludes;

[AttributeUsage(AttributeTargets.Property)]
[Conditional("EXCLUDE_RUNTIME")]
public class ExcludeProperty(string? key = null) : BaseAttribute(key);
