using System;
using System.Diagnostics;
using IncrementialMapper.Attributes.Base;

namespace IncrementialMapper.Attributes.Includes;

/// <summary>
/// <para>
///     When this has been added on a class, struct or record it tells the SG to include
///     collection-collection object mapper that maps every object inside the collection.
/// </para>
/// </summary>
[AttributeUsage(AttributeTargets.Class | AttributeTargets.Struct | AttributeTargets.Method)]
[Conditional("EXCLUDE_RUNTIME")]
public class IncludeLinq(string? key = null) : BaseAttribute(key);