using BankProject.Api.Data;
using BankProject.Api.Models;
using BankProject.Api.RabbitMQ;
using Microsoft.AspNetCore.Mvc;
using System.Text.Json;

namespace BankProject.Api.Controllers;

[ApiController]
[Route("api/contratacoes")]
public class ContratacoesController : ControllerBase
{
    private readonly AppDbContext _context;
    private readonly IRabbitMqProducer _producer;

    public ContratacoesController(AppDbContext context, IRabbitMqProducer producer)
    {
        _context = context;
        _producer = producer;
    }

    [HttpPost]
    public async Task<IActionResult> SolicitarContratacao([FromBody] Contratacao contratacao)
    {
        var cliente = await _context.Clientes.FindAsync(contratacao.ClienteId);
        if (cliente == null)
        {
            return NotFound("Cliente não encontrado.");
        }

        var agencia = await _context.Agencias.FindAsync(contratacao.AgenciaId);
        if (agencia == null)
        {
            return NotFound("Agência não encontrada.");
        }

        contratacao.Status = "PENDENTE";
        contratacao.DataSolicitacao = DateTime.UtcNow;

        _context.Contratacoes.Add(contratacao);
        await _context.SaveChangesAsync();

        var message = new
        {
            ContratacaoId = contratacao.Id,
            ClienteId = contratacao.ClienteId,
            AgenciaId = contratacao.AgenciaId,
            ProdutoId = contratacao.ProdutoId
        };
        _producer.PublishMessage("contratacao-solicitada", JsonSerializer.Serialize(message));

        return Accepted(contratacao);
    }

    [HttpGet("{id}")]
    public async Task<IActionResult> BuscarContratacao(int id)
    {
        var contratacao = await _context.Contratacoes.FindAsync(id);
        if (contratacao == null)
            return NotFound();

        return Ok(contratacao);
    }
}