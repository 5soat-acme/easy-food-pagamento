﻿using AutoFixture;
using AutoFixture.AutoMoq;
using EF.Infra.Commons.Messageria;
using EF.Infra.Commons.Messageria.AWS;
using EF.Pagamentos.Domain.Repository;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Moq;

namespace EF.Api.BDD.Test.Support;

public class CustomWebApplicationFactory<TProgram> : WebApplicationFactory<TProgram> where TProgram : class
{
    public required IServiceProvider ServiceProvider;
    private readonly IFixture _fixture;
    public readonly Mock<IPagamentoRepository> PagamentoRepositoryMock;

    public CustomWebApplicationFactory()
    {
        _fixture = new Fixture().Customize(new AutoMoqCustomization());
        PagamentoRepositoryMock = _fixture.Freeze<Mock<IPagamentoRepository>>();
    }

    protected override IHost CreateHost(IHostBuilder builder)
    {
        builder.ConfigureServices(async services => 
        {
            RemoveProducers(services);
            ConfigureMocks(services);
        });        

        return base.CreateHost(builder);
    }

    private void ConfigureMocks(IServiceCollection services)
    {
        services.AddScoped(_ => PagamentoRepositoryMock.Object);
    }

    private void RemoveProducers(IServiceCollection services)
    {
        var descriptors = services.Where(d => d.ServiceType == typeof(IProducer) && d.ImplementationType == typeof(AwsProducer)).ToList();

        foreach (var descriptor in descriptors)
        {
            services.Remove(descriptor);
        }

        services.AddScoped<IProducer, FakeProducer>();
    }
}