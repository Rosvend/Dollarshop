namespace Identity.Application.Exceptions;

public sealed class UserNotFoundException : Exception
{
    public UserNotFoundException(Guid userId)
        : base($"User '{userId}' was not found.")
    {
    }

    public UserNotFoundException(string username)
        : base($"User '{username}' was not found.")
    {
    }
}
