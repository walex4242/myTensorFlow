using System.Text;
using UglyToad.PdfPig;
using UglyToad.PdfPig.Content;
using DocumentFormat.OpenXml.Packaging;

public class DocumentTextExtractor
{
    public static string ExtractTextFromPdf(string pdfFilePath)
    {
        StringBuilder text = new StringBuilder();
        try
        {
            using PdfDocument document = PdfDocument.Open(pdfFilePath);
            foreach (Page page in document.GetPages())
            {
                foreach (Word word in page.GetWords())
                {
                    text.Append(word.Text);
                    text.Append(" "); // Add space between words
                }
                text.AppendLine(); // Add new line after each page (optional)
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error extracting text from PDF: {ex.Message}");
            return null;
        }
        return text.ToString().Trim();
    }

    public static string ExtractTextFromDocx(string docxFilePath)
    {
        StringBuilder text = new();
        try
        {
            using WordprocessingDocument wordDoc = WordprocessingDocument.Open(docxFilePath, false);
            if (wordDoc.MainDocumentPart != null)
            {
                text.AppendLine(wordDoc.MainDocumentPart.Document.Body.InnerText);
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error extracting text from DOCX: {ex.Message}");
            return null;
        }
        return text.ToString().Trim();
    }
}