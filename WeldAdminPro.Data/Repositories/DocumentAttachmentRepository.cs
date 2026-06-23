using Dapper;
using Microsoft.Data.Sqlite;
using System;
using System.Collections.Generic;
using WeldAdminPro.Core.Quality.Models;

namespace WeldAdminPro.Data.Repositories
{
    public class DocumentAttachmentRepository
    {
        private readonly string _connectionString;

        public DocumentAttachmentRepository(
            string connectionString)
        {
            _connectionString =
                connectionString;
        }

        public void Add(
            DocumentAttachment item)
        {
            using var connection =
                new SqliteConnection(
                    _connectionString);

            connection.Execute(
                @"INSERT INTO DocumentAttachments
                (
                    Id,
                    RelatedEntityId,
                    EntityType,
                    FileName,
                    FilePath,
                    UploadedBy,
                    UploadedDate,
                    Category
                )
                VALUES
                (
                    @Id,
                    @RelatedEntityId,
                    @EntityType,
                    @FileName,
                    @FilePath,
                    @UploadedBy,
                    @UploadedDate,
                    @Category
                )",
                new
                {
                    Id =
                        item.Id.ToString(),

                    RelatedEntityId =
                        item.RelatedEntityId.ToString(),

                    item.EntityType,

                    item.FileName,

                    item.FilePath,

                    item.UploadedBy,

                    item.UploadedDate,

                    item.Category
                });
        }

        public List<DocumentAttachment> GetByEntity(
            Guid entityId)
        {
            using var connection =
                new SqliteConnection(
                    _connectionString);

            var rows = connection.Query(
                @"SELECT *
                  FROM DocumentAttachments
                  WHERE RelatedEntityId = @Id",
                new
                {
                    Id =
                        entityId.ToString()
                });

            var result =
                new List<DocumentAttachment>();

            foreach (var row in rows)
            {
                result.Add(
                    new DocumentAttachment
                    {
                        Id =
                            Guid.Parse(
                                (string)row.Id),

                        RelatedEntityId =
                            Guid.Parse(
                                (string)row.RelatedEntityId),

                        EntityType =
                            row.EntityType?.ToString()
                            ?? "",

                        FileName =
                            row.FileName?.ToString()
                            ?? "",

                        FilePath =
                            row.FilePath?.ToString()
                            ?? "",

                        UploadedBy =
                            row.UploadedBy?.ToString()
                            ?? "",

                        UploadedDate =
                            Convert.ToDateTime(
                                row.UploadedDate),

                        Category =
                            row.Category?.ToString()
                            ?? ""
                    });
            }

            return result;
        }
    }
}