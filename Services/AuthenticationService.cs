using TiendaOnline.Abstractions;
using TiendaOnline.Domain;

namespace TiendaOnline.Services
{
    public class AuthenticationService : IAuthenticationService
    {
        private readonly IUserRepository _userRepository;
        private User? _currentUser;

        public AuthenticationService(IUserRepository userRepository)
        {
            _userRepository = userRepository;
        }

        public bool Authenticate(string username, string password)
        {
            var user = _userRepository.FindByUsername(username);

            if (user == null)
            {
                return false;
            }

            if (user.PasswordHash != password)
            {
                return false;
            }

            _currentUser = user;
            return true;
        }

        public User? GetCurrentUser()
        {
            return _currentUser;
        }
    }
}
