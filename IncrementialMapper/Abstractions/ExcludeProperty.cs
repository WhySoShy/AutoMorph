using System;
using System.Diagnostics;
using IncrementialMapper.Abstractions.Base;

namespace IncrementialMapper.Abstractions;

[AttributeUsage(AttributeTargets.Property)]
public class ExcludeProperty(string? key = null) : BaseAttribute(key);
