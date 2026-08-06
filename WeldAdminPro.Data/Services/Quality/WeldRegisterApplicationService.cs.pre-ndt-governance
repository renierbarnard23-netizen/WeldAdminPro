using WeldAdminPro.Core.Models;
using WeldAdminPro.Core.Quality.Enums;
using WeldAdminPro.Core.Quality.Models;
using WeldAdminPro.Data.Repositories;

namespace WeldAdminPro.Data.Services.Quality
{
    public class WeldRegisterApplicationService
    {
        private readonly WeldRepository _weldRepository;
        private readonly WeldHistoryRepository _historyRepository;
        private readonly WeldNdtRepository _ndtRepository;

        public WeldRegisterApplicationService()
        {
            _weldRepository = new WeldRepository();

            var connectionString = 
                $"Data Source={DatabasePath.Get()}";

            _historyRepository =
                new WeldHistoryRepository(connectionString);

            _ndtRepository =
                new WeldNdtRepository();
        }

        public async Task<List<Weld>> GetProjectWelds(Guid projectId)
        {
            return await _weldRepository.GetByProjectAsync(projectId);
        }

        public List<WeldHistoryEntry> GetHistory(Guid weldId)
        {
            return _historyRepository.GetByWeld(weldId);
        }

        public List<WeldNdtResult> GetNdt(Guid weldId)
        {
            return _ndtRepository.GetByWeld(weldId);
        }

        public List<Weld> GetAllWelds()
        {
            return _weldRepository.GetAll();
        }

        public async Task CaptureNdtAsync(
    Weld weld,
    WeldNdtResult result)
        {
            // Save the NDT result
            _ndtRepository.Add(result);

            // Update weld status
            weld.NdtStatus = result.Result.ToString();

            switch (result.Result)
            {
                case NdtResultType.Accept:
                    weld.Status = WeldStatusType.Accepted;
                    break;

                case NdtResultType.Reject:
                    weld.Status = WeldStatusType.Rejected;
                    break;

                case NdtResultType.Repair:
                    weld.Status = WeldStatusType.RepairRequired;
                    break;

                case NdtResultType.Pending:
                    weld.Status = WeldStatusType.NdtPending;
                    break;

                case NdtResultType.ConditionalAccept:
                    weld.Status = WeldStatusType.Accepted;
                    break;
            }

            weld.RequiresRepair =
                result.RequiresRepair ||
                result.Result == NdtResultType.Repair ||
                result.Result == NdtResultType.Reject;
            
            weld.RepairCycle = result.RepairCycle;
            weld.LastNdtDate = result.InspectionDate;
            weld.LastNdtResult = result.Result.ToString();

            await _weldRepository.UpdateAsync(weld);

            // Write history
            _historyRepository.Add(
                new WeldHistoryEntry
                {
                    Id = Guid.NewGuid(),
                    WeldId = weld.Id,
                    EventDate = DateTime.Now,
                    EventType = "NDT",
                    Description =
                        $"{result.NdtMethod} inspection : {result.Result}",
                    UserName = result.InspectorName,
                    StatusSnapshot = weld.Status.ToString()
                });
        }
    }
}