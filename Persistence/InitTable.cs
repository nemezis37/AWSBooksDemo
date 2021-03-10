using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Amazon.DynamoDBv2;
using Amazon.DynamoDBv2.Model;

namespace Persistence
{
    public static class InitTable
    {
        public static async Task InitTableOnCreate(IAmazonDynamoDB amazonDynamoDB)
        {
            var listTablesResponse = await amazonDynamoDB.ListTablesAsync();
            var tableNames = listTablesResponse.TableNames;
            if (!tableNames.Contains("Book"))
            {
                var request = new CreateTableRequest
                {
                    TableName = "Book",
                    AttributeDefinitions = new List<AttributeDefinition>
                    {
                        new()
                        {
                            AttributeName = "ISBN",
                            // "S" = string, "N" = number, and so on.
                            AttributeType = "S"
                        }
                    },
                    KeySchema = new List<KeySchemaElement>
                    {
                        new()
                        {
                            AttributeName = "ISBN",
                            // "HASH" = hash key, "RANGE" = range key.
                            KeyType = "HASH"
                        }
                    },
                    ProvisionedThroughput = new ProvisionedThroughput
                    {
                        ReadCapacityUnits = 10,
                        WriteCapacityUnits = 5
                    },
                };

                var response = await amazonDynamoDB.CreateTableAsync(request);

                Console.WriteLine("Table created with request ID: " +
                                  response.ResponseMetadata.RequestId);
            }
        }
    }
}