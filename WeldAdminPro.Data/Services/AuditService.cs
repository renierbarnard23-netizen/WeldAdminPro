using System;
using WeldAdminPro.Core.Models;
using WeldAdminPro.Core.Security.Abstractions;
using WeldAdminPro.Data.Repositories;

namespace WeldAdminPro.Data.Services
{
    public class AuditService
    {
        private readonly AuditLogRepository _repository;
        private readonly ICurrentUserContext _currentUser;

        public AuditService(
            ICurrentUserContext currentUser)
        {
            _currentUser = currentUser;

            _repository =
                new AuditLogRepository(
                    DatabasePath.GetConnectionString());
        }

        public void Log(
            string action,
            string module,
            string details)
        {
            var log =
                new AuditLog
                {
                    Id = Guid.NewGuid(),

                    Username =
                        _currentUser.Username,

                    Action = action,

                    Module = module,

                    Details = details,

                    Timestamp = DateTime.UtcNow
                };

            _repository.Add(log);
        }
    }
}