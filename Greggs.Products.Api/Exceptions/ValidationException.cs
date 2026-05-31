using System;

namespace Greggs.Products.Api.Exceptions;

/// <summary>
/// Thrown when caller-supplied input fails validation. Mapped to HTTP 400
/// by <see cref="Middleware.ExceptionHandlingMiddleware"/>.
/// </summary>
public class ValidationException : Exception
{
    public ValidationException(string message)
        : base(message)
    {
    }

    public ValidationException(string message, Exception innerException)
        : base(message, innerException)
    {
    }
}
