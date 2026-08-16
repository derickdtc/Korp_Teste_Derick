using Microserv1Est.Models;
using Microsoft.EntityFrameworkCore;

namespace Microserv1Est.Data;

public class EstoqueDbContext(DbContextOptions<EstoqueDbContext> options) : DbContext(options)
{
    public DbSet<Produto> Produtos => Set<Produto>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        var produto = modelBuilder.Entity<Produto>();
        produto.ToTable("Produtos");
        produto.HasKey(p => p.Id);
        produto.Property(p => p.Codigo).HasMaxLength(50).IsRequired();
        produto.Property(p => p.Descricao).HasMaxLength(200).IsRequired();
        produto.Property(p => p.Saldo).IsRequired();
        produto.HasIndex(p => p.Codigo).IsUnique();
    }
}
