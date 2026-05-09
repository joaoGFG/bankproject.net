using BankProject.Api.Data;
using BankProject.Api.Models;
using Microsoft.AspNetCore.Mvc;

namespace BankProject.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class AgenciasController : ControllerBase
{
    private readonly AppDbContext _context;

    public AgenciasController(AppDbContext context)
    {
        _context = context;
    }

    [HttpPost]
    public async Task<IActionResult> Cadastrar(Agencia agencia)
    {
        _context.Agencias.Add(agencia);
        await _context.SaveChangesAsync();
        
        return CreatedAtAction(nameof(BuscarPorId), new { id = agencia.Id }, agencia);
    }

    [HttpGet("{id}")]
    public async Task<IActionResult> BuscarPorId(int id)
    {
        var agencia = await _context.Agencias.FindAsync(id);
        if (agencia == null) return NotFound("Agência não encontrada.");
        
        return Ok(agencia);
    }
}