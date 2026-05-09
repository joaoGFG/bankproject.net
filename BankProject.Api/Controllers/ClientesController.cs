using BankProject.Api.Data;
using BankProject.Api.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace BankProject.Api.Controllers;

[ApiController]
[Route("api/clientes")]
public class ClientesController : ControllerBase
{
    private readonly AppDbContext _context;

    public ClientesController(AppDbContext context)
    {
        _context = context;
    }

    [HttpPost("pf")]
    public async Task<IActionResult> CriarClientePF([FromBody] PessoaFisica pf)
    {
        // EXIGÊNCIA: Validar se a agência existe.
        var agenciaExiste = await _context.Agencias.FindAsync(pf.AgenciaId);
        if (agenciaExiste == null)
            return NotFound(new { mensagem = "Agência não encontrada." });

        // Validação de CPF Duplicado
        var cpfExiste = await _context.PessoasFisicas.FirstOrDefaultAsync(c => c.Cpf == pf.Cpf);
        if (cpfExiste != null)
            return BadRequest(new { mensagem = "CPF já cadastrado." });

        _context.PessoasFisicas.Add(pf);
        await _context.SaveChangesAsync();

        return CreatedAtAction(nameof(BuscarCliente), new { id = pf.Id }, pf);
    }

    [HttpPost("pj")]
    public async Task<IActionResult> CriarClientePJ([FromBody] PessoaJuridica pj)
    {
        var agenciaExiste = await _context.Agencias.FindAsync(pj.AgenciaId);
        if (agenciaExiste == null)
            return NotFound(new { mensagem = "Agência não encontrada." });

        // Validação de CNPJ Duplicado
        var cnpjExiste = await _context.PessoasJuridicas.FirstOrDefaultAsync(c => c.Cnpj == pj.Cnpj);
        if (cnpjExiste != null)
            return BadRequest(new { mensagem = "CNPJ já cadastrado." });

        _context.PessoasJuridicas.Add(pj);
        await _context.SaveChangesAsync();

        return CreatedAtAction(nameof(BuscarCliente), new { id = pj.Id }, pj);
    }

    [HttpGet("{id}")]
    public async Task<IActionResult> BuscarCliente(int id)
    {
        // O professor especificou usar FindAsync
        var cliente = await _context.Clientes.FindAsync(id);
        if (cliente == null)
            return NotFound();

        return Ok(cliente);
    }
}