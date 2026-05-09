using System.Net;
using System.Net.Http.Json;
using BankProject.Api.Models; // Ajuste se seu namespace for diferente
using Xunit;

namespace BankProject.Tests.IntegrationTests;

public class BankIntegrationTests : IClassFixture<CustomWebApplicationFactory>
{
    private readonly HttpClient _client;

    public BankIntegrationTests(CustomWebApplicationFactory factory)
    {
        _client = factory.CreateClient();
    }

    [Fact]
    public async Task PostClientePF_CriarESolicitarContratacao_DeveRetornar202()
    {
        // 1. Criar Agência
        var agencia = new Agencia { Nome = "Agencia Central", Numero = "001" };
        var responseAgencia = await _client.PostAsJsonAsync("/api/agencias", agencia);
        responseAgencia.EnsureSuccessStatusCode();
        var agenciaCriada = await responseAgencia.Content.ReadFromJsonAsync<Agencia>();

        // 2. Criar Cliente PF
        var cliente = new PessoaFisica
        {
            AgenciaId = agenciaCriada!.Id,
            Cpf = "98765432100",
            DataNascimento = DateTime.UtcNow.AddYears(-20)
        };
        var responseCliente = await _client.PostAsJsonAsync("/api/clientes/pf", cliente);
        responseCliente.EnsureSuccessStatusCode();
        var clienteCriado = await responseCliente.Content.ReadFromJsonAsync<PessoaFisica>();

        // 3. Solicitar Contratação
        var contratacao = new Contratacao
        {
            ClienteId = clienteCriado!.Id,
            AgenciaId = agenciaCriada.Id,
            ProdutoId = 1 // Supondo ID 1 para o Produto
        };

        var responseContratacao = await _client.PostAsJsonAsync("/api/contratacoes", contratacao);

        // Assert - A API deve retornar 202 Accepted para fila processar
        Assert.Equal(HttpStatusCode.Accepted, responseContratacao.StatusCode);

        // Opcional para extrair o ID para consulta
        var contratacaoPendente = await responseContratacao.Content.ReadFromJsonAsync<Contratacao>();

        // 4. Validar Consulta Status (Que deve ser retornada)
        var responseGetContratacao = await _client.GetAsync($"/api/contratacoes/{contratacaoPendente!.Id}");
        Assert.Equal(HttpStatusCode.OK, responseGetContratacao.StatusCode);
    }

    [Fact]
    public async Task PostAgencia_DeveVincularClienteAgenciaInexistente_Retorna400ou404()
    {
        // Act - Tenta criar um cliente passando uma Agência ID 9999 (que não existe)
        var clientePayload = new 
        {
            Nome = "João Silva",
            Cpf = "12345678901",
            DataNascimento = "2000-01-01",
            AgenciaId = 9999 
        };

        var response = await _client.PostAsJsonAsync("/api/clientes/pf", clientePayload);

        // Assert - A API deve bloquear e não retornar 200 OK
        Assert.False(response.IsSuccessStatusCode);
        Assert.True(response.StatusCode == HttpStatusCode.NotFound || response.StatusCode == HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task PostContratacao_ClienteInexistente_DeveRetornar404()
    {
        var contratacao = new 
        {
            ClienteId = 9999, // Cliente não existe
            Produto = "Emprestimo",
            Valor = 1000.00
        };

        var response = await _client.PostAsJsonAsync("/api/contratacoes", contratacao);

        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }
}