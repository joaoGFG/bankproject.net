namespace BankProject.Api.Models;

public class Contratacao
{
    public int Id { get; set; }
    public int ClienteId { get; set; }
    public Cliente? Cliente { get; set; }
    public int AgenciaId { get; set; }
    public int ProdutoId { get; set; }
    public Produto? Produto { get; set; }
    public DateTime DataSolicitacao { get; set; }
    
    // Status possíveis: PENDENTE, APROVADA, RECUSADA
    public string Status { get; set; } = string.Empty;
    public string MensagemMotivo { get; set; } = string.Empty;
}