using WeldAdminPro.Core.Models;

namespace WeldAdminPro.Core.Services
{
    public static class CurrentUserService
    {
        public static SystemUser? CurrentUser
        {
            get;
            set;
        }

        public static bool IsLoggedIn =>
            CurrentUser != null;

        public static void Logout()
        {
            CurrentUser = null;
        }
    }
}