using Microsoft.EntityFrameworkCore;

namespace trinibytes.Models;

public class MyDbContext : DbContext
{
    public MyDbContext(DbContextOptions<MyDbContext> options)
        : base(options)
    {
    }

    public DbSet<CaribbeanJobsPost>? CaribbeanJobsPosts { get; set; }
    public DbSet<BlogPost>? BlogPosts { get; set; }

    public DbSet<UploadedImage>? UploadedImages { get; set; }

    protected override void OnModelCreating(ModelBuilder builder)
    {
        base.OnModelCreating(builder);
        builder.Entity<BlogPost>()
            .Property(b => b.LastModifiedDate)
            .HasDefaultValueSql("getdate()");
        builder.Entity<BlogPost>()
            .Property(b => b.CreationDate)
            .HasDefaultValueSql("getdate()");
    }

    public override Task<int> SaveChangesAsync(bool acceptAllChangesOnSuccess,
        CancellationToken cancellationToken = default(CancellationToken))
    {
        var editedEntities = ChangeTracker.Entries().Where(E => E.State == EntityState.Modified).ToList();

        editedEntities.ForEach(E => { E.Property("LastModifiedDate").CurrentValue = DateTime.Now; });

        return base.SaveChangesAsync(acceptAllChangesOnSuccess, cancellationToken);
    }
}