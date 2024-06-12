using Amazon.DynamoDBv2.DocumentModel;
using Amazon.DynamoDBv2;
using EF.Pagamentos.Infra.Data.AWS.Interfaces;

namespace EF.Pagamentos.Infra.Data.AWS;

public class TableLoader : ITableLoader
{
    public ITable LoadTable(IAmazonDynamoDB client, string tableName)
    {
        return new TableWrapper(Table.LoadTable(client, tableName));
    }
}