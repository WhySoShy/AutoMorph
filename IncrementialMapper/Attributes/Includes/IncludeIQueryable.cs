using System;
using System.Diagnostics;
using IncrementialMapper.Attributes.Base;

namespace IncrementialMapper.Attributes.Includes;

/// <summary>
/// <para>
///     When this has been added on a class, struct or record it tells the SG to include
///     IQueryable-IQueryable object mapper that maps every object inside the IQueryable.
/// </para>
/// </summary>
[AttributeUsage(AttributeTargets.Class | AttributeTargets.Struct | AttributeTargets.Method)]
[Conditional("EXCLUDE_RUNTIME")]
public class IncludeIQueryable(string? key = null) : BaseAttribute(key);