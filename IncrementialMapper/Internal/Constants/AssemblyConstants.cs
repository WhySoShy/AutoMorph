﻿namespace IncrementialMapper.Internal.Constants;

/// <summary>
/// This is constants that is being re-used within the whole assembly.
/// </summary>
internal static class AssemblyConstants
{
    internal const string EXCLUDED_CONDITIONAL_NAME = "EXCLUDE_RUNTIME";
    // The `1 means that the attribute contains 1 generic argument, adding that will fulfill the qualified name.
    internal const string SGMAPPER_FULL_QUALIFIED_METADATA_NAME = $"{FULLY_QUALIFIED_ATTRIBUTE_NAMESPACE}.SGMapperAttribute`1";
    internal const string FULLY_QUALIFIED_ATTRIBUTE_NAMESPACE = "IncrementialMapper.Abstractions.Attributes";
    internal const string DEFAULT_NAMESPACE = "IncrementialMapper.Generated.Mappers";
    internal const string GLOBAL_KEYWORD = "global::";
}