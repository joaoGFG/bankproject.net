using BankProject.Api.Data;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.AspNetCore.TestHost;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace BankProject.Tests.IntegrationTests;

public class CustomWebApplicationFactory : WebApplicationFactory<Program>
{
    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        // Força a API a rodar em ambiente "Testing"
        builder.UseEnvironment("Testing");

        builder.ConfigureTestServices(services =>
        {
            // Remove o banco Oracle da injeção de dependência atual
            services.RemoveAll(typeof(DbContextOptions<AppDbContext>));

            // Adiciona o banco de dados InMemory para os testes, com nome fixo exigido
            services.AddDbContext<AppDbContext>(options =>
            {
                options.UseInMemoryDatabase("TestDb");
            });

            // Garantir que o banco de dados seja criado durante os testes
            var sp = services.BuildServiceProvider();
            using var scope = sp.CreateScope();
            var scopedServices = scope.ServiceProvider;
            var db = scopedServices.GetRequiredService<AppDbContext>();
            db.Database.EnsureCreated();
        });
    }
}