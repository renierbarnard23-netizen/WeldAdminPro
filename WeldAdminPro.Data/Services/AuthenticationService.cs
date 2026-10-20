using WeldAdminPro.Core.Models;
using WeldAdminPro.Data.Repositories;
using WeldAdminPro.Core.Services;

namespace WeldAdminPro.Data.Services
{
    public class AuthenticationService
    {
        private readonly SystemUserRepository
            _repository;

        private readonly PasswordHashService
            _hashService = new();

        public AuthenticationService(
            SystemUserRepository repository)
        {
            _repository = repository;
        }

        public SystemUser? Authenticate(
            string username,
            string password)
        {
            var user =
                _repository.GetByUsername(username);

            if (user == null)
            {
                return null;
            }

            if (!user.IsActive)
            {
                return null;
            }

            if (!_hashService.Verify(
                    password,
                    user.PasswordHash))
            {
                return null;
            }

            if (_hashService.IsLegacyHash(
                    user.PasswordHash))
            {
                user.PasswordHash =
                    _hashService.Hash(password);

                _repository.Update(user);
            }

            return user;
        }
    }
}
