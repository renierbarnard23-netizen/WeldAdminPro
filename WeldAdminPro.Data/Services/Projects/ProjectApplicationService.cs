using System;
using System.Linq;
using System.Diagnostics;
using System.IO;
using System.Collections.Generic;
using WeldAdminPro.Core.Models;
using WeldAdminPro.Core.Quality;
using WeldAdminPro.Data.Repositories;
using WeldAdminPro.Data.Services;

namespace WeldAdminPro.Data.Services.Projects
{
    public class ProjectApplicationService
    {
        private readonly ProjectRepository _projectRepository;
        private readonly ProjectDocumentRepository _documentRepository;
        private readonly ProjectStockUsageRepository _stockRepository;
        private readonly ProjectMaterialService _materialService;
        private readonly ProjectDocumentFileRepository _fileRepository;
        public List<ProjectDocumentFile> GetDocumentFiles(Guid documentId)
        {
            return _fileRepository.GetByDocument(documentId);
        }

        public ProjectApplicationService()
        {
            _projectRepository = new ProjectRepository();
            _documentRepository = new ProjectDocumentRepository();
            _stockRepository = new ProjectStockUsageRepository();
            _materialService = new ProjectMaterialService();

            _fileRepository = new ProjectDocumentFileRepository();
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
            Console.WriteLine("SaveProject called");

            if (project == null)
                throw new ArgumentNullException(nameof(project));

            ValidateProject(project);

            if (project.Id == Guid.Empty)
            {
                Console.WriteLine("Calling CreateProject");
                CreateProject(project);
            }
            else
            {
                Console.WriteLine("Calling UpdateProject");
                UpdateProject(project);
            }
        }

        public void DeleteProject(Guid id)
        {
            _projectRepository.Delete(id);
        }

        private void CreateProject(Project project)
        {
            Console.WriteLine("CreateProject entered");

            project.CreatedOn = DateTime.Now;
            project.LastModifiedOn = DateTime.Now;

            project.Status = ProjectStatus.Active;
            project.IsInvoiced = false;
            project.IsArchived = false;

            Console.WriteLine("Calling repository.Add");

            _projectRepository.Add(project);

            Console.WriteLine("Returned from repository.Add");

            InitializeProject(project);
        }

        private void UpdateProject(Project project)
        {
            project.LastModifiedOn = DateTime.Now;

            ApplyInvoiceRules(project);

            _projectRepository.Update(project);
        }

        private void InitializeProject(Project project)
        {
            new ProjectDocumentService()
                .InitializeProjectDocuments(project.Id);
        }

        private void ValidateProject(Project project)
        {
            if (string.IsNullOrWhiteSpace(project.ProjectName))
                throw new InvalidOperationException("Project name is required.");

            if (string.IsNullOrWhiteSpace(project.Client))
                throw new InvalidOperationException("Client is required.");

            ValidateCompletion(project);
        }

        private void ValidateCompletion(Project project)
        {
            if (project.Status == ProjectStatus.Completed)
            {
                // Future:
                // Verify compliance
                // Verify outstanding documents
                // Verify repairs completed
            }
        }

        private void ApplyInvoiceRules(Project project)
        {
            if (project.IsInvoiced &&
                string.IsNullOrWhiteSpace(project.InvoiceNumber))
            {
                throw new InvalidOperationException(
                    "Invoice number is required when a project is marked as invoiced.");
            }
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

        public void RegisterDocumentFile(
            ProjectDocument document,
            string storedFilePath)
        {
            if (document == null)
                return;

            if (string.IsNullOrWhiteSpace(storedFilePath))
                return;

            var file = new ProjectDocumentFile
            {
                Id = Guid.NewGuid(),
                ProjectDocumentId = document.Id,
                FileName = Path.GetFileName(storedFilePath),
                FilePath = storedFilePath,
                UploadedOn = DateTime.Now,
                IsApproved = false
            };

            _fileRepository.Add(file);

            if (!document.AllowMultiple)
                document.FilePath = storedFilePath;

            document.Files.Add(file);

            document.IsUploaded = true;
            document.UploadedDate = DateTime.Now;
            document.LastModifiedOn = DateTime.Now;

            _documentRepository.Update(document);
        }

        public bool ToggleDocumentApproval(ProjectDocument document)
        {
            if (document == null)
                return false;

            // Cannot approve a document that hasn't been uploaded.
            if (!document.IsUploaded)
                return false;

            document.IsApproved = !document.IsApproved;

            document.LastModifiedOn = DateTime.Now;

            if (document.IsApproved)
            {
                document.ApprovedOn = DateTime.Now;
                document.ApprovedBy = Environment.UserName;
            }
            else
            {
                document.ApprovedOn = null;
                document.ApprovedBy = string.Empty;
            }

            _documentRepository.Update(document);

            // Keep all uploaded files in sync with the document approval.
            var files = _fileRepository.GetByDocument(document.Id);

            foreach (var file in files)
            {
                file.IsApproved = document.IsApproved;

                _fileRepository.Update(file);
            }

            return true;
        }

        public bool OpenDocument(ProjectDocument document)
        {
            if (document == null)
                return false;

            try
            {
                // Multi-file document
                if (document.AllowMultiple)
                {
                    var files = _fileRepository.GetByDocument(document.Id);

                    if (files.Count == 0)
                        return false;

                    foreach (var file in files)
                    {
                        if (File.Exists(file.FilePath))
                        {
                            Process.Start(new ProcessStartInfo
                            {
                                FileName = file.FilePath,
                                UseShellExecute = true
                            });
                        }
                    }

                    return true;
                }

                // Single-file document
                if (string.IsNullOrWhiteSpace(document.FilePath))
                    return false;

                if (!File.Exists(document.FilePath))
                    return false;

                Process.Start(new ProcessStartInfo
                {
                    FileName = document.FilePath,
                    UseShellExecute = true
                });

                return true;
            }
            catch
            {
                return false;
            }
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