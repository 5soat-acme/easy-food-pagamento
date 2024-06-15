using EF.Pagamentos.Application.UseCases.Interfaces;
using FluentAssertions;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.DependencyInjection;
using System.Net;

namespace EF.Api.Test;

public class ProgramTest : IClassFixture<WebApplicationFactory<Program>>
{
    private readonly WebApplicationFactory<Program> _factory;
    private readonly HttpClient _client;

    public ProgramTest(WebApplicationFactory<Program> factory)
    {
        _factory = factory.WithWebHostBuilder(builder =>
        {
        });

        _client = _factory.CreateClient();
    }

    [Fact]
    public void DeveChecarRegistroDeServicos()
    {
        var services = _factory.Services;

        // Verify singleton services
        services.GetService<IHttpContextAccessor>().Should().NotBeNull();

        // Verify scoped services
        using (var scope = services.CreateScope())
        {
            scope.ServiceProvider.GetService<IProcessarPagamentoUseCase>().Should().NotBeNull();
        }
    }

    [Fact]
    public async Task DeveChecarDisponibilidadeDoSwagger()
    {
        // Check if Swagger is available
        var response = await _client.GetAsync("/swagger/index.html");
        response.EnsureSuccessStatusCode();
        response.StatusCode.Should().Be(HttpStatusCode.OK);
    }
}