using System;
using System.Diagnostics;

namespace IncrementialMapper.Attributes.Includes;

/// <summary>
/// <para>
///     When this has been added on a class, struct or record it tells the SG to include
///     collection-collection object mapper that maps every object inside the collection.
/// </para>
/// </summary>
[AttributeUsage(AttributeTargets.Class | AttributeTargets.Struct)]
[Conditional("EXCLUDE_RUNTIME")]
public class IncludeLinq : Attribute
{

}