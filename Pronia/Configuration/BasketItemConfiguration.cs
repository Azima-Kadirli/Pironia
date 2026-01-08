using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Pronia.Configuration;

public class BasketItemConfiguration:IEntityTypeConfiguration<BasketItem>
{
    public void Configure(EntityTypeBuilder<BasketItem> builder)
    {
        builder.Property(b => b.Count).IsRequired();
        builder.ToTable(opt =>
        {
            opt.HasCheckConstraint("CK_Product_Count","[Count]>0");
        });
        builder.HasOne(b=>b.Product).WithMany(b=>b.BasketItems).HasForeignKey(b=>b.ProductId).HasPrincipalKey(b=>b.Id).OnDelete(DeleteBehavior.Cascade);
        builder.HasOne(b=>b.AppUser).WithMany(b=>b.BasketItems).HasForeignKey(b=>b.AppUserId).HasPrincipalKey(b=>b.Id).OnDelete(DeleteBehavior.Cascade);
        
    }
}