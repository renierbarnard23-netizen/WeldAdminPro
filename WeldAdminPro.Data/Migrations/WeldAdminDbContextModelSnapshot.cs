using System;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using WeldAdminPro.Data;

#nullable disable

[DbContext(typeof(WeldAdminDbContext))]
partial class WeldAdminDbContextModelSnapshot : ModelSnapshot
{
    protected override void BuildModel(ModelBuilder modelBuilder)
    {
        modelBuilder
            .HasAnnotation("ProductVersion", "8.0.0")
            .HasAnnotation("Relational:MaxIdentifierLength", 128);

        modelBuilder.Entity("WeldAdminPro.Core.Models.Document", b =>
        {
            b.Property<Guid>("Id").HasColumnType("TEXT");

            b.Property<string>("DocType").IsRequired().HasColumnType("TEXT");

            b.Property<DateTime>("UploadedAt").HasColumnType("TEXT");

            b.Property<string>("UploadedBy").IsRequired().HasColumnType("TEXT");

            b.Property<string>("FilePath").IsRequired().HasColumnType("TEXT");

            b.Property<string>("DocumentNumber").IsRequired().HasColumnType("TEXT");

            b.Property<string>("Revision").IsRequired().HasColumnType("TEXT");

            b.Property<string>("Status").IsRequired().HasColumnType("TEXT");

            b.Property<Guid>("ProjectId").HasColumnType("TEXT");

            b.Property<string>("Title").IsRequired().HasColumnType("TEXT");

            b.HasKey("Id");

            b.HasIndex("ProjectId", "DocumentNumber");

            b.ToTable("Documents");
        });

        modelBuilder.Entity("WeldAdminPro.Core.Models.Project", b =>
        {
            b.Property<Guid>("Id").HasColumnType("TEXT");

            b.Property<string>("Client").IsRequired().HasColumnType("TEXT");

            b.Property<string>("DatabookVersion").IsRequired().HasColumnType("TEXT");

            b.Property<DateTime?>("EndDate").HasColumnType("TEXT");

            b.Property<DateTime?>("StartDate").HasColumnType("TEXT");

            b.Property<string>("ProjectNumber").IsRequired().HasColumnType("TEXT");

            b.Property<string>("Title").IsRequired().HasColumnType("TEXT");

            b.HasKey("Id");

            b.HasIndex("ProjectNumber").IsUnique();

            b.ToTable("Projects");
        });

        modelBuilder.Entity("WeldAdminPro.Core.Models.Wps", b =>
        {
            b.Property<Guid>("Id").HasColumnType("TEXT");

            b.Property<string>("ApprovedBy").IsRequired().HasColumnType("TEXT");

            b.Property<DateTime?>("ApprovedAt").HasColumnType("TEXT");

            b.Property<string>("Author").IsRequired().HasColumnType("TEXT");

            b.Property<string>("BaseMaterial").IsRequired().HasColumnType("TEXT");

            b.Property<string>("FillerMaterial").IsRequired().HasColumnType("TEXT");

            b.Property<string>("Position").IsRequired().HasColumnType("TEXT");

            b.Property<string>("PostHeat").IsRequired().HasColumnType("TEXT");

            b.Property<string>("Preheat").IsRequired().HasColumnType("TEXT");

            b.Property<string>("Process").IsRequired().HasColumnType("TEXT");

            b.Property<Guid>("ProjectId").HasColumnType("TEXT");

            b.Property<string>("Revision").IsRequired().HasColumnType("TEXT");

            b.Property<string>("WpsNumber").IsRequired().HasColumnType("TEXT");

            b.HasKey("Id");

            b.HasIndex("ProjectId");

            b.ToTable("Wpss");
        });
    }
}
