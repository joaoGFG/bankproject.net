namespace BankProject.Api.Models;

public abstract class Cliente
{
    public int Id { get; set; }
    public int AgenciaId { get; set; }
    public Agencia? Agencia { get; set; }

    public ICollection<Contratacao> Contratacoes { get; set; } = new List<Contratacao>();
}