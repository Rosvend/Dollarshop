using TiendaOnline.Abstractions;
using TiendaOnline.Domain;

namespace TiendaOnline.Services
{
    public class InMemoryUserRepository : IUserRepository
    {
        private readonly List<User> _users = new();
        private int _nextId = 1;

        public User Register(RegistrationData data)
        {
            var user = new User
            {
                Id = _nextId++,
                Username = data.Email,
                PasswordHash = string.Empty
            };

            _users.Add(user);
            return user;
        }

        public User? FindByUsername(string name)
        {
            return _users.FirstOrDefault(u => u.Username == name);
        }
    }
}
