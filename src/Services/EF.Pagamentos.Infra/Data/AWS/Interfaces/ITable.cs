using Amazon.DynamoDBv2.DocumentModel;

namespace EF.Pagamentos.Infra.Data.AWS.Interfaces;

public interface ITable
{
    ISearch Scan(ScanOperationConfig config);
}