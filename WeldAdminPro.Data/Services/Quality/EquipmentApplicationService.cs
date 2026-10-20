using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using WeldAdminPro.Core.Models;
using WeldAdminPro.Core.Quality.Models;
using WeldAdminPro.Core.Security;
using WeldAdminPro.Core.Security.Abstractions;
using WeldAdminPro.Core.Reporting.Interfaces;
using WeldAdminPro.Data.Repositories;
using WeldAdminPro.Data.Services.Projects;

namespace WeldAdminPro.Data.Services.Quality
{
    public class EquipmentApplicationService
    {
        private readonly EquipmentRepository _equipmentRepository;
        private readonly EquipmentCalibrationRepository _calibrationRepository;
        private readonly ICurrentUserContext _currentUser;
        private readonly IPermissionAuthorizationService _permissions;
        private readonly IProjectAccessAuthorizationService _projectAccess;
        private readonly ProjectApplicationService _projectService;
        private readonly IDocumentVaultRepository _documentVault;
        private readonly CompanyRepository _companyRepository;

        public EquipmentApplicationService(
            EquipmentRepository equipmentRepository,
            EquipmentCalibrationRepository calibrationRepository,
            ICurrentUserContext currentUser,
            IPermissionAuthorizationService permissions,
            IProjectAccessAuthorizationService projectAccess,
            ProjectApplicationService projectService,
            IDocumentVaultRepository documentVault,
            CompanyRepository companyRepository)
        {
            _equipmentRepository = equipmentRepository ?? throw new ArgumentNullException(nameof(equipmentRepository));
            _calibrationRepository = calibrationRepository ?? throw new ArgumentNullException(nameof(calibrationRepository));
            _currentUser = currentUser ?? throw new ArgumentNullException(nameof(currentUser));
            _permissions = permissions ?? throw new ArgumentNullException(nameof(permissions));
            _projectAccess = projectAccess ?? throw new ArgumentNullException(nameof(projectAccess));
            _projectService = projectService ?? throw new ArgumentNullException(nameof(projectService));
            _documentVault = documentVault ?? throw new ArgumentNullException(nameof(documentVault));
            _companyRepository = companyRepository ?? throw new ArgumentNullException(nameof(companyRepository));
        }

        public async Task<List<Equipment>> GetForProjectAsync(Guid projectId)
        {
            var project = await RequireProjectAccessAsync(projectId);
            var companyId = ResolveWorkingCompany(project);
            await EnsureCompanyAccessAsync(companyId);
            return _equipmentRepository.GetByCompany(companyId, activeOnly: true);
        }

        public async Task<List<Company>> GetAccessibleCompaniesAsync()
        {
            await RequirePermissionAsync(PermissionKeys.Quality.Equipment);
            if (!_currentUser.IsAuthenticated || string.IsNullOrWhiteSpace(_currentUser.UserId))
                throw new UnauthorizedAccessException("Authentication is required.");

            var companies = _companyRepository.GetAll().Where(x => x.IsActive).ToList();
            var accessible = new List<Company>();
            foreach (var company in companies)
            {
                if (await _projectAccess.CanAccessCompanyAsync(_currentUser.UserId, company.Id))
                    accessible.Add(company);
            }
            return accessible;
        }

        public async Task<List<Equipment>> GetManagedEquipmentAsync(Guid companyId)
        {
            await RequirePermissionAsync(PermissionKeys.Quality.Equipment);
            await EnsureCompanyAccessAsync(companyId);
            return _equipmentRepository.GetByCompany(companyId);
        }

        public async Task<Equipment?> GetByIdAsync(Guid id)
        {
            await RequirePermissionAsync(PermissionKeys.Quality.Equipment);
            var equipment = _equipmentRepository.GetById(id);
            if (equipment == null) return null;
            await EnsureCompanyAccessAsync(equipment.CompanyId);
            return equipment;
        }

        public async Task AddEquipmentAsync(Guid companyId, Equipment equipment)
        {
            await RequirePermissionAsync(PermissionKeys.Quality.Equipment);
            await EnsureCompanyAccessAsync(companyId);

            if (companyId == Guid.Empty)
                throw new InvalidOperationException("A valid company must be selected before adding equipment.");

            equipment.CompanyId = companyId;
            _equipmentRepository.Add(equipment);
        }

