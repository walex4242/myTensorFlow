using Microsoft.AspNetCore.Mvc;
using System.Diagnostics;
using System.IO;

[ApiController]
[Route("[controller]")]
public class ExtractionController : ControllerBase
{
    private readonly TextProcessingService _textProcessingService;
    private readonly BertService _bertService;
    private readonly SummarizationService _summarizationService;

    public ExtractionController(TextProcessingService textProcessingService, BertService bertService, SummarizationService summarizationService)
    {
        _textProcessingService = textProcessingService;
        _bertService = bertService;
        _summarizationService = summarizationService;
    }

    [HttpPost("extract/summarize")]
    public async Task<IActionResult> ExtractAndSummarizeDocument([FromForm] IFormFile file)
    {
        if (file == null || file.Length == 0)
        {
            return BadRequest("Please upload a file.");
        }

        var fileName = file.FileName.ToLowerInvariant();
        var tempFilePath = Path.GetTempFileName();
        string rawText = null;
        string embeddingsFilePath = Path.Combine(Directory.GetCurrentDirectory(), "embeddings.json");
        string pythonScriptPath = @"C:\Users\OLAWALE\Desktop\myTensorFlow\Tensorflow\python\test.py"; // Replace with your actual path

        try
        {
            using (var stream = new FileStream(tempFilePath, FileMode.Create))
            {
                await file.CopyToAsync(stream);
            }

            if (fileName.EndsWith(".pdf"))
            {
                rawText = DocumentTextExtractor.ExtractTextFromPdf(tempFilePath);
            }
            else if (fileName.EndsWith(".docx") || fileName.EndsWith(".doc"))
            {
                rawText = DocumentTextExtractor.ExtractTextFromDocx(tempFilePath);
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

            // Run Python script to generate embeddings
            ProcessStartInfo start = new ProcessStartInfo();
            start.FileName = "python";
            start.Arguments = $"{pythonScriptPath} \"{Path.GetTempFileName()}\" \"{embeddingsFilePath}\""; // Create a temp file for Python to read
            start.UseShellExecute = false;
            start.RedirectStandardOutput = true;
            start.RedirectStandardError = true;
            start.CreateNoWindow = true;

            // Write the raw text to the temporary file for the Python script
            await System.IO.File.WriteAllTextAsync(start.Arguments.Split('"')[1], rawText, System.Text.Encoding.UTF8);

            using (Process process = new Process())
            {
                process.StartInfo = start;
                process.Start();
                string output = await process.StandardOutput.ReadToEndAsync();
                string error = await process.StandardError.ReadToEndAsync();
                await process.WaitForExitAsync();

                if (process.ExitCode != 0)
                {
                    Console.WriteLine($"Python script error: {error}");
                    return StatusCode(500, $"Failed to generate embeddings.");
                }
                Console.WriteLine($"Python script output: {output}");

                // Load embeddings and generate summary
                _bertService.LoadEmbeddings(embeddingsFilePath);
                string summary = _summarizationService.GenerateSummary(rawText);

                return Ok(new { RawText = rawText, ProcessedText = processedText, Summary = summary });
            }
        }
        finally
        {
            if (System.IO.File.Exists(tempFilePath))
            {
                System.IO.File.Delete(tempFilePath);
            }
            if (System.IO.File.Exists(embeddingsFilePath))
            {
                System.IO.File.Delete(embeddingsFilePath);
            }
        }
    }
}