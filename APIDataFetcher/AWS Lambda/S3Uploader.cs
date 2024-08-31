using Amazon.S3;
using Amazon.S3.Transfer;
using System.IO;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;

public class S3Uploader
{
    private readonly IAmazonS3 _s3Client;
    private readonly ILogger<S3Uploader> _logger;

    public S3Uploader(IAmazonS3 s3Client, ILogger<S3Uploader> logger)
    {
        _s3Client = s3Client;
        _logger = logger;
    }

    public async Task UploadToS3Async(string bucketName, string key, Stream dataStream)
    {
        try
        {
            var fileTransferUtility = new TransferUtility(_s3Client);
            await fileTransferUtility.UploadAsync(dataStream, bucketName, key);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error occurred while uploading to S3 bucket {BucketName}", bucketName);
            throw;
        }
    }
}