        public async Task UpdateEquipmentAsync(Equipment equipment)
        {
            await RequirePermissionAsync(PermissionKeys.Quality.Equipment);
            await EnsureCompanyAccessAsync(equipment.CompanyId);
            _equipmentRepository.Update(equipment);
        }

        public async Task<List<EquipmentCalibration>> GetCalibrationsAsync(Guid equipmentId)
        {
            await RequirePermissionAsync(PermissionKeys.Quality.Calibration);
            var equipment = _equipmentRepository.GetById(equipmentId)
                ?? throw new InvalidOperationException("Equipment could not be found.");
            await EnsureCompanyAccessAsync(equipment.CompanyId);
            return _calibrationRepository.GetByEquipment(equipmentId);
        }

        public async Task AddCalibrationAsync(EquipmentCalibration calibration)
        {
            await RequirePermissionAsync(PermissionKeys.Quality.Calibration);
            var equipment = _equipmentRepository.GetById(calibration.EquipmentId)
                ?? throw new InvalidOperationException("Equipment could not be found.");
            await EnsureCompanyAccessAsync(equipment.CompanyId);
            calibration.CompanyId = equipment.CompanyId;

            if (equipment.CalibrationRequired && !calibration.DocumentVaultFileId.HasValue)
            {
                throw new InvalidOperationException(
                    "A calibration certificate document must be linked when calibration is required.");
            }

            if (calibration.DocumentVaultFileId.HasValue)
            {
                var document = _documentVault.GetById(calibration.DocumentVaultFileId.Value)
                    ?? throw new InvalidOperationException("The selected calibration certificate document could not be found.");

                if (document.CompanyId != equipment.CompanyId)
                    throw new UnauthorizedAccessException("The calibration certificate belongs to another company.");

                if (document.Category != WeldAdminPro.Core.Reporting.Enums.DocumentCategoryType.CalibrationCertificate)
                    throw new InvalidOperationException("The linked document must be a Calibration Certificate.");

                if (!document.IsApproved)
                    throw new InvalidOperationException(
                        "The calibration certificate document must be approved before it can support a calibration record.");
            }

            _calibrationRepository.Add(calibration);
        }

        public async Task ApproveCalibrationAsync(Guid calibrationId)
        {
            await RequirePermissionAsync(PermissionKeys.Quality.Calibration);
            var calibration = _calibrationRepository.GetById(calibrationId)
                ?? throw new InvalidOperationException("Calibration record could not be found.");
            await EnsureCompanyAccessAsync(calibration.CompanyId);

            if (calibration.ExpiryDate < calibration.CalibrationDate)
                throw new InvalidOperationException("Calibration record dates are invalid.");

            _calibrationRepository.Approve(calibrationId, _currentUser.Username);
        }

        public async Task<EquipmentCalibration?> GetValidCalibrationAsync(Guid equipmentId, DateTime inspectionDate)
        {
            var equipment = _equipmentRepository.GetById(equipmentId);
            if (equipment == null)
                return null;

            await EnsureCompanyAccessAsync(equipment.CompanyId);

            if (!equipment.IsActive || !equipment.CalibrationRequired)
                return equipment.CalibrationRequired ? null : new EquipmentCalibration
                {
                    Id = Guid.Empty,
                    EquipmentId = equipment.Id,
                    CompanyId = equipment.CompanyId,
                    CalibrationDate = inspectionDate.Date,
                    ExpiryDate = inspectionDate.Date,
                    IsActive = true,
                    IsApproved = true,
                    CertificateNumber = "NOT REQUIRED"
                };

            var calibration = _calibrationRepository.GetValidForDate(equipmentId, inspectionDate);
            if (calibration == null)
                return null;

            if (calibration.DocumentVaultFileId.HasValue)
            {
                var document = _documentVault.GetById(calibration.DocumentVaultFileId.Value);
                if (document == null ||
                    document.CompanyId != equipment.CompanyId ||
                    document.Category != WeldAdminPro.Core.Reporting.Enums.DocumentCategoryType.CalibrationCertificate ||
                    !document.IsApproved)
                {
                    return null;
                }
            }

            return calibration;
        }

