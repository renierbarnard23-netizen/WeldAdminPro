using System;
using WeldAdminPro.Core.Models;
using WeldAdminPro.Core.Services;
using WeldAdminPro.Data.Repositories;
using WeldAdminPro.Data.Repositories.Security;

namespace WeldAdminPro.Data.Services.Security;

public class DefaultAdminSeeder
{
    private readonly SystemUserRepository _userRepository;
    private readonly RoleRepository _roleRepository;

    public DefaultAdminSeeder(
        string connectionString)
    {
        _userRepository =
            new SystemUserRepository(
                connectionString);

        _roleRepository =
            new RoleRepository(
                connectionString);
    }

    public async Task SeedAsync()
    {
        var existing =
            _userRepository.GetByUsername(
                "admin");

        if (existing != null)
        {
            return;
        }

        var administratorRole =
            await _roleRepository.GetByNameAsync(
                "Administrator");

        if (administratorRole == null)
        {
            throw new InvalidOperationException(
                "Default administrator could not be created because " +
                "the Administrator role does not exist.");
        }

        var hashService =
            new PasswordHashService();

        _userRepository.Add(
            new SystemUser
            {
                Id = Guid.NewGuid(),

                Username = "admin",

                FullName =
                    "System Administrator",

                PasswordHash =
                    hashService.Hash(
                        "admin123"),

                RoleId =
                    administratorRole.Id,

                RoleName =
                    administratorRole.Name,

                IsActive = true,

                CreatedDate =
                    DateTime.UtcNow
            });
    }
}