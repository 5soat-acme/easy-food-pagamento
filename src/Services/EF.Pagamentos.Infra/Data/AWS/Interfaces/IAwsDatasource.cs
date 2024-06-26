﻿using Amazon.DynamoDBv2;
using Amazon.DynamoDBv2.DocumentModel;

namespace EF.Pagamentos.Infra.Data.AWS.Interfaces
{
    public interface IAwsDatasource : IDisposable
    {
        public AmazonDynamoDBClient dynamoClient { get; }

        public Task<Document> AdicionarItem(string tableName, Document document);

        public Task<Document> AtualizarItem(string tableName, Document document);

        public Task ExcluirItem(string tableName, string id);
    }
}
