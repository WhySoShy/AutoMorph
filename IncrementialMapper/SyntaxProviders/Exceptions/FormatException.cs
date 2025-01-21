using System;

namespace IncrementialMapper.SyntaxProviders.Exceptions;

public class FormatException(string message) : Exception(message);