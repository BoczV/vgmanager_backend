using Microsoft.EntityFrameworkCore;
using VGManager.Entities;
using VGManager.Entities.Configurations;

namespace VGManager.Repositories.DbContexts;

public class OperationsDbContext : DbContext
{
    #region Contructor(s)

    /// <summary>
    /// Initialize a new instance of <see cref="OperationsDbContext"/> for unit testing purposes.
    /// Moq needs this to construct a Mocked version
    /// </summary>
    public OperationsDbContext()
    {
    }

    /// <summary>
    /// Initialize a new instance of <see cref="OperationsDbContext"/>
    /// </summary>
    /// <param name="options">DbContext options</param>
    public OperationsDbContext(DbContextOptions<OperationsDbContext> options)
        : base(options)
    {
    }

    #endregion

    #region DbSets

    public DbSet<VGAddEntity> VGAdditions { get; set; } = null!;
    public DbSet<VGDeleteEntity> VGDeletions { get; set; } = null!;
    public DbSet<VGUpdateEntity> VGEditions { get; set; } = null!;

    #endregion

    #region Overriden Methods

    /// <summary>
    /// Overriden Method to apply entity configurations
    /// </summary>
    /// <param name="modelBuilder">The model builder to configuration</param>
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);
        modelBuilder.ApplyConfiguration(new VGAddEntityConfig());
        modelBuilder.ApplyConfiguration(new VGDeleteEntityConfig());
        modelBuilder.ApplyConfiguration(new VGUpdateEntityConfig());
    }

    #endregion
}
