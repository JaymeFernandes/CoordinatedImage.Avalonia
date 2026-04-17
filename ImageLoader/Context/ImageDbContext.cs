using ImageLoader.Context.Configuration;
using ImageLoader.Context.Model;
using Microsoft.EntityFrameworkCore;

namespace ImageLoader.Context;

public class ImageDbContext : DbContext
{
    public virtual DbSet<FileModel> Files { get; set; }
    
    public ImageDbContext() { }

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    {
        var basePath = Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
            "ImageLoader",
            "Tmp",
            "image.cache");
        
        optionsBuilder.UseSqlite($"Data Source={basePath}");

        base.OnConfiguring(optionsBuilder);
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.ApplyConfiguration(new FileModelConfigure());
        
        base.OnModelCreating(modelBuilder);
    }
}