        public async Task<bool> IsNdtCalibrationCompliantAsync(
            Guid projectId,
            IEnumerable<WeldNdtResult> ndtResults)
        {
            var project = await RequireProjectAccessAsync(projectId);
            var companyId = ResolveWorkingCompany(project);

            var results = ndtResults?.ToList() ?? new List<WeldNdtResult>();
            if (results.Count == 0)
                return true;

            foreach (var ndt in results)
            {
                var equipmentIds = (ndt.EquipmentIds ?? new List<Guid>())
                    .Where(x => x != Guid.Empty)
                    .Distinct()
                    .ToList();

                if (equipmentIds.Count == 0 &&
                    ndt.EquipmentId.HasValue &&
                    ndt.EquipmentId.Value != Guid.Empty)
                {
                    equipmentIds.Add(ndt.EquipmentId.Value);
                }

                if (equipmentIds.Count == 0)
                    return false;

                var usageByEquipment = (ndt.EquipmentUsages ?? new List<WeldNdtEquipmentUsage>())
                    .ToDictionary(x => x.EquipmentId, x => x);

                foreach (var equipmentId in equipmentIds)
                {
                    var equipment = _equipmentRepository.GetById(equipmentId);
                    if (equipment == null || equipment.CompanyId != companyId || !equipment.IsActive)
                        return false;

                    if (!equipment.CalibrationRequired)
                        continue;

                    var calibration = await GetValidCalibrationAsync(
                        equipment.Id,
                        ndt.InspectionDate);

                    if (calibration == null)
                        return false;

                    if (usageByEquipment.TryGetValue(equipment.Id, out var usage) &&
                        usage.CalibrationRecordId.HasValue &&
                        usage.CalibrationRecordId.Value != calibration.Id)
                    {
                        return false;
                    }

                    // For legacy W17 records, the primary calibration reference
                    // remains authoritative when no W18 usage row exists.
                    if (!usageByEquipment.ContainsKey(equipment.Id) &&
                        ndt.EquipmentId == equipment.Id &&
                        ndt.CalibrationRecordId.HasValue &&
                        ndt.CalibrationRecordId.Value != calibration.Id)
                    {
                        return false;
                    }
                }
            }

            return true;
        }

        private async Task<Project> RequireProjectAccessAsync(Guid projectId)
        {
            if (!_currentUser.IsAuthenticated || string.IsNullOrWhiteSpace(_currentUser.UserId))
                throw new UnauthorizedAccessException("Authentication is required.");

            var project = await _projectService.GetProjectAsync(projectId);
            if (project == null)
                throw new UnauthorizedAccessException("The requested project does not exist or is not accessible.");

            return project;
        }

        private async Task RequirePermissionAsync(string permission)
        {
            if (!_currentUser.IsAuthenticated || string.IsNullOrWhiteSpace(_currentUser.UserId))
                throw new UnauthorizedAccessException("Authentication is required.");

            if (!await _permissions.HasPermissionAsync(_currentUser.UserId, _currentUser.Role, permission))
                throw new UnauthorizedAccessException("You do not have permission to manage quality equipment and calibration.");
        }

        private async Task EnsureCompanyAccessAsync(Guid companyId)
        {
            if (companyId == Guid.Empty ||
                !await _projectAccess.CanAccessCompanyAsync(_currentUser.UserId, companyId))
                throw new UnauthorizedAccessException("The requested quality equipment record is not accessible.");
        }

        private Guid RequireCurrentCompany()
        {
            if (!_currentUser.CompanyId.HasValue || _currentUser.CompanyId.Value == Guid.Empty)
                throw new UnauthorizedAccessException("A company context is required.");
            return _currentUser.CompanyId.Value;
        }

        private static Guid ResolveWorkingCompany(Project project)
        {
            if (project.WeldingSubcontractorCompanyId.HasValue && project.WeldingSubcontractorCompanyId.Value != Guid.Empty)
                return project.WeldingSubcontractorCompanyId.Value;
            if (project.CompanyId.HasValue && project.CompanyId.Value != Guid.Empty)
                return project.CompanyId.Value;
            throw new InvalidOperationException("The project has no responsible company for welding/NDT equipment.");
        }
    }
}
