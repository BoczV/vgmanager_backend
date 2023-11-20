using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace VGManager.Entities.Configurations;

public class AdditionEntityConfig : IEntityTypeConfiguration<VGAddEntity>
{
    /// <summary>
    /// 
    /// </summary>
    /// <param name="builder">EntityTypeBuilder <see cref="EntityTypeBuilder{AdditionEntity}"/>.</param>
    public void Configure(EntityTypeBuilder<VGAddEntity> builder)
    {
        builder.HasKey(addition => addition.Id);
        builder.Property(addition => addition.Id).ValueGeneratedOnAdd();
        builder.ToTable("Additions");
    }
}
