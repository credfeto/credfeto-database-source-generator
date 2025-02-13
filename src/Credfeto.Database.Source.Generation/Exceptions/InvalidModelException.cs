using System;

namespace Credfeto.Database.Source.Generation.Exceptions;

public sealed class InvalidModelException : Exception
{
    public InvalidModelException()
        : this("Invalid model") { }

    public InvalidModelException(string message)
        : base(message) { }

    public InvalidModelException(string message, Exception innerException)
        : base(message: message, innerException: innerException) { }
}
