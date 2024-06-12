using Amazon.DynamoDBv2;

namespace EF.Pagamentos.Infra.Data.AWS.Interfaces;

public interface ITableLoader
{
    ITable LoadTable(IAmazonDynamoDB client, string tableName);
}