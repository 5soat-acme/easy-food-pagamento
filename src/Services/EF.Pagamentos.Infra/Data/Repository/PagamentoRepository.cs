using Amazon.DynamoDBv2.DocumentModel;
using Amazon.DynamoDBv2.Model;
using EF.Infra.Commons.EventBus;
using EF.Pagamentos.Domain.Models;
using EF.Pagamentos.Domain.Repository;
using EF.Pagamentos.Infra.Data.AWS;

namespace EF.Pagamentos.Infra.Data.Repository;

public sealed class PagamentoRepository : IPagamentoRepository
{
    private readonly IAwsDatasource _dbContext;
    private readonly IEventBus _bus;
    private readonly string tableName = "Pagamentos";
    private readonly string tableNameTransacoes = "Transacoes";

    public PagamentoRepository(IAwsDatasource dbContext, IEventBus bus)
    {
        _dbContext = dbContext;
        _bus = bus;
    }

    public async Task<Pagamento?> ObterPorId(Guid id)
    {
        QueryRequest request = new QueryRequest
        {
            TableName = tableName,
            KeyConditionExpression = "#Id = :val",
            ExpressionAttributeNames = new Dictionary<string, string>
            {
                { "#Id", "Id" }
            },
            ExpressionAttributeValues = new Dictionary<string, AttributeValue>
            {
                { ":val", new AttributeValue { S = id.ToString() } }
            }
        };

        QueryResponse response = await _dbContext.dynamoClient.QueryAsync(request);
        if (response.Items != null && response.Items.Count > 0)
        {
            var document = Document.FromAttributeMap(response.Items[0]);
            var pagamento = new Pagamento(id: document["Id"].AsGuid(),
                                        pedidoId: document["PedidoId"].AsGuid(),
                                        tipo: (Tipo)document["Tipo"].AsInt(),
                                        dataCriacao: document["DataCriacao"].AsDateTime(),
                                        dataAtualizacao: document.Contains("DataAtualizacao") ? document["DataAtualizacao"].AsDateTime() : null,
                                        valor: document["Valor"].AsDecimal(),
                                        status: (Status)document["Status"].AsInt());
            return pagamento;
        }

        return null;
    }

    public async Task<Pagamento?> ObterPorPedidoId(Guid pedidoId)
    {
        Table table = Table.LoadTable(_dbContext.dynamoClient, tableName);
        ScanFilter filter = new ScanFilter();
        filter.AddCondition("PedidoId", ScanOperator.Equal, pedidoId.ToString());
        ScanOperationConfig config = new ScanOperationConfig
        {
            Filter = filter
        };
        Search search = table.Scan(config);
        var listaDocument = await search.GetNextSetAsync();
        if(listaDocument != null && listaDocument.Count > 0)
        {
            var document = listaDocument[0];
            var pagamento = new Pagamento(id: document["Id"].AsGuid(),
                                        pedidoId: document["PedidoId"].AsGuid(),
                                        tipo: (Tipo)document["Tipo"].AsInt(),
                                        dataCriacao: document["DataCriacao"].AsDateTime(),
                                        dataAtualizacao: document.Contains("DataAtualizacao") ? document["DataAtualizacao"].AsDateTime() : null,
                                        valor: document["Valor"].AsDecimal(),
                                        status: (Status)document["Status"].AsInt());
            return pagamento;
        }

        return null;
    }

    public async Task Criar(Pagamento pagamento)
    {
        var document = new Document
        {
            ["Id"] = pagamento.Id,
            ["PedidoId"] = pagamento.PedidoId,
            ["Tipo"] = (int)pagamento.Tipo,
            ["DataCriacao"] = pagamento.DataCriacao,
            ["Valor"] = pagamento.Valor,
            ["Status"] = (int)pagamento.Status
        };

        await _dbContext.AdicionarItem(tableName, document);
        await PublishEvents(pagamento);
    }

    public async Task Atualizar(Pagamento pagamento)
    {
        var document = new Document
        {
            ["Id"] = pagamento.Id,
            ["PedidoId"] = pagamento.PedidoId,
            ["Tipo"] = (int)pagamento.Tipo,
            ["DataCriacao"] = pagamento.DataCriacao,
            ["DataAtualizacao"] = pagamento.DataAtualizacao,
            ["Valor"] = pagamento.Valor,
            ["Status"] = (int)pagamento.Status
        };

        await _dbContext.AtualizarItem(tableName, document);
        await PublishEvents(pagamento);
    }

    public async Task AdicionarTransacao(Transacao transacao)
    {
        var document = new Document
        {
            ["Id"] = transacao.Id,
            ["PagamentoId"] = transacao.PagamentoId,
            ["Data"] = transacao.Data
        };

        await _dbContext.AdicionarItem(tableNameTransacoes, document);
    }

    public void Dispose()
    {
        _dbContext.Dispose();
    }

    private async Task PublishEvents(Pagamento pagamento)
    {
        if (pagamento.Notifications != null)
        {
            foreach (var e in pagamento.Notifications)
            {
                await _bus.Publish(e);
            }
        }
    }
}