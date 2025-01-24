using System;
using System.Diagnostics;
using IncrementialMapper.Attributes.Base;
using IncrementialMapper.Constants;

namespace IncrementialMapper.Attributes.Includes;

/// <summary>
/// <para>
///     Forces the generated class of the marked class, struct or record to be generated as a static class with the use of extension methods. 
/// </para>
/// </summary>
/// <param name="key"></param>
[AttributeUsage(AttributeTargets.Class | AttributeTargets.Struct)]
public class MarkAsStatic(string? key = null) : BaseAttribute(key);