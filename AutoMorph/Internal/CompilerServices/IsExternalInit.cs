using System.ComponentModel;
// ReSharper disable CheckNamespace

namespace System.Runtime.CompilerServices;

/// <summary>
/// Reserved to be used by the compiler for tracking metadata.
/// This class should not be used by developers in source code.
/// </summary>
[EditorBrowsable(EditorBrowsableState.Never)]
internal class IsExternalInit;

// This helps to fix so you can create init only properties on records.
// Source: -> https://btburnett.com/csharp/2020/12/11/csharp-9-records-and-init-only-setters-without-dotnet5.html