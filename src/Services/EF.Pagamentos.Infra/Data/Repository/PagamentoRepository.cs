using Amazon.DynamoDBv2.DocumentModel;
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
        var document = await _dbContext.LerItem(tableName, id.ToString());
        if (document == null) return null;

        var pagamento = new Pagamento(id: Guid.Parse(document["Id"]), 
            pedidoId: Guid.Parse(document["PedidoId"]), 
            tipo: Tipo.Pix, 
            dataCriacao: DateTime.Now, 
            dataAtualizacao: DateTime.Now, 
            valor: 0, 
            status: Status.Pendente);

        return pagamento;
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
        foreach (Document document in await search.GetNextSetAsync())
        {
            // Processar cada item retornado
            Console.WriteLine("Item encontrado:");
            foreach (var attribute in document.GetAttributeNames())
            {
                Console.WriteLine($"{attribute}: {document[attribute]}");
            }
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
            ["DataCriacao"] = pagamento.DataAtualizacao,
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

/*public sealed class PagamentoRepository : IPagamentoRepository
{
    private readonly PagamentoDbContext _dbContext;

    public PagamentoRepository(PagamentoDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public IUnitOfWork UnitOfWork => _dbContext;

    public async Task<Pagamento?> ObterPorId(Guid id)
    {
        return await _dbContext.Pagamentos
            .Include(i => i.Transacoes)
            .FirstOrDefaultAsync(p => p.Id == id);
    }

    public async Task<Pagamento?> ObterPorPedidoId(Guid pedidoId)
    {
        return await _dbContext.Pagamentos
            .FirstOrDefaultAsync(p => p.PedidoId == pedidoId);
    }

    public void Criar(Pagamento pagamento)
    {
        _dbContext.Pagamentos.Add(pagamento);
    }

    public void Atualizar(Pagamento pagamento)
    {
        _dbContext.Pagamentos.Update(pagamento);
    }

    public void AdicionarTransacao(Transacao transacao)
    {
        _dbContext.Transacoes.Add(transacao);
    }

    public void Dispose()
    {
        _dbContext.Dispose();
    }
}*/