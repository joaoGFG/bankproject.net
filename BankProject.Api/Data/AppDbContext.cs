using BankProject.Api.Models;
using Microsoft.EntityFrameworkCore;

namespace BankProject.Api.Data;

public class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

    public DbSet<Agencia> Agencias { get; set; }
    public DbSet<Cliente> Clientes { get; set; }
    public DbSet<PessoaFisica> PessoasFisicas { get; set; }
    public DbSet<PessoaJuridica> PessoasJuridicas { get; set; }
    
    public DbSet<Produto> Produtos { get; set; }
    public DbSet<Emprestimo> Emprestimos { get; set; }
    
    public DbSet<Contratacao> Contratacoes { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // Configuração de Herança de Clientes (Table-Per-Hierarchy)
        modelBuilder.Entity<Cliente>()
            .HasDiscriminator<string>("TipoCliente")
            .HasValue<PessoaFisica>("PF")
            .HasValue<PessoaJuridica>("PJ");

        // Configuração de Herança de Produtos
        modelBuilder.Entity<Produto>()
            .HasDiscriminator<string>("TipoProduto")
            .HasValue<Emprestimo>("EMPRESTIMO");
    }
}