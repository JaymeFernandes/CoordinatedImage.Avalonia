using ImageLoader.Context.Model;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace ImageLoader.Context.Configuration;

public class FileModelConfigure : IEntityTypeConfiguration<FileModel>
{
    public void Configure(EntityTypeBuilder<FileModel> builder)
    {
        builder.HasKey(x => x.Id);
        builder.Property(x => x.Id).
            ValueGeneratedOnAdd();

        builder.HasIndex(x => x.Key)
            .IsUnique();
        
        builder.Property(x => x.Key)
            .HasMaxLength(50)
            .IsRequired();
        
        builder.Property(x => x.Extension)
            .IsRequired();

        builder.Property(x => x.Created)
            .IsRequired();
        
        builder.Property(x => x.Expires)
            .IsRequired();
    }
}