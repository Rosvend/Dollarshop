namespace Identity.Application.Exceptions;

public sealed class InvalidCredentialsException : Exception
{
    public InvalidCredentialsException()
        : base("The username or password is incorrect.")
    {
    }
}
