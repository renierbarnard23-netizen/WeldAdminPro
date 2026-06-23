using WeldAdminPro.Core.Enums;

namespace WeldAdminPro.Core.Services
{
    public static class CurrentUserContext
    {
        public static string Username
        {
            get;
            set;
        }
            = "";

        public static string FullName
        {
            get;
            set;
        }
            = "";

        public static SystemRole Role
        {
            get;
            set;
        }
            = SystemRole.Viewer;

        public static bool IsAuthenticated
        {
            get;
            set;
        }
    }
}
