
namespace MyTensorFlowApp.Services
{
    public class TrainingBackgroundService(IServiceProvider serviceProvider) : BackgroundService
    {
        private readonly IServiceProvider _serviceProvider = serviceProvider;

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            using (var scope = _serviceProvider.CreateScope())
            {
                var model = scope.ServiceProvider.GetRequiredService<SimpleLinearModel>();
                Console.WriteLine("Starting Simple Linear Regression Training in Background...");
                await Task.Run(() => model.Train(), stoppingToken);
                Console.WriteLine("Background training completed.");
                // Potentially store the trained model somewhere accessible
            }
        }
    }
}