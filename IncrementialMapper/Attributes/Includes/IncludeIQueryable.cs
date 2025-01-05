using System;
using System.Diagnostics;

namespace IncrementialMapper.Attributes.Includes;

/// <summary>
/// <para>
///     When this has been added on a class, struct or record it tells the SG to include
///     IQueryable-IQueryable object mapper that maps every object inside the IQueryable.
/// </para>
/// </summary>
[AttributeUsage(AttributeTargets.Class | AttributeTargets.Struct)]
[Conditional("EXCLUDE_RUNTIME")]
public class IncludeIQueryable : Attribute
{
    
}