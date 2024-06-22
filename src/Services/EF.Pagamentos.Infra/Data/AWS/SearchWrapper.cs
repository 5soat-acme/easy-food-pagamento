using Amazon.DynamoDBv2.DocumentModel;
using EF.Pagamentos.Infra.Data.AWS.Interfaces;

namespace EF.Pagamentos.Infra.Data.AWS;

public class SearchWrapper : ISearch
{
    private readonly Search _search;

    public SearchWrapper(Search search)
    {
        _search = search;
    }

    public Task<List<Document>> GetNextSetAsync()
    {
        return _search.GetNextSetAsync();
    }
}