using System;
using System.Diagnostics;
using IncrementialMapper.Abstractions.Base;

namespace IncrementialMapper.Abstractions.Attributes;

/// <summary>
/// 
/// </summary>
/// <typeparam name="TTarget">Target class or interface.</typeparam>
[AttributeUsage(AttributeTargets.Class | AttributeTargets.Struct | AttributeTargets.Interface, AllowMultiple = true)]
public class SGMapperAttribute<TTarget> : BaseAttribute, ISGMapperAttribute;

/// <summary>
/// Has no effect in the user code, only used to refer to the attribute in the generator.
/// </summary>
internal interface ISGMapperAttribute;