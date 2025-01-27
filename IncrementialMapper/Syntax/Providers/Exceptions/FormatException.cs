using System;

namespace IncrementialMapper.Syntax.Providers.Exceptions;

public class FormatException(string message) : Exception(message);