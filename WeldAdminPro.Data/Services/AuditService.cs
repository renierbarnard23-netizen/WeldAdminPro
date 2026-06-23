using System;
using WeldAdminPro.Core.Models;
using WeldAdminPro.Core.Services;
using WeldAdminPro.Data.Repositories;

namespace WeldAdminPro.Data.Services
{
    public static class AuditService
    {
        public static void Log(
            string action,
            string module,
            string details)
        {
            var repository =
                new AuditLogRepository(
                    DatabasePath.GetConnectionString());

            var log =
                new AuditLog
                {
                    Id = Guid.NewGuid(),

                    Username =
                        CurrentUserContext.Username,

                    Action = action,

                    Module = module,

                    Details = details,

                    Timestamp = DateTime.UtcNow
                };

            repository.Add(log);
        }
    }
}