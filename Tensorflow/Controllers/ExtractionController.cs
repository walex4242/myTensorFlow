using Microsoft.AspNetCore.Mvc;

[ApiController]
[Route("[controller]")]
public class ExtractionController(TextProcessingService textProcessingService) : ControllerBase
{
    private readonly TextProcessingService _textProcessingService = textProcessingService;

    [HttpPost("extract/processed")]
    public async Task<IActionResult> ExtractAndProcessDocument([FromForm] IFormFile file)
    {
        if (file == null || file.Length == 0)
        {
            return BadRequest("Please upload a file.");
        }

        var fileName = file.FileName.ToLowerInvariant();
        var filePath = Path.GetTempFileName();
        string rawText = null;

        try
        {
            using (var stream = new FileStream(filePath, FileMode.Create))
            {
                await file.CopyToAsync(stream);
            }

            if (fileName.EndsWith(".pdf"))
            {
                rawText = DocumentTextExtractor.ExtractTextFromPdf(filePath);
            }
            else if (fileName.EndsWith(".docx") || fileName.EndsWith(".doc"))
            {
                rawText = DocumentTextExtractor.ExtractTextFromDocx(filePath);
            }
            else
            {
                return BadRequest("Unsupported file type. Please upload a PDF or DOCX file.");
            }

            if (rawText == null)
            {
                return StatusCode(500, $"Failed to extract text from {Path.GetExtension(fileName)} file.");
            }

            string processedText = _textProcessingService.PreprocessText(rawText);

            return Ok(new { RawText = rawText, ProcessedText = processedText });
        }
        finally
        {
            if (System.IO.File.Exists(filePath))
            {
                System.IO.File.Delete(filePath);
            }
        }
    }
}