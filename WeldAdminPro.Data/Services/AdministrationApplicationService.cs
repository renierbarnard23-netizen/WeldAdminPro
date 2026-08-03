using System.Linq;
using System.Collections.Generic;
using WeldAdminPro.Core.Models;
using WeldAdminPro.Core.Services;
using WeldAdminPro.Data.Repositories;

namespace WeldAdminPro.Data.Services
{
    public class AdministrationApplicationService
    {
        private readonly SystemUserRepository _userRepository;
        private readonly AuditLogRepository _auditRepository;
        private readonly ProductionSettingsRepository _settingsRepository;
        private readonly PasswordHashService _passwordHashService;
        private readonly AuditService _auditService;

        public AdministrationApplicationService(
            AuditService auditService)
        {
            _auditService = auditService;
            var connectionString = DatabasePath.GetConnectionString();

            _userRepository = new SystemUserRepository(connectionString);
            _auditRepository = new AuditLogRepository(connectionString);
            _settingsRepository = new ProductionSettingsRepository(); 

            _passwordHashService = new PasswordHashService();
        }

        private SystemUser? GetUser(Guid userId)
        {
            return _userRepository
                .GetAll()
                .FirstOrDefault(x => x.Id == userId);
        }

        public List<SystemUser> GetUsers()
        {
            return _userRepository.GetAll();
        }

        public void UpdateUser(SystemUser user)
        {
            _userRepository.Update(user);

            _auditService.Log(
                "EDIT USER",
                "User Management",
                $"Edited user: {user.Username}");
        }

        public void EnableUser(Guid userId)
        {
            var user = GetUser(userId);

            if (user == null)
                return;

            user.IsActive = true;

            _userRepository.Update(user);

            _auditService.Log(
                "ENABLE USER",
                "User Management",
                $"Enabled user: {user.Username}");
        }

        public void DisableUser(Guid userId)
        {
            var user = GetUser(userId);

            if (user == null)
                return;

            user.IsActive = false;

            _userRepository.Update(user);

            _auditService.Log(
                "DISABLE USER",
                "User Management",
                $"Disabled user: {user.Username}");
        }

        public void ResetPassword(Guid userId)
        {
            var user = GetUser(userId);

            if (user == null)
                return;

            user.PasswordHash = _passwordHashService.Hash("password123");

            _userRepository.Update(user);

            _auditService.Log(
                "RESET PASSWORD",
                "User Management",
                $"Password reset for: {user.Username}");
        }

        public void SetPassword(Guid userId, string password)
        {
            if (string.IsNullOrWhiteSpace(password))
                throw new ArgumentException("Password cannot be empty.");

            var user = GetUser(userId);

            if (user == null)
                return;

            user.PasswordHash = _passwordHashService.Hash(password);

            _userRepository.Update(user);

            _auditService.Log(
                "CHANGE PASSWORD",
                "User Management",
                $"Password changed for: {user.Username}");
        }

        public void AddUser(SystemUser user)
        {
            if (string.IsNullOrWhiteSpace(user.PasswordHash))
            {
                user.PasswordHash =
                    _passwordHashService.Hash("password123");
            }

            _userRepository.Add(user);

            _auditService.Log(
                "CREATE USER",
                "User Management",
                $"Created user: {user.Username}");
        }

        public ProductionSettings GetProductionSettings()
        {
            return _settingsRepository.Get();
        }

        public void SaveProductionSettings(ProductionSettings settings)
        {
            _settingsRepository.Save(settings);

            _auditService.Log(
                "UPDATE PRODUCTION SETTINGS",
                "Administration",
                "Production settings updated.");
        }

        public List<AuditLog> GetAuditLog()
        {
            return _auditRepository.GetAll();
        }
    }

}