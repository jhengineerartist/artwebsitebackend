using Azure.Storage.Blobs;
using Microsoft.AspNetCore.Mvc;
using ArtWebsiteDataAccess.Models;
using ArtWebsiteDataAccess;
using Microsoft.EntityFrameworkCore;

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

    [HttpGet]
    public async Task<IActionResult> Get()
    {
        // Get all the image entities from the database using the DbContext class
        var images = await dbContext.Images.ToListAsync();
        logger.LogInformation("Retrieved {Count} images from the database", images.Count);

        // Return the image array as a response
        return Ok(images);
    }

    [HttpDelete]
    public async Task<IActionResult> Delete(int id)
    {
        try
        {
            // Find the image entity by id using the DbContext class
            var image = await dbContext.Images.FindAsync(id);
            if (image == null)
            {
                logger.LogWarning("Image not found with id: {id}", id);
                return NotFound("Image not found");
            }

            // Get a reference to the container using the blob service client
            logger.LogDebug("Getting reference to container");
            var container = blobServiceClient.GetBlobContainerClient("apiimages");

            // Get the blob name from the image URL
            var blobname = Path.GetFileName(image.Url);

            // Delete the blob from the container
            var response = await container.DeleteBlobAsync(blobname);
            logger.LogInformation("Blob {blobname} deleted successfully with {response}", blobname, response);

            // Delete the image entity from the database using the DbContext class
            dbContext.Images.Remove(image);
            await dbContext.SaveChangesAsync();
            logger.LogInformation("Image deleted successfully with ID {Id}", image.Id);

            return Ok($"Image with ID {id} deleted successfully");
        }
        catch (Azure.RequestFailedException ex)
        {
            // Log or display the error details
            logger.LogError(ex, "Blob client creation failed: {ErrorCode} {Message} {Status}", ex.ErrorCode, ex.Message, ex.Status);
            return StatusCode(500, ex.Message);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Image deletion failure: {ErrorCode} {Message} {Status}", ex.Message);
            // Handle any errors that may occur
            return StatusCode(500, ex.Message);
        }

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

            var blobname = Guid.NewGuid().ToString() + Path.GetExtension(file.FileName); ;

            // Upload the file stream to the blob
            using (var stream = file.OpenReadStream())
            {
                var response = await container.UploadBlobAsync(blobname, stream);
                logger.LogInformation("Blob {blobname} uploaded successfully with {response}", blobname, response);
            }

            logger.LogDebug("Getting image URL from the blob container");
            // Set the URL property of the image parameter to the blob URL
            var blob = container.GetBlobClient(blobname);
            image.Url = blob.Uri.ToString();
            logger.LogDebug("Got image URL {Url}", image.Url);

            // Save the image entity to the database using the DbContext class

            dbContext.Images.Add(image);
            await dbContext.SaveChangesAsync();
            logger.LogInformation("Image uploaded successfully with ID {Id}", image.Id);

            // Return the image entity as a response
            return Ok(image);
        }
        catch (Azure.RequestFailedException ex)
        {
            // Log or display the error details
            logger.LogError(ex, "Blob client creation failed: {ErrorCode} {Message} {Status}", ex.ErrorCode, ex.Message, ex.Status);
            return StatusCode(500, ex.Message);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Image upload failure: {ErrorCode} {Message} {Status}", ex.Message);
            // Handle any errors that may occur
            return StatusCode(500, ex.Message);
        }
    }
}
