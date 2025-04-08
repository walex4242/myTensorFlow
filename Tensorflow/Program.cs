
var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddScoped<TextProcessingService>();

// Add BertService
builder.Services.AddSingleton(provider =>
{
    var environment = provider.GetRequiredService<IWebHostEnvironment>();
    var embeddingsFilePath = @"C:\Users\OLAWALE\Desktop\myTensorFlow\python\embeddings.json";

    var bertService = new BertService();
    bertService.LoadEmbeddings(embeddingsFilePath);
    return bertService;
});

// Add SummarizationService
builder.Services.AddScoped<SummarizationService>();

var app = builder.Build();

app.UseAuthorization();

app.MapControllers();

app.Run();