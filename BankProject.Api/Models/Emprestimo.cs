namespace BankProject.Api.Models;

public class Emprestimo : Produto
{
    public decimal ValorLiberado { get; set; }
    public int QuantidadeParcelas { get; set; }
    
    // Regra de negócio extra (Dupla)
    public int ScoreMinimoExigido { get; set; } 
}