using System;

namespace AutoMorph.Internal.Syntax.Kinds;

[Flags]
internal enum CastingKind
{
    None = 0,
    Direct = 1 << 0,
    Explicit = 1 << 1,
    String = 1 << 2,
    TryParse = 1 << 3,
    TryParseToString = 1 << 4, // Forces the input to be appended as string
    Parse = 1 << 5,
    ParseToString = 1 << 6, // Forces the input to be appended as string
}