using System;
using IncrementialMapper.Attributes.Base;

namespace IncrementialMapper.Attributes.Excludes;

/// <summary>
/// <para>
///     Using this on a class, struct or record will make the generated class as a partial class, if the source is also a partial class. <br />
///     If the source class is not a partial class, it will default to a static class with extension methods.    
/// </para>
/// </summary>
/// <param name="message"></param>
[AttributeUsage(AttributeTargets.Class | AttributeTargets.Struct)]
public class ExcludeAsStatic(string? message = null) : BaseAttribute(message);