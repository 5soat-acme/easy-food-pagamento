using AutoFixture;
using AutoFixture.AutoMoq;
using EF.Api.BDD.Test.Support;
using EF.Pagamentos.Application.DTOs.Requests;
using EF.Pagamentos.Application.DTOs.Responses;
using EF.Pagamentos.Domain.Models;
using FluentAssertions;
using Moq;
using System.Net;
using System.Text;
using System.Text.Json;

namespace EF.Api.BDD.Test.StepDefinitions;

[Binding]
[Scope(Tag = "PagamentoController")]
public class PagamentoControllerStepDefinitions
{
    private readonly IFixture _fixture;
    private readonly CustomWebApplicationFactory<Program> _factory;
    private readonly HttpClient _client;
    private HttpResponseMessage _result;

    public PagamentoControllerStepDefinitions(CustomWebApplicationFactory<Program> factory)
    {
        _fixture = new Fixture().Customize(new AutoMoqCustomization());
        _factory = factory;
        _client = factory.CreateClient();
    }

    [Given(@"que o serviço pagamento está configurado")]
    public void DadoQueOServicoPagamentoEstaConfigurado()
    {
    }

    [When(@"eu solicitar os tipos de pagamento")]
    public async Task QuandoEuSolicitarOsTiposDePagamento()
    {
        _result = await _client.GetAsync($"/api/pagamentos/tipos");
    }

    [When(@"eu solicitar o pagamento pelo id do pedido ""(.*)""")]
    public async Task QuandoEuSolicitarOPagamentoPeloIdDoPedido(Guid pedidoId)
    {
        var pagamento = _fixture.Create<Pagamento>();
        _factory.PagamentoRepositoryMock.Setup(x => x.ObterPorPedidoId(pedidoId)).ReturnsAsync(pagamento);

        _result = await _client.GetAsync($"/api/pagamentos?pedidoId={pedidoId}");
    }

    [When(@"o webhook de autorização de pagamento for acionado")]
    public async Task QuandoOWebhookDeAutorizacaoDePagamentoForAcionado()
    {
        var autorizarPagamentoDto = _fixture.Build<AutorizarPagamentoDto>().With(x => x.Autorizado, true).Create();
        var pagamento = _fixture.Create<Pagamento>();

        _factory.PagamentoRepositoryMock.Setup(x => x.ObterPorId(autorizarPagamentoDto.PagamentoId)).ReturnsAsync(pagamento);
        _factory.PagamentoRepositoryMock.Setup(x => x.AdicionarTransacao(It.IsAny<Transacao>()));
        _factory.PagamentoRepositoryMock.Setup(x => x.Atualizar(pagamento));

        var content = new StringContent(JsonSerializer.Serialize(autorizarPagamentoDto), Encoding.UTF8, "application/json");
        var request = new HttpRequestMessage(HttpMethod.Post, "/api/pagamentos/autorizar/webhook")
        {
            Content = content
        };
        request.Headers.Add("Authorization", "9E541194-61B4-44F6-BE2A-B1F08C24BB52");
        _result = await _client.SendAsync(request);
    }

    [Then(@"a resposta deve ser (.*)")]
    public void EntaoARespostaDeveSer(int statusCode)
    {
        _result.StatusCode.Should().Be((HttpStatusCode)statusCode);
    }

    [Then(@"a resposta deve conter os tipos de pagamento")]
    public async Task EntaoARespostaDeveConterOsTiposDePagamento()
    {
        var body = await _result.Content.ReadAsStringAsync();
        var resposta = JsonSerializer.Deserialize<MetodoPagamentoDto>(body, new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true
        });
        var experado = new MetodoPagamentoDto { MetodosPagamento = Enum.GetNames(typeof(Tipo)) };
        resposta.Should().BeEquivalentTo(experado);
    }

    [AfterScenario]
    public void AfterScenario()
    {
        _client.Dispose();
        _factory.Dispose();
    }
}
