public class MergeRequestDto
{
    public required string FileId { get; set; }
    public int TotalChunks { get; set; }
    public required string OutputFileName { get; set; }
}