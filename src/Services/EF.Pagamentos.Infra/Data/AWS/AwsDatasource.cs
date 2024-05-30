using Amazon;
using Amazon.DynamoDBv2;
using Amazon.DynamoDBv2.DocumentModel;
using Amazon.Runtime;
using EF.Infra.Commons.Messageria.AWS.Config;
using Microsoft.Extensions.Options;

namespace EF.Pagamentos.Infra.Data.AWS
{
    public class AwsDatasource : IAwsDatasource
    {
        private readonly AmazonDynamoDBClient _dynamoClient;

        AmazonDynamoDBClient IAwsDatasource.dynamoClient => _dynamoClient;

        public AwsDatasource(IOptions<AwsCredentialsSettings> options)
        {
            var awsCredentialsSettings = options.Value;
            var credentials = new SessionAWSCredentials(awsCredentialsSettings.AccessKey, awsCredentialsSettings.SecretKey, awsCredentialsSettings.SessionToken);
            var region = RegionEndpoint.GetBySystemName(awsCredentialsSettings.Region);
            _dynamoClient = new AmazonDynamoDBClient(credentials, region);
        }

        public async Task<Document> AdicionarItem(string tableName, Document document)
        {
            var table = Table.LoadTable(_dynamoClient, tableName);
            return await table.PutItemAsync(document);
        }

        public async Task<Document> AtualizarItem(string tableName, Document document)
        {
            var table = Table.LoadTable(_dynamoClient, tableName);
            return await table.UpdateItemAsync(document);
        }

        public async Task ExcluirItem(string tableName, string id)
        {
            var table = Table.LoadTable(_dynamoClient, tableName);
            await table.DeleteItemAsync(id);
        }

        public void Dispose()
        {
            _dynamoClient.Dispose();
        }
    }
}