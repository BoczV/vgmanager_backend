using VGManager.Entities.SecretEntities;
using VGManager.Repositories.Boilerplate;
using VGManager.Repositories.DbContexts;
using VGManager.Repositories.Interfaces.SecretRepositories;

namespace VGManager.Repositories.SecretRepositories;

public class KeyVaultCopyColdRepository : SqlRepository<KeyVaultCopyEntity>, IKeyVaultCopyColdRepository
{
    public KeyVaultCopyColdRepository(OperationsDbContext dbContext) : base(dbContext)
    {
    }

    public async Task AddEntityAsync(KeyVaultCopyEntity entity, CancellationToken cancellationToken = default)
    {
        await AddAsync(entity, cancellationToken);
        await SaveChangesAsync(cancellationToken);
    }

    public async Task<IEnumerable<KeyVaultCopyEntity>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        var result = await GetAllAsync(new KeyVaultCopySpecification(), cancellationToken);
        return result?.ToList() ?? Enumerable.Empty<KeyVaultCopyEntity>();
    }

    public async Task<IEnumerable<KeyVaultCopyEntity>> GetAsync(
        DateTime from,
        DateTime to,
        string user,
        CancellationToken cancellationToken = default
        )
    {
        var result = await GetAllAsync(new KeyVaultCopySpecification(from, to.AddDays(1), user), cancellationToken);
        return result?.ToList() ?? Enumerable.Empty<KeyVaultCopyEntity>();
    }

    public async Task<IEnumerable<KeyVaultCopyEntity>> GetAsync(
        DateTime from,
        DateTime to,
        CancellationToken cancellationToken = default
        )
    {
        var result = await GetAllAsync(new KeyVaultCopySpecification(from, to.AddDays(1)), cancellationToken);
        return result?.ToList() ?? Enumerable.Empty<KeyVaultCopyEntity>();
    }

    public class KeyVaultCopySpecification : SpecificationBase<KeyVaultCopyEntity>
    {
        public KeyVaultCopySpecification() : base(entity => !string.IsNullOrEmpty(entity.Id))
        {
        }

        public KeyVaultCopySpecification(DateTime from, DateTime to, string user) : base(
            entity => entity.Date >= from &&
            entity.Date <= to &&
            entity.User == user
            )
        {
        }

        public KeyVaultCopySpecification(DateTime from, DateTime to) : base(
            entity => entity.Date >= from &&
            entity.Date <= to
            )
        {
        }
    }
}
