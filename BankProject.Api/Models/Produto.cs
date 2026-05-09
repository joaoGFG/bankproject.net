namespace BankProject.Api.Models;

public abstract class Produto
{
    public int Id { get; set; }
    public string Nome { get; set; } = string.Empty;
}