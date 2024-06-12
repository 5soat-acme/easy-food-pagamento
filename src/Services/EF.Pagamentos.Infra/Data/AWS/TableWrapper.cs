using Amazon.DynamoDBv2.DocumentModel;
using EF.Pagamentos.Infra.Data.AWS.Interfaces;

namespace EF.Pagamentos.Infra.Data.AWS;

public class TableWrapper : ITable
{
    private readonly Table _table;

    public TableWrapper(Table table)
    {
        _table = table;
    }

    public ISearch Scan(ScanOperationConfig config)
    {
        return new SearchWrapper(_table.Scan(config));
    }
}