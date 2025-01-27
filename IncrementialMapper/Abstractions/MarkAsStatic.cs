using System;
using IncrementialMapper.Abstractions.Base;

namespace IncrementialMapper.Abstractions;

/// <summary>
/// <para>
///     Forces the generated class of the marked class, struct or record to be generated as a static class with the use of extension methods. 
/// </para>
/// </summary>
/// <param name="key"></param>
[AttributeUsage(AttributeTargets.Class | AttributeTargets.Struct)]
public class MarkAsStatic(string? key = null) : BaseAttribute(key);