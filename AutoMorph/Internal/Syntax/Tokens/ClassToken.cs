﻿using System;
using System.CodeDom.Compiler;
using System.Collections.Generic;
using System.IO;
using AutoMorph.Internal.Constants;
using AutoMorph.Internal.Syntax.Kinds;

// ReSharper disable TypeWithSuspiciousEqualityIsUsedInRecord.Global

// Everything in the record is being initialized at some point, if not it will not be generated, therefor we can safely disable to warning.
#pragma warning disable CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider adding the 'required' modifier or declaring as nullable.

namespace AutoMorph.Internal.Syntax.Tokens;

/// <summary>
/// A token that represents a class that needs to be generated, with its mapper methods and other information that the SG needs.
/// </summary>
internal sealed record ClassToken
{
    /// <summary>
    /// This just represents 
    /// </summary>
    public ReferenceClassToken AttachedSourceClass { get; init; }

    /// <summary>
    /// The namespace that the mapper should be generated into.
    /// </summary>
    public string? Namespace { get; set; }

    /// <summary>
    /// Class visibility, this defaults to public if null.
    /// </summary>
    public VisibilityKind Visibility { get; init; }

    /// <summary>
    /// <para>
    ///     Modifiers that should be added to the class. <br />
    ///     Auto orders, to keep the 'heaviest' modifiers first,
    ///     this just means that it appears in the correct order when assembling the code. 
    /// </para>
    /// </summary>
    public List<ModifierKind> Modifiers { get; set; } = [];

    /// <summary>
    /// <para>
    ///     Contains what method types should be generated.
    /// </para>
    /// </summary>
    public List<MethodToken> Methods { get; set; } = [];

    /// <summary>
    /// Namespaces mostly from mappers, that are generated as partial.
    /// </summary>
    public HashSet<string> NameSpaces { get; set; } = [];

    /// <summary>
    /// Writer that is being used to generate each file.
    /// </summary>
    internal IndentedTextWriter Writer { get; } = new IndentedTextWriter(new StringWriter());
}