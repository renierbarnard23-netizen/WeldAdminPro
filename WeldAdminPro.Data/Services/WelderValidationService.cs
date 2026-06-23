using WeldAdminPro.Data.Repositories;

namespace WeldAdminPro.Data.Services
{
    public class WelderValidationService
    {
        private readonly WelderQualificationRepository _repo = new();

        public (int expired, int expiringSoon) Check()
        {
            var all = _repo.GetAll();

            int expired = 0;
            int expiringSoon = 0;

            foreach (var w in all)
            {
                if (w.ExpiryDate < DateTime.Today)
                {
                    expired++;
                }
                else if (w.ExpiryDate <= DateTime.Today.AddDays(30))
                {
                    expiringSoon++;
                }
            }

            return (expired, expiringSoon);
        }
    }
}