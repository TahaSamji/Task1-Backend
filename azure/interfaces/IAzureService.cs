// Interface update - IAzureService.cs
namespace myapp_back.azure.interfaces
{
    public interface IAzureService
    {
        Task<string> GenerateUploadSasUriAsync(string filename);
        Task MergeChunksAsync(string fileId, int totalChunks, string outputFileName);
    }
}