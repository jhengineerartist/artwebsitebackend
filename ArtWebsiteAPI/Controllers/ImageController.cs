using Azure.Storage.Blobs;
using Microsoft.AspNetCore.Mvc;
using ArtWebsiteDataAccess.Models;
using ArtWebsiteDataAccess;

namespace ArtWebsiteAPI.Controllers;
[Route("api/[controller]")]
[ApiController]
public class ImageController : Controller
{
    private readonly ILogger<ImageController> logger;
    private readonly BlobServiceClient blobServiceClient; // The blob service client instance
    private readonly ImageDbContext dbContext; // The DbContext instance

    public ImageController(ILogger<ImageController> logger, ImageDbContext dbContext, BlobServiceClient blobServiceClient)
    {
        this.logger = logger;
        this.dbContext = dbContext; // Assign the DbContext instance
        this.blobServiceClient = blobServiceClient; // Assign the blob service client instance
    }

    // GET api/image/upload
    [HttpGet("upload")]
    public IActionResult Upload()
    {
        // Return the view
        return View();
    }

    // POST api/image
    [HttpPost]
    public async Task<IActionResult> Post([FromForm] IFormFile file, [FromForm] Image image)
    {
        // Check if the image parameter is null or invalid
        if (image == null || !ModelState.IsValid || file == null || file.Length == 0 || !file.ContentType.StartsWith("image/"))
        {
            logger.LogWarning("Invalid image data: {image} | {file}", image, file);
            return BadRequest("Invalid image data");
        }

        try
        {
            // Get a reference to the container using the blob service client
            logger.LogDebug("Getting reference to container");
            var container = blobServiceClient.GetBlobContainerClient("apiimages");
            await container.CreateIfNotExistsAsync();
            await container.SetAccessPolicyAsync(Azure.Storage.Blobs.Models.PublicAccessType.Blob);

            // Get a reference to a new blob with a unique name using the blob service client
            var blob = container.GetBlobClient(Guid.NewGuid().ToString() + Path.GetExtension(image.Title));

            logger.LogDebug("Uploading image file {FileName} to container {BlobContainerName}", file.FileName, blob.BlobContainerName);

            // Upload the file stream to the blob
            using (var stream = file.OpenReadStream())
            {
                await blob.UploadAsync(stream);
            }
            logger.LogDebug("Getting image URL from the blob container");
            // Set the URL property of the image parameter to the blob URL
            image.Url = blob.Uri.ToString();
            logger.LogDebug("Got image URL {Url}", image.Url);

            // Save the image entity to the database using the DbContext class

            logger.LogDebug("Saving image entity via dbContext");
            dbContext.Images.Add(image);
            await dbContext.SaveChangesAsync();
            logger.LogInformation("Image uploaded successfully with ID {Id}", image.Id);

            // Return the image entity as a response
            return Ok(image);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Image upload failed: {Message} {StackTrace}", ex.Message, ex.StackTrace);
            // Handle any errors that may occur
            return StatusCode(500, ex.Message);
        }
    }
}
