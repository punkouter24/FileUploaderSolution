using System;
using System.Data.SqlClient;
using System.IO;
using Azure.Storage.Blobs;
using Azure.Data.Tables;
using Microsoft.Extensions.Configuration;

namespace DataCopier
{
    class Program
    {
        static void Main(string[] args)
        {
            // Build configuration
            var builder = new ConfigurationBuilder()
                .AddUserSecrets<Program>();
            var configuration = builder.Build();

            // Get the connection string from secrets
            string storageConnectionString = configuration["AzureStorageConnectionString"];

            string connectionString = "Data Source=(localdb)\\ProjectModels;Initial Catalog=FileDataDb;Integrated Security=True;Connect Timeout=30;Encrypt=False;Trust Server Certificate=False;Application Intent=ReadWrite;Multi Subnet Failover=False";
            string tableName = "FilesTable";
            string blobContainerName = "fileuploads";

            var serviceClient = new TableServiceClient(storageConnectionString);
            var tableClient = serviceClient.GetTableClient(tableName);
            var blobServiceClient = new BlobServiceClient(storageConnectionString);
            var blobContainerClient = blobServiceClient.GetBlobContainerClient(blobContainerName);


            tableClient.CreateIfNotExists();
            blobContainerClient.CreateIfNotExists();

            try
            {
                using (SqlConnection connection = new SqlConnection(connectionString))
                {
                    connection.Open();

                    string query = "SELECT Id, FileName, FileContent FROM Files";
                    using (SqlCommand command = new SqlCommand(query, connection))
                    using (SqlDataReader reader = command.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            int id = reader.GetInt32(0);
                            string fileName = reader.GetString(1);
                            byte[] fileContent = (byte[])reader.GetValue(2);

                            // Upload file to Azure Blob Storage
                            string blobName = $"{id}_{fileName}";
                            BlobClient blobClient = blobContainerClient.GetBlobClient(blobName);
                            using (var stream = new MemoryStream(fileContent))
                            {
                                blobClient.Upload(stream);
                            }

                            // Get the blob URL
                            string blobUrl = blobClient.Uri.ToString();

                            // Save metadata and blob URL to Azure Table Storage
                            var fileEntity = new FileEntity
                            {
                                PartitionKey = "FilePartition",
                                RowKey = id.ToString(),
                                FileName = fileName,
                                BlobUrl = blobUrl
                            };

                            tableClient.AddEntity(fileEntity);
                        }
                    }
                }

                Console.WriteLine("Data copied to Azure Blob Storage and Table Storage successfully.");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"An error occurred: {ex.Message}");
            }
        }
    }
}
