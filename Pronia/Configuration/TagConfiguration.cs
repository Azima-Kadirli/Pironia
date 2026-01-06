using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Pronia.Configuration;

public class TagConfiguration:IEntityTypeConfiguration<Tag>
{
    public void Configure(EntityTypeBuilder<Tag> builder)
    {
        builder.HasMany(x=>x.ProductTags).WithOne(x=>x.Tag).HasForeignKey(x => x.TagId).HasPrincipalKey(x => x.Id).OnDelete(DeleteBehavior.Cascade);

    }
}