using Microserv2Fatu.Models;
using Microsoft.EntityFrameworkCore;

namespace Microserv2Fatu.Data;

public class FaturamentoDbContext(DbContextOptions<FaturamentoDbContext> options) : DbContext(options)
{
    public DbSet<NotaFiscal> NotasFiscais => Set<NotaFiscal>();
    public DbSet<ItemNotaFiscal> ItensNotasFiscais => Set<ItemNotaFiscal>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        var nota = modelBuilder.Entity<NotaFiscal>();
        nota.ToTable("NotasFiscais");
        nota.HasKey(n => n.Id);
        nota.Property(n => n.Numero).IsRequired();
        nota.HasIndex(n => n.Numero).IsUnique();
        nota.Property(n => n.Status).HasConversion<string>().HasMaxLength(20).IsRequired();
        nota.HasMany(n => n.Itens).WithOne(i => i.NotaFiscal).HasForeignKey(i => i.NotaFiscalId).OnDelete(DeleteBehavior.Cascade);

        var item = modelBuilder.Entity<ItemNotaFiscal>();
        item.ToTable("ItensNotasFiscais");
        item.HasKey(i => i.Id);
        item.Property(i => i.ProdutoId).IsRequired();
        item.Property(i => i.Descricao).HasMaxLength(200).IsRequired();
        item.Property(i => i.Quantidade).IsRequired();
    }
}
