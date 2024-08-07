using Azure;
using Azure.Data.Tables;

public class FileEntity : ITableEntity
{
    public string PartitionKey { get; set; }
    public string RowKey { get; set; }
    public string FileName { get; set; }
    public string BlobUrl { get; set; }  // URL to the blob storage
    public DateTimeOffset? Timestamp { get; set; }
    public ETag ETag { get; set; }
}
