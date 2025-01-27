using System;
using System.Diagnostics;
using IncrementialMapper.Abstractions.Base;

namespace IncrementialMapper.Abstractions;

/// <summary>
/// <para>
///     When this has been added on a class, struct or record it tells the SG to include
///     IQueryable-IQueryable object mapper that maps every object inside the IQueryable.
/// </para>
/// </summary>
[AttributeUsage(AttributeTargets.Class | AttributeTargets.Struct | AttributeTargets.Method)]
public class IncludeIQueryable(string? key = null) : BaseAttribute(key);