using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using SkFabricatorAndErector.Domain.Entities;

namespace SkFabricatorAndErector.Infrastructure.Persistence;

public class ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : IdentityDbContext<ApplicationUser>(options)
{
    public DbSet<Inquiry> Inquiries { get; set; }
    public DbSet<Project> Projects { get; set; }
    public DbSet<OurService> OurServices { get; set; }
    public DbSet<Photo> Photos { get; set; }
    public DbSet<TeamMember> TeamMembers { get; set; }
    public DbSet<ClientDetails> ClientDetails { get; set; }
    public DbSet<HomeSlider> HomeSliders { get; set; }
    public DbSet<ApiClient> ApiClients { get; set; }
    public DbSet<OtpCode> OtpCodes { get; set; }
    public DbSet<PageImageSlot> PageImageSlots { get; set; }

    protected override void OnModelCreating(ModelBuilder builder)
    {
        base.OnModelCreating(builder);

        builder.Entity<IdentityRoleClaim<string>>(entity =>
        {
            entity.Property(e => e.Id).UseIdentityByDefaultColumn();
        });

        builder.Entity<IdentityUserClaim<string>>(entity =>
        {
            entity.Property(e => e.Id).UseIdentityByDefaultColumn();
        });

        builder.Entity<PageImageSlot>(entity =>
        {
            entity.HasIndex(e => e.SlotKey).IsUnique();

            entity.HasData(
                new PageImageSlot { Id = 1, SlotKey = "home.hero.background", PageName = "Home", SectionName = "Hero", Label = "Fabrication Yard / Drone Jobsite Shot" },
                new PageImageSlot { Id = 2, SlotKey = "home.whyus.icon.customize-service", PageName = "Home", SectionName = "Why Choose Us", Label = "Customize Service Icon / Photo" },
                new PageImageSlot { Id = 3, SlotKey = "home.whyus.icon.reliable-services", PageName = "Home", SectionName = "Why Choose Us", Label = "Reliable Services Icon / Photo" },
                new PageImageSlot { Id = 4, SlotKey = "home.whyus.icon.client-friendly", PageName = "Home", SectionName = "Why Choose Us", Label = "Client-Friendly Approach Icon / Photo" },
                new PageImageSlot { Id = 5, SlotKey = "home.whyus.icon.competitive-prices", PageName = "Home", SectionName = "Why Choose Us", Label = "Competitive Prices Icon / Photo" },
                new PageImageSlot { Id = 6, SlotKey = "home.whyus.icon.timely-delivery", PageName = "Home", SectionName = "Why Choose Us", Label = "Timely Delivery Icon / Photo" },
                new PageImageSlot { Id = 7, SlotKey = "home.whyus.icon.quality-management", PageName = "Home", SectionName = "Why Choose Us", Label = "Quality Management Icon / Photo" },
                new PageImageSlot { Id = 8, SlotKey = "services.mechanical-piping.card", PageName = "Services", SectionName = "Mechanical & Piping", Label = "Card Thumbnail — Valve/Piping Photo" },
                new PageImageSlot { Id = 9, SlotKey = "services.mechanical-piping.hero", PageName = "Services", SectionName = "Mechanical & Piping", Label = "Detail Page Hero" },
                new PageImageSlot { Id = 10, SlotKey = "services.jacketed-piping.card", PageName = "Services", SectionName = "Jacketed Piping", Label = "Card Thumbnail" },
                new PageImageSlot { Id = 11, SlotKey = "services.jacketed-piping.hero", PageName = "Services", SectionName = "Jacketed Piping", Label = "Detail Page Hero" },
                new PageImageSlot { Id = 12, SlotKey = "services.structure-fabrication.card", PageName = "Services", SectionName = "Structure Fabrication & Erection", Label = "Card Thumbnail" },
                new PageImageSlot { Id = 13, SlotKey = "services.structure-fabrication.hero", PageName = "Services", SectionName = "Structure Fabrication & Erection", Label = "Detail Page Hero" },
                new PageImageSlot { Id = 14, SlotKey = "services.storage-tanks.card", PageName = "Services", SectionName = "Storage Tank Manufacturing", Label = "Card Thumbnail" },
                new PageImageSlot { Id = 15, SlotKey = "services.storage-tanks.hero", PageName = "Services", SectionName = "Storage Tank Manufacturing", Label = "Detail Page Hero" },
                new PageImageSlot { Id = 16, SlotKey = "services.magnetic-filters.card", PageName = "Services", SectionName = "SS Magnetic Filters", Label = "Card Thumbnail" },
                new PageImageSlot { Id = 17, SlotKey = "services.magnetic-filters.hero", PageName = "Services", SectionName = "SS Magnetic Filters", Label = "Detail Page Hero" },
                new PageImageSlot { Id = 18, SlotKey = "services.maintenance.card", PageName = "Services", SectionName = "Plant Maintenance", Label = "Card Thumbnail" },
                new PageImageSlot { Id = 19, SlotKey = "services.maintenance.hero", PageName = "Services", SectionName = "Plant Maintenance", Label = "Detail Page Hero" },
                new PageImageSlot { Id = 20, SlotKey = "services.insulation.card", PageName = "Services", SectionName = "Insulation", Label = "Card Thumbnail" },
                new PageImageSlot { Id = 21, SlotKey = "services.insulation.hero", PageName = "Services", SectionName = "Insulation", Label = "Detail Page Hero" },
                new PageImageSlot { Id = 22, SlotKey = "about.intro.photo", PageName = "About", SectionName = "Introduction", Label = "Supporting Photo Next to Intro Text" },
                new PageImageSlot { Id = 23, SlotKey = "about.safety-triangle.graphic", PageName = "About", SectionName = "Safety", Label = "Service/Quality/Delivery Graphic" },
                new PageImageSlot { Id = 24, SlotKey = "contact.banner.background", PageName = "Contact", SectionName = "Banner", Label = "Contact Page Header Background" },
                new PageImageSlot { Id = 25, SlotKey = "project.proj-1", PageName = "Projects", SectionName = "Piping", Label = "Gas Header Pipeline Assembly" },
                new PageImageSlot { Id = 26, SlotKey = "project.proj-2", PageName = "Projects", SectionName = "Piping", Label = "Water Storage System Piping" },
                new PageImageSlot { Id = 27, SlotKey = "project.proj-3", PageName = "Projects", SectionName = "Maintenance & Ducting", Label = "Industrial Exhaust Ducting Spool" },
                new PageImageSlot { Id = 28, SlotKey = "project.proj-4", PageName = "Projects", SectionName = "Structural", Label = "Factory Ventilation System" },
                new PageImageSlot { Id = 29, SlotKey = "project.proj-5", PageName = "Projects", SectionName = "Piping", Label = "Precision Stainless Spool Piece" },
                new PageImageSlot { Id = 30, SlotKey = "project.proj-6", PageName = "Projects", SectionName = "Maintenance", Label = "Onsite Piping Installation Crew" },
                new PageImageSlot { Id = 31, SlotKey = "project.proj-7", PageName = "Projects", SectionName = "Filters & Vessels", Label = "Exhaust Filter Cyclone Battery" },
                new PageImageSlot { Id = 32, SlotKey = "project.proj-8", PageName = "Projects", SectionName = "Piping", Label = "Skid-Mounted Dosing Skid" },
                new PageImageSlot { Id = 33, SlotKey = "project.proj-9", PageName = "Projects", SectionName = "Filters & Vessels", Label = "SS Magnetic Filter Housing" },
                new PageImageSlot { Id = 34, SlotKey = "project.proj-10", PageName = "Projects", SectionName = "Storage Tanks", Label = "High-Capacity Storage Tank" },
                new PageImageSlot { Id = 35, SlotKey = "project.proj-11", PageName = "Projects", SectionName = "Structural", Label = "Plant Safety Enclosures" },
                new PageImageSlot { Id = 36, SlotKey = "project.proj-12", PageName = "Projects", SectionName = "Storage Tanks", Label = "Stainless Storage Tank Battery" }
            );
        });
    }
}
