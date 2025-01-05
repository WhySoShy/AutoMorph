using System;
using System.Diagnostics;

namespace IncrementialMapper.Attributes.Excludes;

/// <summary>
/// When this has been added on a class, struct or record it tells the SG to exclude the standard object-object mapper.
/// </summary>
[AttributeUsage(AttributeTargets.Class | AttributeTargets.Struct)]
[Conditional("EXCLUDE_RUNTIME")]
public class ExcludeStandard : Attribute
{
    
}