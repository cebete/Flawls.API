using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Flawls.API.Controllers;

[ApiController]
[Route("api/images")]
[Authorize]
public class ImagesController : ControllerBase
{
    private readonly string _uploadPath;

    public ImagesController(IWebHostEnvironment env)
    {
        _uploadPath = Path.Combine(env.ContentRootPath, "uploads");
        Directory.CreateDirectory(_uploadPath);
    }

    [HttpPost]
    public async Task<ActionResult> Upload(IFormFile file)
    {
        if (file is null || file.Length == 0)
            return BadRequest(new { message = "No file provided." });

        var allowed = new[] { ".jpg", ".jpeg", ".png", ".webp" };
        var ext = Path.GetExtension(file.FileName).ToLowerInvariant();
        if (!allowed.Contains(ext))
            return BadRequest(new { message = "Only jpg, png and webp are allowed." });

        var fileName = $"{Guid.NewGuid()}{ext}";
        var filePath = Path.Combine(_uploadPath, fileName);

        using var stream = System.IO.File.Create(filePath);
        await file.CopyToAsync(stream);

        return Ok(new { url = $"/uploads/{fileName}" });
    }
}