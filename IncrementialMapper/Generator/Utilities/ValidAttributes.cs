using System.Collections.Generic;
using IncrementialMapper.Abstractions;
using IncrementialMapper.Syntax.Kinds;

namespace IncrementialMapper.Generator.Utilities;

internal static class ValidAttributes
{
    internal static readonly Dictionary<string, MethodKind?> ValidIncludeAttributes = new()
    {
        { typeof(IncludeLinq).FullName, MethodKind.Linq },
        { typeof(IncludeIQueryable).FullName, MethodKind.Linq_IQueryable }
    };

    public const string INCLUDE_ATTRIBUTE_NAMESPACE = "IncrementialMapper.Attributes.Includes";
}