using System;
using System.Linq;
using System.Collections.Generic;
using WeldAdminPro.Core.Models;
using WeldAdminPro.Core.Quality;
using WeldAdminPro.Data.Repositories;

namespace WeldAdminPro.Data.Services.Projects
{
    public class ProjectApplicationService
    {
        private readonly ProjectRepository _projectRepository;
        private readonly ProjectDocumentRepository _documentRepository;
        private readonly ProjectStockUsageRepository _stockRepository;
        private readonly ProjectMaterialService _materialService;

        public ProjectApplicationService()
        {
            _projectRepository = new ProjectRepository();
            _documentRepository = new ProjectDocumentRepository();
            _stockRepository = new ProjectStockUsageRepository();
            _materialService = new ProjectMaterialService();
        }

        // =============================
        // Projects
        // =============================

        public List<Project> GetProjects()
        {
            return _projectRepository.GetAll().ToList();
        }

        public Project? GetProject(Guid id)
        {
            return _projectRepository.GetById(id);
        }

        public void SaveProject(Project project)
        {
            var existing = _projectRepository.GetById(project.Id);

            if (existing == null)
            {
                _projectRepository.Add(project);
            }
            else
            {
                _projectRepository.Update(project);
            }
        }

        public void DeleteProject(Guid id)
        {
            _projectRepository.Delete(id);
        }

        // =============================
        // Stock
        // =============================

        public List<ProjectStockUsage> GetStockUsage(Guid projectId)
        {
            return _stockRepository.GetByProjectId(projectId);
        }
        public List<ProjectStockSummary> GetStockSummary(Guid projectId)
        {
            return _stockRepository.GetProjectStockSummary(projectId);
        }

        // =============================
        // Documents
        // =============================

        public List<ProjectDocument> GetDocuments(Guid projectId)
        {
            return _documentRepository.GetByProject(projectId);
        }

        // =============================
        // Material Summary
        // =============================

        public List<ProjectStockSummary> GetMaterialSummary(Guid projectId)
        {
            return _stockRepository.GetProjectStockSummary(projectId);
        }

        public void UpdateDocument(ProjectDocument document)
        {
            _documentRepository.Update(document);
        }

        public void AddDocument(ProjectDocument document)
        {
            _documentRepository.Add(document);
        }

        public List<ProjectDocumentFile> GetDocumentFiles(Guid documentId)
        {
            return new ProjectDocumentFileRepository()
                .GetByDocument(documentId);
        }

        // =============================
        // MATERIAL MANAGEMENT
        // =============================

        public List<StockItem> GetStockItems()
        {
            return _materialService
                .GetStockItems()
                .ToList();
        }

        public void IssueMaterial(
            Project project,
            StockItem item,
            decimal quantity,
            string issuedBy)
        {
            _materialService.IssueMaterial(
                project,
                item,
                quantity,
                issuedBy);
        }

        public void ReturnMaterial(
            Project project,
            StockItem item,
            decimal quantity,
            decimal unitCost,
            string issuedBy)
        {
            _materialService.ReturnMaterial(
                project,
                item,
                quantity,
                unitCost,
                issuedBy);
        }

        public List<ReturnableItemDto> GetReturnableItems(Guid projectId)
        {
            return _materialService
                .GetReturnableItems(projectId)
                .ToList();
        }

        public List<StockTransaction> GetProjectTransactions(Guid projectId)
        {
            return _materialService
                .GetProjectTransactions(projectId)
                .ToList();
        }
    }
}