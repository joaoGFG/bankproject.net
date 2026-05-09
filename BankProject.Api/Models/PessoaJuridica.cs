namespace BankProject.Api.Models;

public class PessoaJuridica : Cliente
{
    public string Cnpj { get; set; } = string.Empty;
    public string RazaoSocial { get; set; } = string.Empty;
}