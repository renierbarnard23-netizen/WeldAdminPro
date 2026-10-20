using System;
using System.Collections.Generic;
using WeldAdminPro.Core.Models;

namespace WeldAdminPro.Core.Interfaces
{
    public interface IWpqrRepository
    {
        List<Wpqr> GetAll();

        Wpqr? GetById(Guid id);

        Wpqr? GetByWpqrNumber(string wpqrNumber);

        List<Wpqr> GetByWpsId(Guid wpsId);

        List<Wpqr> GetByWelderQualificationId(int welderQualificationId);

        List<Wpqr> GetActive();

        List<Wpqr> GetApproved();

        void Add(Wpqr wpqr);

        void Update(Wpqr wpqr);

        bool TryApprove(Guid id, string approvedBy, DateTime approvedOn, string approvalRole);

        void SaveRevision(Wpqr previous, Wpqr revision);

        void Delete(Guid id);
    }
}


