using TiendaOnline.Domain;

namespace TiendaOnline.Abstractions
{
    public interface IAuthenticationService
    {
        bool Authenticate(string user, string pass);
        User? GetCurrentUser();
    }
}
