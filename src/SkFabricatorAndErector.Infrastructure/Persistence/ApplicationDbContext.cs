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
    }
}
