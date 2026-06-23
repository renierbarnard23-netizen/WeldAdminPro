using WeldAdminPro.Core.Quality;

namespace WeldAdminPro.Core.Interfaces
{
    public interface IWelderQualificationRepository
    {
        bool HasValidQualification(
            string welderNumber);

        bool HasValidProcessQualification(
            string welderNumber,
            string process);
    }
}