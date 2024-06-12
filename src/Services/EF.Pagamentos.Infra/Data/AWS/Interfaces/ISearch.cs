using Amazon.DynamoDBv2.DocumentModel;

namespace EF.Pagamentos.Infra.Data.AWS.Interfaces;

public interface ISearch
{
    Task<List<Document>> GetNextSetAsync();
}