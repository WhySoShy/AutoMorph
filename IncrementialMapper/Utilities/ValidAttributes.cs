using System.Collections.Generic;
using IncrementialMapper.Attributes.Includes;
using IncrementialMapper.Syntax.Kinds;

namespace IncrementialMapper.Utilities;

internal static class ValidAttributes
{
    public static readonly Dictionary<string, MethodKind?> ValidIncludeAttributes = new()
    {
        { typeof(IncludeLinq).FullName, MethodKind.Linq },
        { typeof(IncludeIQueryable).FullName, MethodKind.Linq_IQueryable }
    };

    public const string INCLUDE_ATTRIBUTE_NAMESPACE = "IncrementialMapper.Attributes.Includes";
}