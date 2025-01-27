using System;
using System.Diagnostics;
using IncrementialMapper.Abstractions.Base;

namespace IncrementialMapper.Abstractions;

/// <summary>
/// When this has been added on a class, struct or record it tells the SG to exclude the standard object-object mapper.
/// </summary>
[AttributeUsage(AttributeTargets.Class | AttributeTargets.Struct)]
public class ExcludeStandard(string? key = null) : BaseAttribute(key);