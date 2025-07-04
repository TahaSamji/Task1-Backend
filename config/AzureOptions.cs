// Models/AzureBlobOptions.cs
namespace myapp_back.config
{
    public class AzureOptions
    {
        public required string ConnectionString { get; set; }
        public required string ContainerName { get; set; }
        public required string AccountName { get; set; }
        public required string AccountKey { get; set; }
    }
}