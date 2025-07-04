// Controllers/AzureController.cs
using Microsoft.AspNetCore.Mvc;
using myapp_back.azure.interfaces;

namespace myapp_back.azure.controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class AzureController : ControllerBase
    {
        private readonly IAzureService _azureService;


        public AzureController(IAzureService azureService)
        {
            _azureService = azureService;
        }

        [HttpGet("sas-url")]
        public async Task<IActionResult> GetSasUrl([FromQuery] string fileName)
        {
            var sasUrl = await _azureService.GenerateUploadSasUriAsync(fileName);
            Console.WriteLine(sasUrl);

            return Ok(sasUrl);
        }

        [HttpPost("merge")]
        public async Task<IActionResult> MergeChunks([FromBody] MergeRequestDto dto)
        {
            await _azureService.MergeChunksAsync(dto.FileId, dto.TotalChunks, dto.OutputFileName);
        return Ok(new { message = " Merge completed." });
        }
    }

}
