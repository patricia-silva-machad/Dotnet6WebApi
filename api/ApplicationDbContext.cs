//criar a aplicacao web (hosting)
using Microsoft.EntityFrameworkCore;

public class ApplicationDbContext: DbContext{
    
    public DbSet<Product> Products { get; set; }

//contrutor: recebe um "DbContextOptions" e repassa para o seu pai ": base(options)"  os : no C# significa herança
    public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base(options) {}
    protected override void OnModelCreating(ModelBuilder builder){
        builder.Entity<Product>()
            .Property(p => p.Description).HasMaxLength(500).IsRequired(false);
        builder.Entity<Product>()
            .Property(p => p.Name).HasMaxLength(120).IsRequired();
        builder.Entity<Product>()
            .Property(p => p.Code).HasMaxLength(10).IsRequired();
    }

}
