using AutoFixture.AutoMoq;
using AutoFixture;
using EF.Infra.Commons.EventBus;
using Moq;
using EF.Pagamentos.Domain.Repository;
using EF.Pagamentos.Infra.Data.Repository;
using Amazon.DynamoDBv2.Model;
using EF.Pagamentos.Domain.Models;
using FluentAssertions;
using Amazon.DynamoDBv2.DocumentModel;
using EF.Pagamentos.Infra.Data.AWS.Interfaces;
using Amazon.DynamoDBv2;
using Microsoft.EntityFrameworkCore;
using EF.Core.Commons.Messages;
using EF.Pagamentos.Application.Events.Messages;

namespace EF.Pagamentos.Infra.Test.Data.Repository;

public class PagamentoRepositoryTest : IDisposable
{
    private readonly IFixture _fixture;
    private readonly Mock<IAwsDatasource> _dbContextMock;
    private readonly Mock<IEventBus> _busMock;
    private readonly new Mock<ISearch> _searchMock;
    private readonly IPagamentoRepository _pagamentoRepository;
    private bool disposed = false;

    public PagamentoRepositoryTest()
    {
        _fixture = new Fixture().Customize(new AutoMoqCustomization());
        _dbContextMock = _fixture.Freeze<Mock<IAwsDatasource>>();
        _busMock = _fixture.Freeze<Mock<IEventBus>>();
        _searchMock = _fixture.Freeze<Mock<ISearch>>();        

        var mockTableLoader = _fixture.Freeze<Mock<ITableLoader>>();
        var mockTable = _fixture.Freeze<Mock<ITable>>();
        mockTable.Setup(t => t.Scan(It.IsAny<ScanOperationConfig>())).Returns(_searchMock.Object);
        mockTableLoader.Setup(tl => tl.LoadTable(It.IsAny<IAmazonDynamoDB>(), "Pagamentos")).Returns(mockTable.Object);

        _pagamentoRepository = _fixture.Create<PagamentoRepository>();
    }

    [Fact]
    public async Task DeveObterPagamentoPorId()
    {
        // Arrange
        var pagamento = _fixture.Create<Pagamento>();

        var queryResponse = new QueryResponse
        {
            Items = new List<Dictionary<string, AttributeValue>>
            {
                new Dictionary<string, AttributeValue>
                {
                    { "Id", new AttributeValue { S = pagamento.Id.ToString() } },
                    { "PedidoId", new AttributeValue { S = pagamento.PedidoId.ToString() } },
                    { "Tipo", new AttributeValue { N = ((int)pagamento.Tipo).ToString() } },
                    { "DataCriacao", new AttributeValue { S = pagamento.DataCriacao.ToString() } },
                    { "Valor", new AttributeValue { N = pagamento.Valor.ToString() } },
                    { "Status", new AttributeValue { N = ((int)pagamento.Status).ToString() } }
                }
            }
        };

        _dbContextMock.Setup(x => x.dynamoClient.QueryAsync(It.IsAny<QueryRequest>(), default))
                     .ReturnsAsync(queryResponse);

        // Act
        var resultado = await _pagamentoRepository.ObterPorId(pagamento.Id);

        // Assert
        resultado.Should().NotBeNull();
        resultado.Should().BeEquivalentTo(pagamento);
    }

    [Fact]
    public async Task DeveRetornalNull_QuandoObterPagamentoPorIdInexistente()
    {
        // Arrange
        var pagamentoId = _fixture.Create<Guid>();

        var queryResponse = new QueryResponse
        {
            Items = new List<Dictionary<string, AttributeValue>>()
        };

        _dbContextMock.Setup(x => x.dynamoClient.QueryAsync(It.IsAny<QueryRequest>(), default)).ReturnsAsync(queryResponse);

        // Act
        var resultado = await _pagamentoRepository.ObterPorId(pagamentoId);

        // Assert
        resultado.Should().BeNull();
    }

    [Fact]
    public async Task DeveObterPagamentoPorPedidoId()
    {   
        // Arrange
        var pagamento = _fixture.Create<Pagamento>();

        var document = new Document
        {
            ["Id"] = pagamento.Id,
            ["PedidoId"] = pagamento.PedidoId,
            ["Tipo"] = (int)pagamento.Tipo,
            ["DataCriacao"] = pagamento.DataCriacao,
            ["Valor"] = pagamento.Valor,
            ["Status"] = (int)pagamento.Status
        };

        _searchMock.Setup(x => x.GetNextSetAsync()).ReturnsAsync(new List<Document> { document });

        // Act
        var resultado = await _pagamentoRepository.ObterPorPedidoId(pagamento.PedidoId);

        // Assert
        resultado.Should().NotBeNull();
        resultado.Should().BeEquivalentTo(pagamento);
    }

    [Fact]
    public async Task DeveRetornalNull_QuandoObterPagamentoPorPedidoIdInexistente()
    {
        // Arrange
        var pedidoId = _fixture.Create<Guid>();

        _searchMock.Setup(x => x.GetNextSetAsync()).ReturnsAsync(new List<Document> {  });

        // Act
        var resultado = await _pagamentoRepository.ObterPorPedidoId(pedidoId);

        // Assert
        resultado.Should().BeNull();
    }

    [Fact]
    public async Task DeveCriarPagamento()
    {
        // Arrange
        var pagamento = _fixture.Create<Pagamento>();
        pagamento.AddEvent(new PagamentoAutorizadoEvent
        {
            AggregateId = pagamento.Id,
            PedidoId = pagamento.PedidoId
        });

        _dbContextMock.Setup(x => x.AdicionarItem(It.IsAny<string>(), It.IsAny<Document>()));
        _busMock.Setup(x => x.Publish(It.IsAny<Event>()));

        // Act
        await _pagamentoRepository.Criar(pagamento);

        // Assert
        _dbContextMock.Verify(x => x.AdicionarItem(It.IsAny<string>(), It.IsAny<Document>()), Times.Once);
        _busMock.Verify(x => x.Publish(It.IsAny<Event>()));
    }

    [Fact]
    public async Task DeveAtualizarPagamento()
    {
        // Arrange
        var pagamento = _fixture.Create<Pagamento>();

        _dbContextMock.Setup(x => x.AtualizarItem(It.IsAny<string>(), It.IsAny<Document>()));

        // Act
        await _pagamentoRepository.Atualizar(pagamento);

        // Assert
        _dbContextMock.Verify(x => x.AtualizarItem(It.IsAny<string>(), It.IsAny<Document>()), Times.Once);
    }

    [Fact]
    public async Task DeveAdicionarTransacao()
    {
        // Arrange
        var transacao = _fixture.Create<Transacao>();

        _dbContextMock.Setup(x => x.AdicionarItem(It.IsAny<string>(), It.IsAny<Document>()));

        // Act
        await _pagamentoRepository.AdicionarTransacao(transacao);

        // Assert
        _dbContextMock.Verify(x => x.AdicionarItem(It.IsAny<string>(), It.IsAny<Document>()), Times.Once);
    }

    protected virtual void Dispose(bool disposing)
    {
        if (!disposed)
        {
            if (disposing)
            {
                _pagamentoRepository.Dispose();
            }

            disposed = true;
        }
    }

    public void Dispose()
    {
        Dispose(true);
        GC.SuppressFinalize(this);
    }
}
