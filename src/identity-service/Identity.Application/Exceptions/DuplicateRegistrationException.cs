namespace Identity.Application.Exceptions;

public sealed class DuplicateRegistrationException : Exception
{
    public DuplicateRegistrationException(string message) : base(message)
    {
    }
}
