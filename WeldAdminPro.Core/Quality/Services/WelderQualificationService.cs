using System;
using WeldAdminPro.Core.Quality;

namespace WeldAdminPro.Core.Quality.Services
{
    public enum QualificationStatus
    {
        Valid,
        ExpiringSoon,
        Expired
    }

    public class WelderQualificationService
    {
        public QualificationStatus GetStatus(WelderQualification w)
        {
            var today = DateTime.Today;

            if (w.ExpiryDate < today)
                return QualificationStatus.Expired;

            if (w.ExpiryDate <= today.AddDays(30))
                return QualificationStatus.ExpiringSoon;

            return QualificationStatus.Valid;
        }
    }
}