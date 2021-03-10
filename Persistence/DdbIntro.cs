using System;
using System.Collections.Generic;
using System.IO;
using System.Net.NetworkInformation;
using System.Threading.Tasks;
using Amazon.DynamoDBv2;
using Amazon.DynamoDBv2.DocumentModel;
using Amazon.DynamoDBv2.Model;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace Persistence
{
    public class DdbIntro
    {
        /*-----------------------------------------------------------------------------------
        *  If you are creating a client for the Amazon DynamoDB service, make sure your credentials
        *  are set up first, as explained in:
        *  https://docs.aws.amazon.com/amazondynamodb/latest/developerguide/SettingUp.DynamoWebService.html,
        *
        *  If you are creating a client for DynamoDBLocal (for testing purposes),
        *  DynamoDB-Local should be started first. For most simple testing, you can keep
        *  data in memory only, without writing anything to disk.  To do this, use the
        *  following command line:
        *
        *    java -Djava.library.path=./DynamoDBLocal_lib -jar DynamoDBLocal.jar -inMemory
        *
        *  For information about DynamoDBLocal, see:
        *  https://docs.aws.amazon.com/amazondynamodb/latest/developerguide/DynamoDBLocal.html.
        *-----------------------------------------------------------------------------------*/

        // So we know whether local DynamoDB is running
        private static readonly string Ip = "localhost";
        private static readonly int Port = 8000;
        private static readonly string EndpointUrl = "http://" + Ip + ":" + Port;

        public static AmazonDynamoDBClient Client { get; set; }

        private static bool IsPortInUse()
        {
            var isAvailable = true;

            // Evaluate current system TCP connections. This is the same information provided
            // by the netstat command line application, just in .Net strongly-typed object
            // form.  We will look through the list, and if our port we would like to use
            // in our TcpClient is occupied, we will set isAvailable to false.
            var ipGlobalProperties = IPGlobalProperties.GetIPGlobalProperties();
            var tcpConnInfoArray = ipGlobalProperties.GetActiveTcpListeners();

            foreach (var endpoint in tcpConnInfoArray)
                if (endpoint.Port == Port)
                {
                    isAvailable = false;
                    break;
                }

            return isAvailable;
        }

        public static bool createClient(bool useDynamoDbLocal)
        {
            if (useDynamoDbLocal)
            {
                // First, check to see whether anyone is listening on the DynamoDB local port
                // (by default, this is port 8000, so if you are using a different port, modify this accordingly)
                var portUsed = IsPortInUse();

                if (portUsed)
                {
                    Console.WriteLine("The local version of DynamoDB is NOT running.");
                    return false;
                }

                // DynamoDB-Local is running, so create a client
                Console.WriteLine("  -- Setting up a DynamoDB-Local client (DynamoDB Local seems to be running)");
                var ddbConfig = new AmazonDynamoDBConfig();
                ddbConfig.ServiceURL = EndpointUrl;
                try
                {
                    Client = new AmazonDynamoDBClient(ddbConfig);
                }
                catch (Exception ex)
                {
                    Console.WriteLine("     FAILED to create a DynamoDBLocal client; " + ex.Message);
                    return false;
                }
            }
            else
            {
                Client = new AmazonDynamoDBClient();
            }

            return true;
        }

        public static async Task<bool> CheckingTableExistence_async(string tblNm)
        {
            var response = await Client.ListTablesAsync();
            return response.TableNames.Contains(tblNm);
        }

        public static async Task<bool> CreateTable_async(string tableName,
            List<AttributeDefinition> tableAttributes,
            List<KeySchemaElement> tableKeySchema,
            ProvisionedThroughput provisionedThroughput)
        {
            var response = true;

            // Build the 'CreateTableRequest' structure for the new table
            var request = new CreateTableRequest
            {
                TableName = tableName,
                AttributeDefinitions = tableAttributes,
                KeySchema = tableKeySchema,
                // Provisioned-throughput settings are always required,
                // although the local test version of DynamoDB ignores them.
                ProvisionedThroughput = provisionedThroughput
            };

            try
            {
                var makeTbl = await Client.CreateTableAsync(request);
            }
            catch (Exception)
            {
                response = false;
            }

            return response;
        }

        public static async Task<TableDescription> GetTableDescription(string tableName)
        {
            TableDescription result = null;

            // If the table exists, get its description.
            try
            {
                var response = await Client.DescribeTableAsync(tableName);
                result = response.Table;
            }
            catch (Exception)
            {
            }

            return result;
        }

        public static async Task<bool> LoadingData_async(Table table, string filePath)
        {
            var movieArray = await ReadJsonMovieFile_async(filePath);

            if (movieArray != null)
                await LoadJsonMovieData_async(table, movieArray);

            return true;
        }

        public static async Task<JArray> ReadJsonMovieFile_async(string jsonMovieFilePath)
        {
            StreamReader sr = null;
            JsonTextReader jtr = null;
            JArray movieArray = null;

            Console.WriteLine("  -- Reading the movies data from a JSON file...");

            try
            {
                sr = new StreamReader(jsonMovieFilePath);
                jtr = new JsonTextReader(sr);
                movieArray = (JArray) await JToken.ReadFromAsync(jtr);
            }
            catch (Exception ex)
            {
                Console.WriteLine("     ERROR: could not read the file!\n          Reason: {0}.", ex.Message);
            }
            finally
            {
                jtr?.Close();
                sr?.Close();
            }

            return movieArray;
        }

        public static async Task<bool> LoadJsonMovieData_async(Table moviesTable, JArray moviesArray)
        {
            var n = moviesArray.Count;
            Console.Write("     -- Starting to load {0:#,##0} movie records into the Movies table asynchronously...\n" +
                          "" +
                          "        Wrote: ", n);
            for (int i = 0, j = 99; i < n; i++)
                try
                {
                    var itemJson = moviesArray[i].ToString();
                    var doc = Document.FromJson(itemJson);
                    Task putItem = moviesTable.PutItemAsync(doc);
                    if (i >= j)
                    {
                        j++;
                        Console.Write("{0,5:#,##0}, ", j);
                        if (j % 1000 == 0)
                            Console.Write("\n               ");
                        j += 99;
                    }

                    await putItem;
                }
                catch (Exception)
                {
                    return false;
                }

            return true;
        }
    }
}