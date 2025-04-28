using Evently.Common.Domain;

namespace Evently.Common.Application.Exceptions;

public sealed class EventlyException(string requestName, Error? error = null, Exception? innerException = null)
    : Exception("Application exception", innerException)
{
    public string RequestName { get; } = requestName;
    public Error? Error { get; } = error;
}
