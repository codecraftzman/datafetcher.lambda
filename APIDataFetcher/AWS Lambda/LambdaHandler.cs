using Amazon.Lambda.Core;
using Amazon.Lambda.S3Events;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;

public class LambdaHandler
{
    private readonly DataFetcher _dataFetcher;
    private readonly S3Uploader _s3Uploader;
    private readonly ILogger<LambdaHandler> _logger;

    public LambdaHandler(DataFetcher dataFetcher, S3Uploader s3Uploader, ILogger<LambdaHandler> logger)
    {
        _dataFetcher = dataFetcher;
        _s3Uploader = s3Uploader;
        _logger = logger;
    }

    public async Task HandleRequest(S3Event s3Event, ILambdaContext context)
    {
        try
        {
            // Example: Fetch data and upload to S3
            var data = await _dataFetcher.FetchDataInParallel(new List<string> { "endpoint1", "endpoint2" }, new Dictionary<string, string> { { "param", "value" } });
            using (var stream = new MemoryStream(System.Text.Encoding.UTF8.GetBytes(string.Join("\n", data))))
            {
                await _s3Uploader.UploadToS3Async("your-bucket-name", "your-key", stream);
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error occurred while handling the Lambda request");
            throw;
        }
    }
}
