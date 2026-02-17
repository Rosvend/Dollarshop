using TiendaOnline.Domain;

namespace TiendaOnline.Abstractions
{
    public interface IUserRepository
    {
        User Register(RegistrationData data);
        User? FindByUsername(string name);
    }
}
